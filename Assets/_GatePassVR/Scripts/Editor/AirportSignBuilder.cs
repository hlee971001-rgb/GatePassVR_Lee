// Dest_ 지점 기준으로 이동 표지판(Point & Hold 목표, 안내 기둥형)과 구역 이름 표지판을 가리지 않는 자리에 생성하는 Editor 도구
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace GatePassVR.EditorTools
{
    public static class AirportSignBuilder
    {
        const string DestRootName = "DestinationPoints";
        const string SourceFontPath = "Assets/Fonts/NotoSansKR-Medium.ttf";
        // 표지판 글자만 미리 담은 고정(Static) 폰트. 동적 폰트는 글자를 쓸 때마다 Atlas가 바뀌고 글자 크기·굵기가 들쭉날쭉해진다.
        const string SignFontPath = "Assets/Fonts/NotoSansKR-Medium SDF Signs.asset";
        const string MaterialFolder = "Assets/_GatePassVR/Art/Temp";
        const float EyeHeight = SignViewRules.StandingEyeHeight;

        // 이동 표지판 아래 작은 안내 문구(Point & Hold 조작 방법)
        const string HoldHintText = "가리키고 잠시 기다리세요";

        // 표지판 종류별 모양과 자리 찾기 범위
        class SignStyle
        {
            public string RootName;
            public string NamePrefix;
            public Vector3 BoardSize;
            public Color BoardColor;
            public Color TextColor;
            public string MaterialPath;
            public bool AddCollider;
            public bool IsPylon;     // 바닥에 선 기둥 + 패널(안내 기둥형)
            public bool IsZone;      // 구역 표지판: 왼쪽 출국/입국 띠, 천장에 매달거나(천장이 가까우면) 두 기둥으로 세움
            public float[] Heights;  // 바닥에서 판 가운데 높이 후보
            public float[] FallbackHeights = new float[0]; // Heights로 자리가 없을 때만 쓰는 높이
            public float MinDistance;
            public float MaxDistance;
            public float MaxYaw;     // 정면 기준 좌우 허용 각도
        }

        // 이동 표지판: 바닥에 선 안내 기둥. 왼쪽 흰 원에 방향 화살표(김씨 원형 게이지가 붙을 자리), 오른쪽에 목적지와 조작 안내.
        // 패널 가운데를 서서 보는 눈높이 근처(1.6m)에 두고 정면 ±20° 안에 세워 화면에서 찌그러지지 않게 한다.
        static readonly SignStyle MoveStyle = new SignStyle
        {
            RootName = "MoveSigns",
            NamePrefix = "MoveSign_",
            BoardSize = new Vector3(1.2f, 0.7f, 0.05f),
            BoardColor = new Color(0.08f, 0.2f, 0.45f),
            TextColor = Color.white,
            MaterialPath = MaterialFolder + "/M_TempSignBoard.mat",
            AddCollider = true,
            IsPylon = true,
            Heights = new[] { 1.6f },
            MinDistance = 2.5f,
            MaxDistance = 4f,
            MaxYaw = 20f,
        };

        // 구역 이름 표지판: 실제 공항처럼 노란 판, 검은 글씨. Collider 없음(Point & Hold Ray를 막지 않게).
        // "지금 여기가 어디인지" 알려 주는 배경 정보라서 눈높이의 이동 표지판(행동)보다 작고 높게 둔다(3m 이상, 4m 이상 떨어져서).
        // 왼쪽 띠로 출국(파랑)/입국(초록)을 구별한다. 이 공항은 출국·입국 구역이 한 건물이라 서로 보이기 때문이다.
        static readonly SignStyle ZoneStyle = new SignStyle
        {
            RootName = "ZoneSigns",
            NamePrefix = "ZoneSign_",
            BoardSize = new Vector3(1.6f, 0.45f, 0.05f),
            BoardColor = new Color(1f, 0.8f, 0.1f),
            TextColor = new Color(0.05f, 0.05f, 0.05f),
            MaterialPath = MaterialFolder + "/M_TempZoneSignBoard.mat",
            AddCollider = false,
            IsZone = true,
            Heights = new[] { 3.0f, 3.3f, 3.6f },
            // 2층 심사 부스처럼 앞이 높은 시설로 막혀 3m 이상에 자리가 없을 때만 낮춘다.
            // (2층 부스 줄은 이 범위로도 자리가 없어 FacilityMountYaw로 부스 앞면에 붙인다.)
            FallbackHeights = new[] { 2.6f },
            MinDistance = 4f,
            MaxDistance = 8f,
            MaxYaw = SignViewRules.MaxYaw,
        };

        const float ZoneBandWidth = 0.4f;
        static readonly Color DepartureBandColor = new Color(0.1f, 0.35f, 0.75f);
        static readonly Color ArrivalBandColor = new Color(0.1f, 0.55f, 0.3f);
        // 판 윗면에서 천장까지 이 거리 안이면 천장에 매달고, 더 멀면(2층 천장 13m 이상 등) 바닥에 두 기둥으로 세운다.
        const float MaxHangLength = 3f;

        // (서 있는 지점, 목적지 문구, 방향 화살표, 정면 기준 각도(+는 오른쪽), 수평 거리 m)
        // 각도는 다음 구역 쪽(±20° 안으로 자른 값)이다. 화살표는 서 있는 지점에서 본 다음 구역 방향이다.
        // 다음 구역 실제 방향(정면 기준): Start→CheckIn +34°, CheckIn→Security -136°(뒤 왼쪽), Security→Boarding -10°(2층),
        // Boarding→Arrival +67°(비행), Arrival→Immigration -110°(2층), Immigration→Baggage -165°(1층), Baggage→EXIT 미정.
        static readonly (string from, string text, string arrow, float yawOffset, float distance)[] MoveSigns =
        {
            ("Dest_Start", "체크인", "↗", 20f, 3f),
            ("Dest_CheckIn", "보안검색", "↙", -20f, 3f),
            ("Dest_Security", "탑승구 <size=65%>2층</size>", "↑", -10f, 3f),
            ("Dest_Boarding", "탑승하기", "→", 20f, 3f),
            ("Dest_Arrival", "입국심사 <size=65%>2층</size>", "←", -20f, 3f),
            ("Dest_Immigration", "수하물 찾는 곳 <size=65%>1층</size>", "↙", -20f, 3f),
            ("Dest_Baggage", "출구", "↑", -20f, 3f), // EXIT 위치를 김씨와 정한 뒤 화살표 조정
        };

        // (서 있는 지점, 구역 이름, 출국/입국 띠, 정면 기준 각도, 수평 거리 m, 바닥에서 판 가운데 높이 m)
        // 출구(EXIT) 구역 표지판은 EXIT 위치를 김씨와 정한 뒤 추가한다.
        static readonly (string from, string text, string band, float yawOffset, float distance, float height)[] ZoneSigns =
        {
            ("Dest_CheckIn", "체크인", "출국", 0f, 4.5f, 3.0f),
            ("Dest_Security", "보안검색", "출국", 0f, 4.5f, 3.0f),
            ("Dest_Boarding", "탑승구", "출국", 0f, 4.5f, 3.0f),
            ("Dest_Immigration", "입국심사", "입국", 0f, 4.5f, 3.0f),
            ("Dest_Baggage", "수하물 찾는 곳", "입국", 0f, 4.5f, 3.0f),
        };

        // 2층 심사 부스 줄은 바로 앞이 높이 3.7m 부스와 벽이라 매달거나 세울 빈자리가 없다.
        // 이 지점들은 표의 각도 방향으로 가장 가까운 시설(부스) 앞면에 판을 붙인다. 이동 표지판과 반대쪽 각도로 둔다.
        // 높이는 앉은 사용자가 MaxElevation 안쪽(24°)으로 올려다보도록 부스까지 거리에서 계산한다.
        static readonly Dictionary<string, float> FacilityMountYaw = new Dictionary<string, float>
        {
            { "Dest_Boarding", -20f },     // 이동 표지판 +20°
            { "Dest_Immigration", 20f },   // 이동 표지판 -15°
        };

        // 기존 표지판을 지우고 새로 만든다. Undo(Ctrl+Z)로 되돌릴 수 있다.
        // 구역 표지판은 이동 표지판 Collider를 피해서 자리를 찾으므로 이동 표지판을 먼저 만든다.
        [MenuItem("GatePass/Signs/Build Move Signs")]
        static void BuildMoveSigns()
        {
            var signs = new List<(string from, string text, string arrow, float yawOffset, float distance, float height)>();
            foreach (var (from, text, arrow, yaw, distance) in MoveSigns)
            {
                signs.Add((from, text, arrow, yaw, distance, MoveStyle.Heights[0]));
            }
            Build(MoveStyle, signs);
        }

        [MenuItem("GatePass/Signs/Build Zone Signs")]
        static void BuildZoneSigns()
        {
            var signs = new List<(string from, string text, string arrow, float yawOffset, float distance, float height)>();
            // 구역 표지판은 화살표 자리에 출국/입국 띠 문구를 넘긴다.
            foreach (var (from, text, band, yaw, distance, height) in ZoneSigns)
            {
                signs.Add((from, text, band, yaw, distance, height));
            }
            Build(ZoneStyle, signs);
        }

        static void Build(SignStyle style, List<(string from, string text, string arrow, float yawOffset, float distance, float height)> signs)
        {
            var destRoot = GameObject.Find(DestRootName);
            if (destRoot == null)
            {
                Debug.LogError($"[AirportSignBuilder] '{DestRootName}' 오브젝트를 찾을 수 없습니다. Airport_Lee Scene을 열어 주세요.");
                return;
            }
            var font = GetOrUpdateSignFont();
            if (font == null)
            {
                return;
            }
            var material = GetOrCreateMaterial(style.MaterialPath, style.BoardColor);

            var old = GameObject.Find(style.RootName);
            if (old != null)
            {
                Undo.DestroyObjectImmediate(old);
            }
            var root = new GameObject(style.RootName);
            Undo.RegisterCreatedObjectUndo(root, $"Build {style.RootName}");

            int created = 0;
            foreach (var (from, text, arrow, yawOffset, distance, height) in signs)
            {
                var dest = destRoot.transform.Find(from);
                if (dest == null)
                {
                    Debug.LogError($"[AirportSignBuilder] '{from}' 지점을 찾을 수 없습니다.");
                    continue;
                }

                if (style.IsZone && FacilityMountYaw.TryGetValue(from, out float mountYaw))
                {
                    if (TryFacilityMount(dest, mountYaw, out float mountDistance, out float mountHeight))
                    {
                        var mounted = CreateSign(style, root.transform, $"{style.NamePrefix}{dest.name}", dest, text,
                            mountYaw, mountDistance, mountHeight, font, material);
                        AddZoneParts(style, mounted.transform, dest, arrow, font, false);
                        Debug.Log($"[AirportSignBuilder] {from} '{text}': 시설 앞면에 붙임 ({mountYaw:+0;-0;0}°, {mountDistance:0.00}m, 높이 {mountHeight:0.00}m)");
                        created++;
                        continue;
                    }
                    Debug.LogWarning($"[AirportSignBuilder] {from} '{text}': {mountYaw:+0;-0;0}° 방향 3m 안에 붙일 시설이 없어 일반 방식으로 둡니다.");
                }

                if (!FindClearPlacement(style, dest, yawOffset, distance, height,
                        out float placedYaw, out float placedDistance, out float placedHeight))
                {
                    Debug.LogWarning($"[AirportSignBuilder] {from} '{text}': 가리지 않는 자리를 찾지 못해 표의 위치에 둡니다. 표 값을 조정해 주세요.");
                    placedYaw = Mathf.Clamp(yawOffset, -style.MaxYaw, style.MaxYaw);
                    placedDistance = distance;
                    placedHeight = height;
                }
                else if (!Mathf.Approximately(placedYaw, yawOffset) || !Mathf.Approximately(placedDistance, distance)
                         || !Mathf.Approximately(placedHeight, height))
                {
                    Debug.Log($"[AirportSignBuilder] {from} '{text}': 표의 위치({yawOffset:+0;-0;0}°, {distance}m, 높이 {height}m)가 가려져 " +
                              $"{placedYaw:+0;-0;0}°, {placedDistance}m, 높이 {placedHeight}m로 옮겼습니다.");
                }

                var sign = CreateSign(style, root.transform, $"{style.NamePrefix}{dest.name}", dest, text,
                    placedYaw, placedDistance, placedHeight, font, material);
                if (style.IsPylon)
                {
                    AddPylonParts(style, sign.transform, dest, arrow, font);
                }
                if (style.IsZone)
                {
                    AddZoneParts(style, sign.transform, dest, arrow, font, true);
                }
                created++;
            }

            EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log($"[AirportSignBuilder] {style.RootName} {created}개 생성. Scene을 저장해 주세요.", root);
        }

        // 표의 위치에서 가까운 순서로 후보를 시험해, 눈에서 판의 가운데와 네 모서리가 모두 보이고
        // 판(과 기둥)이 다른 Collider와 겹치지 않는 첫 자리를 고른다. Collider가 없는 장식은 피하지 못한다.
        static bool FindClearPlacement(SignStyle style, Transform dest, float preferredYaw, float preferredDistance,
            float preferredHeight, out float yawOffset, out float distance, out float height)
        {
            // 표의 각도도 허용 범위(±MaxYaw) 안으로 맞춘 뒤 시험한다.
            preferredYaw = Mathf.Clamp(preferredYaw, -style.MaxYaw, style.MaxYaw);
            var heights = new List<float>(style.Heights);
            if (!heights.Contains(preferredHeight))
            {
                heights.Add(preferredHeight);
            }
            heights.AddRange(style.FallbackHeights);

            var candidates = new List<(float yaw, float distance, float height, float cost)>
            {
                (preferredYaw, preferredDistance, preferredHeight, -1f),
            };
            // 스타일 범위(±MaxYaw, Min~MaxDistance) 안의 자리를 먼저 고른다.
            // 바로 앞이 X-ray 기계·심사 부스처럼 높은 시설로 막혀 범위 안에 빈자리가 없으면,
            // 편하게 보이는 범위(±SignViewRules.MaxYaw)와 2m~MaxDistance+1m까지 넓혀서 찾는다(비용을 크게 더해 마지막에 시험).
            const float OutOfRangeCost = 100f;
            float searchMinDistance = Mathf.Min(style.MinDistance, 2f);
            float searchMaxDistance = style.MaxDistance + 1f;
            foreach (float h in heights)
            {
                float heightCost = Mathf.Abs(h - preferredHeight) / 0.5f;
                if (System.Array.IndexOf(style.FallbackHeights, h) >= 0)
                {
                    heightCost += OutOfRangeCost;
                }
                for (float yaw = -SignViewRules.MaxYaw; yaw <= SignViewRules.MaxYaw + 0.01f; yaw += 5f)
                {
                    for (float d = searchMinDistance; d <= searchMaxDistance + 0.01f; d += 0.5f)
                    {
                        float cost = Mathf.Abs(yaw - preferredYaw) / 5f + Mathf.Abs(d - preferredDistance) / 0.5f + heightCost;
                        if (Mathf.Abs(yaw) > style.MaxYaw + 0.01f || d < style.MinDistance - 0.01f || d > style.MaxDistance + 0.01f)
                        {
                            cost += OutOfRangeCost;
                        }
                        candidates.Add((yaw, d, h, cost));
                    }
                }
            }
            candidates.Sort((a, b) => a.cost.CompareTo(b.cost));

            Physics.SyncTransforms();
            var standingEye = SignViewRules.Eye(dest, SignViewRules.StandingEyeHeight);
            var seatedEye = SignViewRules.Eye(dest, SignViewRules.SeatedEyeHeight);
            foreach (var candidate in candidates)
            {
                GetPlacement(dest, candidate.yaw, candidate.distance, candidate.height, out var position, out var rotation);
                // 앉은 사용자도 MaxElevation 이하로 올려다보고, 서서·앉아서 모두 가려지지 않아야 한다.
                if (SignViewRules.Elevation(seatedEye, position) <= SignViewRules.MaxElevation
                    && (!style.IsPylon || IsPostClear(style, dest, position))
                    && (!style.IsZone || IsZoneMountClear(style, dest, position, rotation))
                    && IsClear(style, standingEye, position, rotation) && IsClear(style, seatedEye, position, rotation))
                {
                    yawOffset = candidate.yaw;
                    distance = candidate.distance;
                    height = candidate.height;
                    return true;
                }
            }
            yawOffset = preferredYaw;
            distance = preferredDistance;
            height = preferredHeight;
            return false;
        }

        static readonly Vector3 Gap = new Vector3(0.2f, 0.1f, 0.1f);
        const float PostWidth = 0.08f;

        static bool IsClear(SignStyle style, Vector3 eye, Vector3 center, Quaternion rotation)
        {
            // 판 둘레에 여유(Gap)를 두고 검사해, 다른 표지판(Collider가 있는 이동 표지판)이나 시설과 붙어 보이지 않게 한다.
            if (Physics.CheckBox(center, style.BoardSize / 2f + Gap, rotation, ~0, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            var right = rotation * Vector3.right * (style.BoardSize.x / 2f);
            var up = rotation * Vector3.up * (style.BoardSize.y / 2f);
            var points = new[] { center, center + right + up, center + right - up, center - right + up, center - right - up };
            foreach (var point in points)
            {
                var toPoint = point - eye;
                if (Physics.Raycast(eye, toPoint.normalized, toPoint.magnitude - 0.05f, ~0, QueryTriggerInteraction.Ignore))
                {
                    return false;
                }
            }
            return true;
        }

        // 기둥이 카운터·벨트 같은 시설을 뚫고 지나가지 않는지 검사한다. 바닥에 닿는 아래 5cm는 빼고 본다.
        static bool IsPostClear(SignStyle style, Transform dest, Vector3 boardCenter)
        {
            float bottom = dest.position.y + 0.05f;
            float top = boardCenter.y - style.BoardSize.y / 2f;
            if (top <= bottom)
            {
                return true;
            }
            var center = new Vector3(boardCenter.x, (bottom + top) / 2f, boardCenter.z);
            var half = new Vector3(PostWidth / 2f + 0.05f, (top - bottom) / 2f, PostWidth / 2f + 0.05f);
            return !Physics.CheckBox(center, half, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
        }

        // 구역 표지판 두 지지대(판 좌우 끝에서 0.15m 안쪽)의 위치
        static Vector3[] ZoneSupportPoints(SignStyle style, Vector3 center, Quaternion rotation)
        {
            var side = rotation * Vector3.right * (style.BoardSize.x / 2f - 0.15f);
            return new[] { center - side, center + side };
        }

        // 두 지지대 위로 MaxHangLength 안에 천장이 있으면 매달기(천장 높이 반환), 없으면 바닥 기둥.
        static bool TryFindCeiling(SignStyle style, Vector3 center, Quaternion rotation, out float ceilingY)
        {
            ceilingY = float.PositiveInfinity;
            float top = center.y + style.BoardSize.y / 2f;
            foreach (var point in ZoneSupportPoints(style, center, rotation))
            {
                if (!Physics.Raycast(new Vector3(point.x, top + 0.01f, point.z), Vector3.up, out var hit,
                        MaxHangLength, ~0, QueryTriggerInteraction.Ignore))
                {
                    return false;
                }
                ceilingY = Mathf.Min(ceilingY, hit.point.y);
            }
            return true;
        }

        // 매달 수 없으면 두 바닥 기둥이 시설을 뚫지 않아야 한다.
        static bool IsZoneMountClear(SignStyle style, Transform dest, Vector3 center, Quaternion rotation)
        {
            if (TryFindCeiling(style, center, rotation, out _))
            {
                return true;
            }
            foreach (var point in ZoneSupportPoints(style, center, rotation))
            {
                if (!IsPostClear(style, dest, point))
                {
                    return false;
                }
            }
            return true;
        }

        const float FacilityMountElevation = 24f;

        // 표의 각도 방향으로 높이를 바꿔 가며 3m 안의 시설 앞면을 찾는다. 판을 앞면에서 4cm 띄워 붙인다.
        static bool TryFacilityMount(Transform dest, float yawOffset, out float distance, out float height)
        {
            // 부스 유리 원통처럼 Collider가 없는 모델 부분에 판이 묻히지 않도록, 공항 모델 전체에 임시 Mesh Collider를 만들어 잰다.
            // (AirportLayoutValidator의 Analyze와 같은 방식. 숨김 오브젝트라 Scene에 남지 않는다.)
            var temporary = new List<GameObject>();
            var model = GameObject.Find("Airport7_FINALc_EscenaMontada");
            if (model != null)
            {
                foreach (var filter in model.GetComponentsInChildren<MeshFilter>())
                {
                    if (filter.sharedMesh == null)
                    {
                        continue;
                    }
                    var go = new GameObject(filter.name) { hideFlags = HideFlags.HideAndDontSave };
                    go.transform.SetPositionAndRotation(filter.transform.position, filter.transform.rotation);
                    go.transform.localScale = filter.transform.lossyScale;
                    go.AddComponent<MeshCollider>().sharedMesh = filter.sharedMesh;
                    temporary.Add(go);
                }
            }
            try
            {
                return MeasureFacilityMount(dest, yawOffset, out distance, out height);
            }
            finally
            {
                foreach (var go in temporary)
                {
                    Object.DestroyImmediate(go);
                }
                Physics.SyncTransforms();
            }
        }

        static bool MeasureFacilityMount(Transform dest, float yawOffset, out float distance, out float height)
        {
            Physics.SyncTransforms();
            var direction = Quaternion.Euler(0f, dest.eulerAngles.y + yawOffset, 0f) * Vector3.forward;
            // 먼저 대략 높이 2.1m에서 앞면까지 거리를 재고, 그 거리로 높이를 다시 정한 뒤 그 높이에서 한 번 더 잰다.
            // 원통형 부스처럼 앞면이 휘어 있으면 가운데만 재면 판 양 끝이 시설에 박힌다. 판 가운데와 양 끝 세 줄 중 가장 가까운 앞면에 맞춘다.
            var side = Quaternion.Euler(0f, dest.eulerAngles.y + yawOffset, 0f) * Vector3.right * (ZoneStyle.BoardSize.x / 2f);
            height = 2.1f;
            distance = 0f;
            for (int i = 0; i < 2; i++)
            {
                // 판 폭을 0.2m 간격으로, 판 위·가운데·아래 세 높이에서 잰다(원통 하나가 판 일부만 가려도 놓치지 않게).
                float nearest = float.PositiveInfinity;
                for (float t = -1f; t <= 1.001f; t += 0.25f)
                {
                    foreach (float dy in new[] { -ZoneStyle.BoardSize.y / 2f, 0f, ZoneStyle.BoardSize.y / 2f })
                    {
                        var origin = dest.position + Vector3.up * (height + dy) + side * t;
                        if (Physics.Raycast(origin, direction, out var hit, 3f, ~0, QueryTriggerInteraction.Ignore))
                        {
                            nearest = Mathf.Min(nearest, hit.distance);
                        }
                    }
                }
                if (float.IsInfinity(nearest))
                {
                    return false;
                }
                distance = nearest - 0.04f;
                height = SignViewRules.SeatedEyeHeight + Mathf.Tan(FacilityMountElevation * Mathf.Deg2Rad) * distance;
            }
            return true;
        }

        // 왼쪽 출국/입국 띠와, 천장에 매다는 줄 또는 바닥 기둥 두 개를 붙인다(시설에 붙인 판은 지지대 없음).
        static void AddZoneParts(SignStyle style, Transform sign, Transform dest, string band, TMP_FontAsset font, bool addSupports)
        {
            float front = -style.BoardSize.z / 2f;
            if (!string.IsNullOrEmpty(band))
            {
                bool departure = band == "출국";
                var bandMaterial = GetOrCreateMaterial(
                    MaterialFolder + (departure ? "/M_TempZoneBandDeparture.mat" : "/M_TempZoneBandArrival.mat"),
                    departure ? DepartureBandColor : ArrivalBandColor);
                float bandX = -style.BoardSize.x / 2f + ZoneBandWidth / 2f;
                CreateBox("Band", sign, new Vector3(bandX, 0f, front - 0.002f),
                    new Vector3(ZoneBandWidth, style.BoardSize.y, 0.004f), bandMaterial);
                CreateLabel("BandLabel", sign, new Vector3(bandX, 0f, front - 0.01f),
                    new Vector2(ZoneBandWidth - 0.08f, style.BoardSize.y - 0.12f), band, font, Color.white, 10f);
            }

            if (!addSupports)
            {
                return;
            }
            var supportMaterial = GetOrCreateMaterial(MaterialFolder + "/M_TempSignPost.mat", new Color(0.18f, 0.2f, 0.24f));
            Physics.SyncTransforms();
            bool hanging = TryFindCeiling(style, sign.position, sign.rotation, out float ceilingY);
            float boardTop = sign.position.y + style.BoardSize.y / 2f;
            float boardBottom = sign.position.y - style.BoardSize.y / 2f;
            float from = hanging ? boardTop : dest.position.y;
            float to = hanging ? ceilingY : boardBottom;
            float width = hanging ? 0.02f : PostWidth;
            var points = ZoneSupportPoints(style, sign.position, sign.rotation);
            for (int i = 0; i < points.Length; i++)
            {
                var local = sign.InverseTransformPoint(new Vector3(points[i].x, (from + to) / 2f, points[i].z));
                CreateBox(hanging ? $"HangRod{i}" : $"Post{i}", sign, new Vector3(local.x, local.y, 0.01f),
                    new Vector3(width, to - from, width), supportMaterial);
            }
        }

        // 표지판의 +Z가 플레이어 반대쪽을 향하게 해서, 글씨(-Z 면)가 플레이어 쪽에 보이게 한다.
        static void GetPlacement(Transform dest, float yawOffset, float distance, float height,
            out Vector3 position, out Quaternion rotation)
        {
            var direction = Quaternion.Euler(0f, dest.eulerAngles.y + yawOffset, 0f) * Vector3.forward;
            position = dest.position + direction * distance + Vector3.up * height;
            var toSign = position - (dest.position + Vector3.up * EyeHeight);
            toSign.y = 0f;
            rotation = Quaternion.LookRotation(toSign);
        }

        static GameObject CreateSign(SignStyle style, Transform root, string name, Transform dest, string text,
            float yawOffset, float distance, float height, TMP_FontAsset font, Material material)
        {
            GetPlacement(dest, yawOffset, distance, height, out var position, out var rotation);
            var sign = new GameObject(name);
            sign.transform.SetParent(root, false);
            sign.transform.SetPositionAndRotation(position, rotation);
            if (style.AddCollider)
            {
                sign.AddComponent<BoxCollider>().size = style.BoardSize;
            }

            CreateBox("Board", sign.transform, Vector3.zero, style.BoardSize, material);

            // 안내 기둥은 왼쪽 원 아이콘 자리를 비우고 오른쪽 위에 목적지, 아래에 조작 안내를 둔다.
            float front = -style.BoardSize.z / 2f - 0.005f;
            if (style.IsPylon)
            {
                // 글자 영역은 원 아이콘 오른쪽 끝(-0.21) 바로 뒤부터 판 오른쪽 끝 5cm 앞까지(폭 0.72m).
                CreateLabel("Label", sign.transform, new Vector3(0.19f, 0.09f, front), new Vector2(0.72f, 0.34f),
                    text, font, style.TextColor, 10f);
                CreateLabel("Hint", sign.transform, new Vector3(0.19f, -0.21f, front), new Vector2(0.72f, 0.14f),
                    HoldHintText, font, new Color(0.75f, 0.85f, 1f), 10f);
            }
            else if (style.IsZone)
            {
                // 왼쪽 띠(ZoneBandWidth) 오른쪽 영역 가운데에 구역 이름을 둔다.
                float areaLeft = -style.BoardSize.x / 2f + ZoneBandWidth + 0.05f;
                float areaRight = style.BoardSize.x / 2f - 0.05f;
                CreateLabel("Label", sign.transform, new Vector3((areaLeft + areaRight) / 2f, 0f, front),
                    new Vector2(areaRight - areaLeft, style.BoardSize.y - 0.08f), text, font, style.TextColor, 10f);
            }
            else
            {
                CreateLabel("Label", sign.transform, new Vector3(0f, 0f, front),
                    new Vector2(style.BoardSize.x - 0.1f, style.BoardSize.y - 0.08f), text, font, style.TextColor, 10f);
            }
            return sign;
        }

        // 원 아이콘: 판 왼쪽 끝에서 5cm 안쪽(-0.55)부터 지름 0.34m
        const float IconDiameter = 0.34f;
        static readonly Vector3 IconCenter = new Vector3(-0.38f, 0f, 0f);

        // 바닥까지 내려오는 기둥과, 왼쪽의 흰 원(방향 화살표) 아이콘을 붙인다.
        // GaugeAnchor는 김씨의 Point & Hold 원형 게이지(RadialGaugeVisual)를 붙일 자리다(원 둘레에 게이지가 차오르게).
        static void AddPylonParts(SignStyle style, Transform sign, Transform dest, string arrow, TMP_FontAsset font)
        {
            float boardBottom = sign.position.y - style.BoardSize.y / 2f;
            float postLength = boardBottom - dest.position.y;
            if (postLength > 0.01f)
            {
                var postMaterial = GetOrCreateMaterial(MaterialFolder + "/M_TempSignPost.mat", new Color(0.18f, 0.2f, 0.24f));
                CreateBox("Post", sign, new Vector3(0f, -style.BoardSize.y / 2f - postLength / 2f, 0.01f),
                    new Vector3(PostWidth, postLength, PostWidth), postMaterial);
            }

            float front = -style.BoardSize.z / 2f;
            var iconMaterial = GetOrCreateMaterial(MaterialFolder + "/M_TempSignIcon.mat", Color.white);
            var icon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            icon.name = "HoldIcon";
            Object.DestroyImmediate(icon.GetComponent<Collider>());
            icon.transform.SetParent(sign, false);
            // Cylinder는 Y축이 높이라 X로 90° 눕혀 판 앞면에 원판으로 붙인다.
            icon.transform.localPosition = IconCenter + new Vector3(0f, 0f, front - 0.003f);
            icon.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            icon.transform.localScale = new Vector3(IconDiameter, 0.003f, IconDiameter);
            icon.GetComponent<MeshRenderer>().sharedMaterial = iconMaterial;
            icon.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            if (!string.IsNullOrEmpty(arrow))
            {
                CreateLabel("Arrow", sign, IconCenter + new Vector3(0f, 0.01f, front - 0.01f),
                    new Vector2(IconDiameter * 0.8f, IconDiameter * 0.8f), arrow, font, style.BoardColor, 10f);
            }

            var anchor = new GameObject("GaugeAnchor");
            anchor.transform.SetParent(sign, false);
            anchor.transform.localPosition = IconCenter + new Vector3(0f, 0f, front - 0.012f);
        }

        static void CreateBox(string name, Transform parent, Vector3 localPosition, Vector3 size, Material material)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            Object.DestroyImmediate(box.GetComponent<Collider>());
            box.transform.SetParent(parent, false);
            box.transform.localPosition = localPosition;
            box.transform.localScale = size;
            box.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        static void CreateLabel(string name, Transform parent, Vector3 localPosition, Vector2 size, string text,
            TMP_FontAsset font, Color color, float maxFontSize)
        {
            var label = new GameObject(name);
            label.transform.SetParent(parent, false);
            label.transform.localPosition = localPosition;
            var tmp = label.AddComponent<TextMeshPro>();
            tmp.font = font;
            tmp.text = text;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 0.1f;
            tmp.fontSizeMax = maxFontSize;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.rectTransform.sizeDelta = size;
        }

        // 표에 쓰인 모든 문구의 글자를 고정 폰트에 넣는다. 표의 문구를 바꾸고 다시 실행하면 새 글자도 추가된다.
        static TMP_FontAsset GetOrUpdateSignFont()
        {
            var characters = new HashSet<char>(HoldHintText + "0123456789층");
            foreach (var sign in MoveSigns)
            {
                characters.UnionWith(StripTags(sign.text) + sign.arrow);
            }
            foreach (var sign in ZoneSigns)
            {
                characters.UnionWith(StripTags(sign.text) + sign.band);
            }
            characters.Remove(' ');
            string needed = string.Concat(characters);

            var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SignFontPath);
            if (asset == null)
            {
                var source = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
                if (source == null)
                {
                    Debug.LogError($"[AirportSignBuilder] 원본 폰트를 찾을 수 없습니다: {SourceFontPath}");
                    return null;
                }
                asset = TMP_FontAsset.CreateFontAsset(source, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024,
                    AtlasPopulationMode.Dynamic, false);
                asset.name = "NotoSansKR-Medium SDF Signs";
                AssetDatabase.CreateAsset(asset, SignFontPath);
                asset.atlasTexture.name = asset.name + " Atlas";
                AssetDatabase.AddObjectToAsset(asset.atlasTexture, asset);
                asset.material.name = asset.name + " Material";
                AssetDatabase.AddObjectToAsset(asset.material, asset);
            }

            if (!asset.HasCharacters(needed, out _, false, false))
            {
                asset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                if (!asset.TryAddCharacters(needed, out string missing))
                {
                    Debug.LogWarning($"[AirportSignBuilder] 폰트에 넣지 못한 글자: {missing}");
                }
            }
            asset.atlasPopulationMode = AtlasPopulationMode.Static;
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            return asset;
        }

        static string StripTags(string text) => System.Text.RegularExpressions.Regex.Replace(text, "<[^>]*>", "");

        // 같은 종류의 표지판 판은 Material 하나를 공유한다(§24 Material 복제 금지).
        // 조명에 따라 색이 탁해지지 않도록 Unlit을 쓴다. 실제 공항의 빛나는 안내판처럼 보이고 계산도 가볍다.
        const string BoardShaderName = "Universal Render Pipeline/Unlit";

        static Material GetOrCreateMaterial(string path, Color color)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                // 이전에 Lit으로 만든 Material도 Unlit과 지정 색으로 맞춘다.
                if (material.shader.name != BoardShaderName || material.color != color)
                {
                    material.shader = Shader.Find(BoardShaderName);
                    material.color = color;
                    EditorUtility.SetDirty(material);
                    AssetDatabase.SaveAssets();
                }
                return material;
            }

            if (!AssetDatabase.IsValidFolder("Assets/_GatePassVR/Art"))
            {
                AssetDatabase.CreateFolder("Assets/_GatePassVR", "Art");
            }
            if (!AssetDatabase.IsValidFolder(MaterialFolder))
            {
                AssetDatabase.CreateFolder("Assets/_GatePassVR/Art", "Temp");
            }

            material = new Material(Shader.Find(BoardShaderName)) { color = color };
            AssetDatabase.CreateAsset(material, path);
            AssetDatabase.SaveAssets();
            return material;
        }
    }
}
