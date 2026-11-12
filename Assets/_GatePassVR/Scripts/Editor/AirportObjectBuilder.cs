// Dest_ 지점 기준으로 임시 오브젝트(여권·캐리어·탑승권·트레이·가방)와 기능 영역 표시를 실제 크기로 배치하는 Editor 도구
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace GatePassVR.EditorTools
{
    public static class AirportObjectBuilder
    {
        const string DestRootName = "DestinationPoints";
        const string RootName = "TempObjects";
        const string MaterialFolder = "Assets/_GatePassVR/Art/Temp";
        // 표면을 찾는 Ray 시작 높이(바닥 기준). 2.0m에 걸린 이동 표지판 Collider(아래 끝 약 1.78m)에 걸리지 않게 그보다 낮게 둔다.
        const float SurfaceProbeHeight = 1.6f;

        enum Kind
        {
            Grab,   // 집는 물건: Box Collider + Rigidbody + XRGrabInteractable(임시, 동작 확정은 김씨)
            Zone,   // 기능 영역 표시: 반투명 판 + Trigger Box Collider (Placement·Scanner·Hand-over가 붙을 자리)
            Stand,  // 임시 받침대: 고정 Box Collider
            Npc,    // 정적 NPC(애니메이션 없음): 캡슐 몸통 + 구 머리 + 역할 이름표 + 몸 Collider, 서는 지점을 바라봄
                    // 몸 Collider가 있어야 표지판 도구·점검 도구가 NPC 뒤에 표지판을 두지 않는다(처음엔 없어서 입국심사관이 이동 표지판을 가렸다).
        }

        // 정적 NPC 역할(표의 이름 → 역할 ID, 화면 이름, 상호작용 지점 이름). 지점 이름은 같은 지점의 Zone 표 이름(접미사 제외).
        static readonly Dictionary<string, (string roleId, string displayName, string zone)> NpcRoles =
            new Dictionary<string, (string, string, string)>
            {
                { "Npc_CheckInStaff", ("CheckInStaff", "체크인 직원", "Zone_HandOver") },
                { "Npc_SecurityOfficer", ("SecurityOfficer", "보안검색 요원", null) },
                { "Npc_BoardingStaff", ("BoardingStaff", "탑승구 직원", "Zone_Scan") },
                { "Npc_ImmigrationOfficer", ("ImmigrationOfficer", "입국심사관", "Zone_HandOver") },
            };

        // NPC 모양(실제 사람 키 약 1.7m). 실제 사람 에셋은 라이선스 확인 뒤 교체한다.
        const float NpcBodyHeight = 1.4f;
        const float NpcBodyWidth = 0.45f;
        const float NpcBodyDepth = 0.3f;
        const float NpcHeadSize = 0.24f;
        const float NpcLabelHeight = 1.95f;

        // 같은 색은 Material 하나를 공유한다(§24 Material 복제 금지).
        static readonly Dictionary<string, Color> Colors = new Dictionary<string, Color>
        {
            { "Passport", new Color(0.45f, 0.08f, 0.1f) },
            { "Paper", new Color(0.95f, 0.95f, 0.92f) },
            { "Luggage", new Color(0.95f, 0.5f, 0.1f) },
            // 트레이는 어두운 X-ray 벨트 위에서 눈에 띄도록 밝은 회색.
            { "Tray", new Color(0.78f, 0.8f, 0.84f) },
            { "Stand", new Color(0.6f, 0.6f, 0.6f) },
            { "Laptop", new Color(0.22f, 0.24f, 0.28f) },
            // 들고 타는 가방은 맡기는 캐리어(주황)와 헷갈리지 않게 청록색.
            { "Bag", new Color(0.1f, 0.55f, 0.6f) },
            { "Zone", new Color(0.1f, 0.85f, 0.35f, 0.35f) },
            // NPC 제복: 역할마다 다른 색으로 구별한다.
            { "UniformCheckIn", new Color(0.12f, 0.2f, 0.45f) },
            { "UniformSecurity", new Color(0.12f, 0.12f, 0.14f) },
            { "UniformBoarding", new Color(0.55f, 0.12f, 0.15f) },
            { "UniformImmigration", new Color(0.2f, 0.38f, 0.32f) },
            { "Skin", new Color(0.86f, 0.7f, 0.58f) },
            { "NpcLabel", new Color(0.1f, 0.12f, 0.16f) },
        };

        // (서 있는 지점, 이름, 종류, 크기 m (가로 X, 높이 Y, 앞뒤 Z), 지점 기준 오른쪽 m, 앞 m, 지점 기준 Y 회전, 색)
        // 높이는 지정 위치 아래의 표면(바닥·카운터·벨트)을 Ray로 찾아 그 위에 올린다. 표만 고치고 다시 실행하면 된다.
        static readonly (string from, string name, Kind kind, Vector3 size, float right, float forward, float yaw, string color)[] Items =
        {
            // 여권: 처음부터 손에 들고 시작할 예정(김씨 Grab 영역). 그 전까지 Start 앞 임시 받침대 위에 둔다.
            ("Dest_Start", "Stand_Passport", Kind.Stand, new Vector3(0.4f, 0.9f, 0.4f), 0.3f, 0.6f, 0f, "Stand"),
            ("Dest_Start", "Obj_Passport", Kind.Grab, new Vector3(0.088f, 0.005f, 0.125f), 0.3f, 0.6f, 0f, "Passport"),
            // 체크인: 캐리어는 플레이어 왼쪽 바닥, 탑승권은 카운터 앞쪽 끝, 수하물 올리는 자리는 카운터 왼쪽 벨트 위.
            ("Dest_CheckIn", "Obj_Carrier", Kind.Grab, new Vector3(0.4f, 0.6f, 0.25f), -0.55f, 0.15f, 0f, "Luggage"),
            ("Dest_CheckIn", "Obj_BoardingPass", Kind.Grab, new Vector3(0.08f, 0.002f, 0.2f), -0.45f, 0.95f, 0f, "Paper"),
            ("Dest_CheckIn", "Zone_BaggageDrop", Kind.Zone, new Vector3(0.7f, 0.02f, 0.6f), -1.8f, 1.35f, 0f, "Zone"),
            // 체크인 직원은 카운터(앞뒤 0.9~1.5m) 뒤 바닥에 서고, 그 앞 카운터 위에 여권 건네는 자리를 둔다.
            // 카운터 위 모니터가 오른쪽 앞(0~+0.5m, 앞 1.1~1.35m)을 차지해서 그보다 왼쪽에 두고, 탑승권은 더 왼쪽(-0.45m)으로 옮겼다.
            // 직원도 여권 건네는 자리 바로 뒤에 서게 해서 "누구에게 건네는지" 보이게 한다.
            ("Dest_CheckIn", "Zone_HandOver", Kind.Zone, new Vector3(0.3f, 0.02f, 0.22f), -0.2f, 1.05f, 0f, "Zone"),
            // 직원을 -0.2m에 세우면 '보안검색' 이동 표지판(-20°)의 아래 모서리를 가려서 0으로 둔다.
            ("Dest_CheckIn", "Npc_CheckInStaff", Kind.Npc, new Vector3(NpcBodyWidth, 1.7f, NpcBodyDepth), 0f, 2.0f, 0f, "UniformCheckIn"),
            // 보안검색: 노트북(벨트 입구 오른쪽 빈 곳)과 가방(플레이어 오른쪽 바닥)을 집어 트레이에 넣는다(guide-texts S3-1, S3-2).
            // 오른쪽에 받침대를 세우면 이동 표지판을 가려서, 받침대 없이 벨트 빈 곳과 바닥을 쓴다.
            // 트레이는 고정(집지 않음)하고, 긴 쪽(60cm)을 벨트 가로로 두어 왼쪽 반은 노트북, 오른쪽 반은 가방 자리로 쓴다.
            // 가로로 두면 두 자리 모두 서는 지점에서 1m 안에 들어온다(세로로 두면 뒤쪽 칸이 1m를 넘음).
            ("Dest_Security", "Obj_SecurityTray", Kind.Stand, new Vector3(0.6f, 0.08f, 0.4f), -0.375f, 0.95f, 0f, "Tray"),
            ("Dest_Security", "Zone_TrayLaptop", Kind.Zone, new Vector3(0.28f, 0.02f, 0.38f), -0.525f, 0.95f, 0f, "Zone"),
            ("Dest_Security", "Zone_TrayBag", Kind.Zone, new Vector3(0.28f, 0.02f, 0.38f), -0.225f, 0.95f, 0f, "Zone"),
            ("Dest_Security", "Obj_Laptop", Kind.Grab, new Vector3(0.22f, 0.02f, 0.3f), 0.15f, 0.88f, 0f, "Laptop"),
            ("Dest_Security", "Obj_Bag", Kind.Grab, new Vector3(0.28f, 0.38f, 0.15f), 0.55f, 0.2f, 0f, "Bag"),
            // 보안검색 요원은 벨트 왼쪽에 선다(오른쪽은 '탑승구' 이동 표지판으로 가는 시선을 가림).
            ("Dest_Security", "Npc_SecurityOfficer", Kind.Npc, new Vector3(NpcBodyWidth, 1.7f, NpcBodyDepth), -1.1f, 1.6f, 0f, "UniformSecurity"),
            // 탑승 게이트: 부스 책상 위 여권·탑승권 스캔 자리. 책상 앞 끝이 지점에서 약 0.98m라 앞 끝에 붙여 1m 안에 둔다.
            ("Dest_Boarding", "Zone_Scan", Kind.Zone, new Vector3(0.3f, 0.02f, 0.25f), -0.3f, 1.1f, 0f, "Zone"),
            // 부스 직원·심사관은 책상(앞뒤 1.2~1.5m) 뒤 부스 안 바닥에 선다.
            ("Dest_Boarding", "Npc_BoardingStaff", Kind.Npc, new Vector3(NpcBodyWidth, 1.7f, NpcBodyDepth), -0.45f, 2.0f, 0f, "UniformBoarding"),
            // 입국심사: 부스 책상 위 여권 건네는 자리(Boarding과 같은 부스 줄, 같은 기준).
            ("Dest_Immigration", "Zone_HandOver", Kind.Zone, new Vector3(0.3f, 0.02f, 0.25f), -0.3f, 1.1f, 0f, "Zone"),
            ("Dest_Immigration", "Npc_ImmigrationOfficer", Kind.Npc, new Vector3(NpcBodyWidth, 1.7f, NpcBodyDepth), -0.45f, 2.0f, 0f, "UniformImmigration"),
            // 수하물 수취: 컨베이어 가장자리에 눕힌 가방(서는 지점에서 1m 안).
            ("Dest_Baggage", "Obj_BaggageBag", Kind.Grab, new Vector3(0.4f, 0.25f, 0.6f), -0.5f, 0.55f, 0f, "Luggage"),
        };

        // Placement Zone 기능을 붙일 영역 표시(이름 → 받는 물건 이름). 이름은 지점 접미사(_CheckIn 등)를 뺀 표의 이름이다.
        // 받는 물건 이름은 같은 지점의 물건(접미사 자동)이고, 다른 지점의 물건은 접미사까지 쓴다(예: Obj_Passport_Start).
        // 스캔 자리는 김씨 Scanner가 붙을 자리라 넣지 않는다.
        static readonly Dictionary<string, string[]> PlacementZones = new Dictionary<string, string[]>
        {
            { "Zone_BaggageDrop", new[] { "Obj_Carrier" } },
            { "Zone_TrayLaptop", new[] { "Obj_Laptop" } },
            { "Zone_TrayBag", new[] { "Obj_Bag" } },
            // 여권 건네는 자리: 김씨 Hand-over가 생기기 전까지 Placement Zone으로 임시 동작(여권을 초록 칸에 놓으면 성공).
            { "Zone_HandOver", new[] { "Obj_Passport_Start" } },
        };

        // 성공해도 물건을 고정하지 않는 영역. 건넨 여권은 돌려받아(다시 집어) 다음 단계에서 또 쓰기 때문이다.
        static readonly HashSet<string> NoLockZones = new HashSet<string> { "Zone_HandOver" };

        // 임시 오브젝트와 겹쳐서 헷갈리게 만드는 공항 모델의 장식(Collider 없음). Scene에서만 끈다(모델 원본은 그대로).
        static readonly (string path, string reason)[] HiddenDecorations =
        {
            ("17.8.Model.002", "보안검색 트레이 위에 겹친 장식 배낭. 트레이에 넣는 '가방'과 헷갈림"),
        };

        // 기존 TempObjects를 지우고 새로 만든다. Undo(Ctrl+Z)로 되돌릴 수 있다.
        [MenuItem("GatePass/Objects/Build Temp Objects")]
        static void Build()
        {
            var destRoot = GameObject.Find(DestRootName);
            if (destRoot == null)
            {
                Debug.LogError($"[AirportObjectBuilder] '{DestRootName}' 오브젝트를 찾을 수 없습니다. Airport_Lee Scene을 열어 주세요.");
                return;
            }

            var old = GameObject.Find(RootName);
            if (old != null)
            {
                Undo.DestroyObjectImmediate(old);
            }
            var root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, $"Build {RootName}");

            var materials = new Dictionary<string, Material>();
            int created = 0;
            foreach (var item in Items)
            {
                var dest = destRoot.transform.Find(item.from);
                if (dest == null)
                {
                    Debug.LogError($"[AirportObjectBuilder] '{item.from}' 지점을 찾을 수 없습니다.");
                    continue;
                }
                if (!materials.TryGetValue(item.color, out var material))
                {
                    material = GetOrCreateMaterial(item.color, item.kind == Kind.Zone);
                    materials[item.color] = material;
                }

                // 앞서 만든 받침대 위에 올라가도록 매번 Collider 위치를 갱신한다.
                Physics.SyncTransforms();
                var point = dest.TransformPoint(new Vector3(item.right, 0f, item.forward));
                // 가운데와 네 모서리 아래 표면 중 가장 높은 곳에 올린다.
                // 가운데 한 점만 재면 X-ray 벨트 롤러 사이 틈에 걸려 물건이 벨트에 박힌 채 기울어진다.
                var rotation = Quaternion.Euler(0f, dest.eulerAngles.y + item.yaw, 0f);
                var halfX = rotation * Vector3.right * (item.size.x / 2f);
                var halfZ = rotation * Vector3.forward * (item.size.z / 2f);
                // NPC는 발밑 한 점만 잰다(카운터 바로 뒤에 서므로 몸 둘레를 재면 카운터 위로 올라감).
                var footprint = item.kind == Kind.Npc
                    ? new[] { point }
                    : new[] { point, point + halfX + halfZ, point + halfX - halfZ, point - halfX + halfZ, point - halfX - halfZ };
                float surface = float.NegativeInfinity;
                string surfaceName = null;
                foreach (var probe in footprint)
                {
                    if (Physics.Raycast(probe + Vector3.up * SurfaceProbeHeight, Vector3.down, out var hit,
                            SurfaceProbeHeight + 0.5f, ~0, QueryTriggerInteraction.Ignore) && hit.point.y > surface)
                    {
                        surface = hit.point.y;
                        surfaceName = hit.collider.name;
                    }
                }
                if (surfaceName == null)
                {
                    surface = dest.position.y;
                    surfaceName = "지점 바닥 높이(표면 없음)";
                    Debug.LogWarning($"[AirportObjectBuilder] {item.name}: 아래에 표면 Collider가 없어 지점 바닥 높이에 둡니다.");
                }

                if (item.kind == Kind.Npc)
                {
                    var npcObject = CreateNpc(item.name, root.transform, dest, new Vector3(point.x, surface, point.z), material,
                        materials);
                    Debug.Log($"[AirportObjectBuilder] {npcObject.name}: {surfaceName} 위 (바닥에서 {surface - dest.position.y:0.00}m)");
                    created++;
                    continue;
                }

                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"{item.name}_{dest.name.Replace("Dest_", "")}";
                go.transform.SetParent(root.transform, false);
                // 바닥에 박히지 않게 표면에서 5mm 띄운다.
                go.transform.position = new Vector3(point.x, surface + item.size.y / 2f + 0.005f, point.z);
                go.transform.rotation = rotation;
                go.transform.localScale = item.size;
                go.GetComponent<MeshRenderer>().sharedMaterial = material;

                var box = go.GetComponent<BoxCollider>();
                switch (item.kind)
                {
                    case Kind.Grab:
                        // 탑승권(두께 2mm)·여권처럼 얇은 물건은 떨어질 때 벨트·카운터 Collider를 뚫고 빠질 수 있어 연속 충돌 검사를 쓴다.
                        go.AddComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                        go.AddComponent<XRGrabInteractable>();
                        break;
                    case Kind.Zone:
                        box.isTrigger = true;
                        // 반투명 판은 그림자를 만들지 않는다.
                        go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        break;
                }

                Debug.Log($"[AirportObjectBuilder] {go.name}: {surfaceName} 위 (바닥에서 {surface - dest.position.y:0.00}m)");
                created++;
            }

            AttachPlacementZones(root.transform);
            LinkNpcStations(root.transform);
            HideDecorations();

            EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log($"[AirportObjectBuilder] {RootName} {created}개 생성. Scene을 저장해 주세요.", root);
        }

        // 정적 NPC: 캡슐 몸통 + 구 머리 + 머리 위 역할 이름표. 서는 지점(dest)을 바라본다. Collider는 모두 지운다.
        static GameObject CreateNpc(string baseName, Transform root, Transform dest, Vector3 feet, Material uniform,
            Dictionary<string, Material> materials)
        {
            var npc = new GameObject($"{baseName}_{dest.name.Replace("Dest_", "")}");
            npc.transform.SetParent(root, false);
            npc.transform.position = feet;
            var toPlayer = dest.position - feet;
            toPlayer.y = 0f;
            npc.transform.rotation = Quaternion.LookRotation(toPlayer);

            Material Get(string colorName)
            {
                if (!materials.TryGetValue(colorName, out var m))
                {
                    m = GetOrCreateMaterial(colorName, false);
                    materials[colorName] = m;
                }
                return m;
            }

            void Part(PrimitiveType type, string name, Vector3 localPosition, Vector3 scale, Material material)
            {
                var part = GameObject.CreatePrimitive(type);
                part.name = name;
                Object.DestroyImmediate(part.GetComponent<Collider>());
                part.transform.SetParent(npc.transform, false);
                part.transform.localPosition = localPosition;
                part.transform.localScale = scale;
                part.GetComponent<MeshRenderer>().sharedMaterial = material;
            }

            // Capsule 기본 높이 2m → y 배율 = 높이 / 2
            Part(PrimitiveType.Capsule, "Body", new Vector3(0f, NpcBodyHeight / 2f, 0f),
                new Vector3(NpcBodyWidth, NpcBodyHeight / 2f, NpcBodyDepth), uniform);
            Part(PrimitiveType.Sphere, "Head", new Vector3(0f, NpcBodyHeight + NpcHeadSize / 2f - 0.02f, 0f),
                Vector3.one * NpcHeadSize, Get("Skin"));

            // 몸 전체(머리 포함)를 감싸는 Collider 하나. 부분 Primitive의 Collider는 위에서 모두 지웠다.
            var bodyCollider = npc.AddComponent<CapsuleCollider>();
            float totalHeight = NpcBodyHeight + NpcHeadSize;
            bodyCollider.height = totalHeight;
            bodyCollider.radius = NpcBodyWidth / 2f;
            bodyCollider.center = new Vector3(0f, totalHeight / 2f, 0f);

            if (NpcRoles.TryGetValue(baseName, out var role))
            {
                // 이름표: 어두운 판 + 흰 글씨. 판의 -Z 면(글씨 쪽)이 플레이어를 향한다.
                var label = new GameObject("RoleLabel");
                label.transform.SetParent(npc.transform, false);
                label.transform.localPosition = new Vector3(0f, NpcLabelHeight, 0f);
                label.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                Part(PrimitiveType.Cube, "LabelBoard", new Vector3(0f, NpcLabelHeight, 0f), new Vector3(0.62f, 0.16f, 0.01f), Get("NpcLabel"));
                var text = label.AddComponent<TMPro.TextMeshPro>();
                text.font = AirportSignBuilder.GetOrUpdateSignFont(AllNpcNames());
                text.text = role.displayName;
                text.color = Color.white;
                text.alignment = TMPro.TextAlignmentOptions.Center;
                text.enableAutoSizing = true;
                text.fontSizeMin = 0.1f;
                text.fontSizeMax = 10f;
                text.rectTransform.sizeDelta = new Vector2(0.56f, 0.12f);
                // 판 앞(플레이어 쪽)으로 살짝 띄운다. label이 180° 돌아 있어 로컬 -Z가 NPC 앞쪽이다.
                label.transform.localPosition = new Vector3(0f, NpcLabelHeight, 0.01f);

                var station = npc.AddComponent<GatePassVR.Interaction.NpcStation>();
                var serialized = new SerializedObject(station);
                serialized.FindProperty("roleId").stringValue = role.roleId;
                serialized.FindProperty("displayName").stringValue = role.displayName;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            return npc;
        }

        static string AllNpcNames()
        {
            var all = new System.Text.StringBuilder();
            foreach (var role in NpcRoles.Values)
            {
                all.Append(role.displayName);
            }
            return all.ToString();
        }

        // NPC 역할표의 상호작용 지점(같은 지점의 Zone)을 NpcStation에 연결한다.
        static void LinkNpcStations(Transform root)
        {
            foreach (Transform child in root)
            {
                var station = child.GetComponent<GatePassVR.Interaction.NpcStation>();
                if (station == null)
                {
                    continue;
                }
                int split = child.name.LastIndexOf('_');
                string baseName = child.name.Substring(0, split);
                string suffix = child.name.Substring(split);
                if (!NpcRoles.TryGetValue(baseName, out var role) || string.IsNullOrEmpty(role.zone))
                {
                    continue;
                }
                var zone = root.Find(role.zone + suffix);
                if (zone == null)
                {
                    Debug.LogError($"[AirportObjectBuilder] {child.name}: 상호작용 지점 '{role.zone + suffix}'을 찾을 수 없습니다.");
                    continue;
                }
                var serialized = new SerializedObject(station);
                serialized.FindProperty("interactionPoint").objectReferenceValue = zone;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log($"[AirportObjectBuilder] {child.name}: 상호작용 지점 {zone.name} 연결");
            }
        }

        static void HideDecorations()
        {
            var model = GameObject.Find("Airport7_FINALc_EscenaMontada");
            if (model == null)
            {
                return;
            }
            foreach (var (path, reason) in HiddenDecorations)
            {
                var decoration = model.transform.Find(path);
                if (decoration == null)
                {
                    Debug.LogWarning($"[AirportObjectBuilder] 끌 장식 '{path}'을 찾을 수 없습니다.");
                    continue;
                }
                if (decoration.gameObject.activeSelf)
                {
                    Undo.RecordObject(decoration.gameObject, "Hide decoration");
                    decoration.gameObject.SetActive(false);
                    Debug.Log($"[AirportObjectBuilder] 장식 '{path}' 끔: {reason}");
                }
            }
        }

        // PlacementZones 표의 영역에 PlacementZone을 붙이고, 같은 지점의 받는 물건을 연결한다.
        static void AttachPlacementZones(Transform root)
        {
            foreach (Transform zone in root)
            {
                int split = zone.name.LastIndexOf('_');
                string baseName = split > 0 ? zone.name.Substring(0, split) : zone.name;
                string suffix = split > 0 ? zone.name.Substring(split) : "";
                if (!PlacementZones.TryGetValue(baseName, out var acceptedNames))
                {
                    continue;
                }

                var component = zone.gameObject.AddComponent<GatePassVR.Interaction.PlacementZone>();
                var serialized = new SerializedObject(component);
                serialized.FindProperty("zoneId").stringValue = zone.name;
                var accepted = serialized.FindProperty("acceptedItems");
                accepted.arraySize = 0;
                foreach (string acceptedName in acceptedNames)
                {
                    // 같은 지점 물건(접미사 자동)을 먼저 찾고, 없으면 접미사까지 쓴 다른 지점 물건으로 찾는다.
                    var item = root.Find(acceptedName + suffix) ?? root.Find(acceptedName);
                    var grab = item != null ? item.GetComponent<XRGrabInteractable>() : null;
                    if (grab == null)
                    {
                        Debug.LogError($"[AirportObjectBuilder] {zone.name}: 받는 물건 '{acceptedName}'(XRGrabInteractable)을 찾을 수 없습니다.");
                        continue;
                    }
                    accepted.arraySize++;
                    accepted.GetArrayElementAtIndex(accepted.arraySize - 1).objectReferenceValue = grab;
                }
                serialized.FindProperty("lockOnPlaced").boolValue = !NoLockZones.Contains(baseName);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log($"[AirportObjectBuilder] {zone.name}: PlacementZone 연결 (받는 물건 {accepted.arraySize}개)");
            }
        }

        static Material GetOrCreateMaterial(string colorName, bool transparent)
        {
            string path = $"{MaterialFolder}/M_Temp{colorName}.mat";
            var color = Colors[colorName];
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                // 표의 색을 바꾼 경우 기존 Material도 맞춘다.
                if (material.color != color)
                {
                    material.color = color;
                    EditorUtility.SetDirty(material);
                    AssetDatabase.SaveAssets();
                }
                return material;
            }

            if (transparent)
            {
                // 영역 표시는 조명과 관계없이 같은 색으로 보이는 반투명 Unlit.
                material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_Blend", 0f);
                material.SetFloat("_ZWrite", 0f);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            else
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            }
            material.color = color;
            AssetDatabase.CreateAsset(material, path);
            AssetDatabase.SaveAssets();
            return material;
        }
    }
}
