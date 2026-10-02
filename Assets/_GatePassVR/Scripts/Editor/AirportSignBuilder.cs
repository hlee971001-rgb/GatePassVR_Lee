// Dest_ 지점 기준으로 이동 표지판(Point & Hold 목표 자리)과 구역 이름 표지판을 가리지 않는 자리에 생성하는 Editor 도구
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GatePassVR.EditorTools
{
    public static class AirportSignBuilder
    {
        const string DestRootName = "DestinationPoints";
        const string FontPath = "Assets/Fonts/NotoSansKR-Medium SDF.asset";
        const string MaterialFolder = "Assets/_GatePassVR/Art/Temp";
        const float EyeHeight = 1.6f;

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
            public float[] Heights;
            public float MaxYaw;
            public float MinDistance;
            public float MaxDistance;
        }

        // 이동 표지판: 남색 판, 흰 글씨, Ray용 Box Collider. 가리키면 다음 구역으로 이동한다.
        static readonly SignStyle MoveStyle = new SignStyle
        {
            RootName = "MoveSigns",
            NamePrefix = "MoveSign_",
            BoardSize = new Vector3(1.2f, 0.45f, 0.05f),
            BoardColor = new Color(0.08f, 0.2f, 0.45f),
            TextColor = Color.white,
            MaterialPath = MaterialFolder + "/M_TempSignBoard.mat",
            AddCollider = true,
            Heights = new[] { 2.0f },
            MaxYaw = 40f,
            MinDistance = 2f,
            MaxDistance = 6f,
        };

        // 구역 이름 표지판: 실제 공항처럼 노란 판, 검은 글씨, 더 크고 높게. Collider 없음(Point & Hold Ray를 막지 않게).
        static readonly SignStyle ZoneStyle = new SignStyle
        {
            RootName = "ZoneSigns",
            NamePrefix = "ZoneSign_",
            BoardSize = new Vector3(2.0f, 0.6f, 0.06f),
            BoardColor = new Color(1f, 0.8f, 0.1f),
            TextColor = new Color(0.05f, 0.05f, 0.05f),
            MaterialPath = MaterialFolder + "/M_TempZoneSignBoard.mat",
            AddCollider = false,
            Heights = new[] { 3.0f, 3.5f, 4.0f },
            MaxYaw = 30f,
            MinDistance = 2f,
            MaxDistance = 8f,
        };

        // (서 있는 지점, 문구, 정면 기준 각도(+는 오른쪽), 수평 거리 m, 바닥에서 판 가운데 높이 m)
        // 표의 위치가 가려지면 범위 안에서 가장 가까운 빈자리를 찾는다. 표만 고치고 다시 실행하면 된다.
        static readonly (string from, string text, float yawOffset, float distance, float height)[] MoveSigns =
        {
            ("Dest_Start", "체크인", 30f, 4f, 2.0f),
            ("Dest_CheckIn", "보안검색", -40f, 2.5f, 2.0f),
            ("Dest_Security", "탑승구", -15f, 3.5f, 2.0f),
            ("Dest_Boarding", "탑승하기", 35f, 3f, 2.0f),
            ("Dest_Arrival", "입국심사", -40f, 4f, 2.0f),
            ("Dest_Immigration", "수하물 찾는 곳", -40f, 3f, 2.0f),
            ("Dest_Baggage", "출구", -30f, 4f, 2.0f),
        };

        // 출구(EXIT) 구역 표지판은 EXIT 위치를 김씨와 정한 뒤 추가한다(지금은 이동 표지판 "출구"와 겹침).
        static readonly (string from, string text, float yawOffset, float distance, float height)[] ZoneSigns =
        {
            ("Dest_CheckIn", "체크인", 0f, 3f, 3.0f),
            // 바로 아래의 이동 표지판 "탑승구"와 붙어 보이지 않도록 높게 단다.
            ("Dest_Security", "보안검색", 0f, 3f, 3.5f),
            ("Dest_Boarding", "탑승구", 0f, 2.5f, 3.0f),
            ("Dest_Immigration", "입국심사", 0f, 2.5f, 3.0f),
            ("Dest_Baggage", "수하물 찾는 곳", 0f, 4f, 3.0f),
        };

        // 기존 표지판을 지우고 새로 만든다. Undo(Ctrl+Z)로 되돌릴 수 있다.
        [MenuItem("GatePass/Signs/Build Move Signs")]
        static void BuildMoveSigns() => Build(MoveStyle, MoveSigns);

        [MenuItem("GatePass/Signs/Build Zone Signs")]
        static void BuildZoneSigns() => Build(ZoneStyle, ZoneSigns);

        static void Build(SignStyle style, (string from, string text, float yawOffset, float distance, float height)[] signs)
        {
            var destRoot = GameObject.Find(DestRootName);
            if (destRoot == null)
            {
                Debug.LogError($"[AirportSignBuilder] '{DestRootName}' 오브젝트를 찾을 수 없습니다. Airport_Lee Scene을 열어 주세요.");
                return;
            }
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (font == null)
            {
                Debug.LogError($"[AirportSignBuilder] 폰트를 찾을 수 없습니다: {FontPath}");
                return;
            }
            var material = GetOrCreateMaterial(style);

            var old = GameObject.Find(style.RootName);
            if (old != null)
            {
                Undo.DestroyObjectImmediate(old);
            }
            var root = new GameObject(style.RootName);
            Undo.RegisterCreatedObjectUndo(root, $"Build {style.RootName}");

            int created = 0;
            foreach (var (from, text, yawOffset, distance, height) in signs)
            {
                var dest = destRoot.transform.Find(from);
                if (dest == null)
                {
                    Debug.LogError($"[AirportSignBuilder] '{from}' 지점을 찾을 수 없습니다.");
                    continue;
                }

                if (!FindClearPlacement(style, dest, yawOffset, distance, height,
                        out float placedYaw, out float placedDistance, out float placedHeight))
                {
                    Debug.LogWarning($"[AirportSignBuilder] {from} '{text}': 가리지 않는 자리를 찾지 못해 표의 위치에 둡니다. 표 값을 조정해 주세요.");
                    placedYaw = yawOffset;
                    placedDistance = distance;
                    placedHeight = height;
                }
                else if (!Mathf.Approximately(placedYaw, yawOffset) || !Mathf.Approximately(placedDistance, distance)
                         || !Mathf.Approximately(placedHeight, height))
                {
                    Debug.Log($"[AirportSignBuilder] {from} '{text}': 표의 위치({yawOffset:+0;-0;0}°, {distance}m, 높이 {height}m)가 가려져 " +
                              $"{placedYaw:+0;-0;0}°, {placedDistance}m, 높이 {placedHeight}m로 옮겼습니다.");
                }

                CreateSign(style, root.transform, $"{style.NamePrefix}{dest.name}", dest, text,
                    placedYaw, placedDistance, placedHeight, font, material);
                created++;
            }

            EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log($"[AirportSignBuilder] {style.RootName} {created}개 생성. Scene을 저장해 주세요.", root);
        }

        // 표의 위치에서 가까운 순서로 후보를 시험해, 눈에서 판의 가운데와 네 모서리가 모두 보이고
        // 판이 다른 Collider와 겹치지 않는 첫 자리를 고른다. Collider가 없는 장식은 피하지 못한다.
        static bool FindClearPlacement(SignStyle style, Transform dest, float preferredYaw, float preferredDistance,
            float preferredHeight, out float yawOffset, out float distance, out float height)
        {
            var heights = new List<float>(style.Heights);
            if (!heights.Contains(preferredHeight))
            {
                heights.Add(preferredHeight);
            }

            var candidates = new List<(float yaw, float distance, float height, float cost)>
            {
                (preferredYaw, preferredDistance, preferredHeight, -1f),
            };
            foreach (float h in heights)
            {
                float heightCost = Mathf.Abs(h - preferredHeight) / 0.5f;
                for (float yaw = -style.MaxYaw; yaw <= style.MaxYaw + 0.01f; yaw += 5f)
                {
                    for (float d = style.MinDistance; d <= style.MaxDistance + 0.01f; d += 0.5f)
                    {
                        float cost = Mathf.Abs(yaw - preferredYaw) / 5f + Mathf.Abs(d - preferredDistance) / 0.5f + heightCost;
                        candidates.Add((yaw, d, h, cost));
                    }
                }
            }
            candidates.Sort((a, b) => a.cost.CompareTo(b.cost));

            Physics.SyncTransforms();
            var eye = dest.position + Vector3.up * EyeHeight;
            foreach (var candidate in candidates)
            {
                GetPlacement(dest, candidate.yaw, candidate.distance, candidate.height, out var position, out var rotation);
                if (IsClear(style, eye, position, rotation))
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

        static bool IsClear(SignStyle style, Vector3 eye, Vector3 center, Quaternion rotation)
        {
            if (Physics.CheckBox(center, style.BoardSize / 2f, rotation, ~0, QueryTriggerInteraction.Ignore))
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

        static void CreateSign(SignStyle style, Transform root, string name, Transform dest, string text,
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

            var board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "Board";
            Object.DestroyImmediate(board.GetComponent<Collider>());
            board.transform.SetParent(sign.transform, false);
            board.transform.localScale = style.BoardSize;
            board.GetComponent<MeshRenderer>().sharedMaterial = material;

            var label = new GameObject("Label");
            label.transform.SetParent(sign.transform, false);
            label.transform.localPosition = new Vector3(0f, 0f, -style.BoardSize.z / 2f - 0.005f);
            var tmp = label.AddComponent<TextMeshPro>();
            tmp.font = font;
            tmp.text = text;
            tmp.color = style.TextColor;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 0.1f;
            tmp.fontSizeMax = 10f;
            tmp.rectTransform.sizeDelta = new Vector2(style.BoardSize.x - 0.1f, style.BoardSize.y - 0.08f);
        }

        // 같은 종류의 표지판 판은 Material 하나를 공유한다(§24 Material 복제 금지).
        // 조명에 따라 색이 탁해지지 않도록 Unlit을 쓴다. 실제 공항의 빛나는 안내판처럼 보이고 계산도 가볍다.
        const string BoardShaderName = "Universal Render Pipeline/Unlit";

        static Material GetOrCreateMaterial(SignStyle style)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(style.MaterialPath);
            if (material != null)
            {
                // 이전에 Lit으로 만든 Material도 Unlit과 지정 색으로 맞춘다.
                if (material.shader.name != BoardShaderName || material.color != style.BoardColor)
                {
                    material.shader = Shader.Find(BoardShaderName);
                    material.color = style.BoardColor;
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

            material = new Material(Shader.Find(BoardShaderName)) { color = style.BoardColor };
            AssetDatabase.CreateAsset(material, style.MaterialPath);
            AssetDatabase.SaveAssets();
            return material;
        }
    }
}
