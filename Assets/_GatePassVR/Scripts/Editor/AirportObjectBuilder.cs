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
        }

        // 같은 색은 Material 하나를 공유한다(§24 Material 복제 금지).
        static readonly Dictionary<string, Color> Colors = new Dictionary<string, Color>
        {
            { "Passport", new Color(0.45f, 0.08f, 0.1f) },
            { "Paper", new Color(0.95f, 0.95f, 0.92f) },
            { "Luggage", new Color(0.95f, 0.5f, 0.1f) },
            // 트레이는 어두운 X-ray 벨트 위에서 눈에 띄도록 밝은 회색.
            { "Tray", new Color(0.78f, 0.8f, 0.84f) },
            { "Stand", new Color(0.6f, 0.6f, 0.6f) },
            { "Zone", new Color(0.1f, 0.85f, 0.35f, 0.35f) },
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
            ("Dest_CheckIn", "Obj_BoardingPass", Kind.Grab, new Vector3(0.08f, 0.002f, 0.2f), -0.2f, 0.95f, 0f, "Paper"),
            ("Dest_CheckIn", "Zone_BaggageDrop", Kind.Zone, new Vector3(0.7f, 0.02f, 0.6f), -1.8f, 1.35f, 0f, "Zone"),
            // 보안검색: 트레이는 X-ray 벨트 입구. 벨트가 앞뒤로 흐르므로 긴 쪽(60cm)을 앞뒤로 둔다.
            ("Dest_Security", "Obj_SecurityTray", Kind.Grab, new Vector3(0.4f, 0.08f, 0.6f), -0.375f, 1.05f, 0f, "Tray"),
            // 탑승 게이트: 부스 책상 위 여권·탑승권 스캔 자리. 책상 앞 끝이 지점에서 약 0.98m라 앞 끝에 붙여 1m 안에 둔다.
            ("Dest_Boarding", "Zone_Scan", Kind.Zone, new Vector3(0.3f, 0.02f, 0.25f), -0.3f, 1.1f, 0f, "Zone"),
            // 입국심사: 부스 책상 위 여권 건네는 자리(Boarding과 같은 부스 줄, 같은 기준).
            ("Dest_Immigration", "Zone_HandOver", Kind.Zone, new Vector3(0.3f, 0.02f, 0.25f), -0.3f, 1.1f, 0f, "Zone"),
            // 수하물 수취: 컨베이어 가장자리에 눕힌 가방(서는 지점에서 1m 안).
            ("Dest_Baggage", "Obj_BaggageBag", Kind.Grab, new Vector3(0.4f, 0.25f, 0.6f), -0.5f, 0.55f, 0f, "Luggage"),
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
                var footprint = new[] { point, point + halfX + halfZ, point + halfX - halfZ, point - halfX + halfZ, point - halfX - halfZ };
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
                        go.AddComponent<Rigidbody>();
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

            EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log($"[AirportObjectBuilder] {RootName} {created}개 생성. Scene을 저장해 주세요.", root);
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
