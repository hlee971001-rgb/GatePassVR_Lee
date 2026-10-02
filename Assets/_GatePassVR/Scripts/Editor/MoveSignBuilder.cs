// Dest_ 지점마다 다음 구역 이름이 적힌 이동 표지판(Point & Hold 목표 자리)을 생성하는 Editor 도구
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GatePassVR.EditorTools
{
    public static class MoveSignBuilder
    {
        const string DestRootName = "DestinationPoints";
        const string RootName = "MoveSigns";
        const string FontPath = "Assets/Fonts/NotoSansKR-Medium SDF.asset";
        const string MaterialFolder = "Assets/_GatePassVR/Art/Temp";
        const string BoardMaterialPath = MaterialFolder + "/M_TempSignBoard.mat";

        static readonly Vector3 BoardSize = new Vector3(1.2f, 0.45f, 0.05f);
        const float BoardCenterHeight = 2.0f;
        const float EyeHeight = 1.6f;
        static readonly Color BoardColor = new Color(0.08f, 0.2f, 0.45f);

        // 표지판 위치: 서 있는 지점의 방향 기준 YawOffset(+는 오른쪽)으로 Distance(수평 m)만큼 떨어진 곳.
        // 이 표만 고치고 다시 실행하면 표지판 전체가 새로 만들어진다.
        static readonly (string from, string text, float yawOffset, float distance)[] Signs =
        {
            ("Dest_Start", "체크인", 30f, 4f),
            ("Dest_CheckIn", "보안검색", -40f, 2.5f),
            ("Dest_Security", "탑승구", -15f, 3.5f),
            ("Dest_Boarding", "탑승하기", 35f, 3f),
            ("Dest_Arrival", "입국심사", -40f, 4f),
            ("Dest_Immigration", "수하물 찾는 곳", -40f, 3f),
            ("Dest_Baggage", "출구", -30f, 4f),
        };

        // 기존 MoveSigns를 지우고 새로 만든다. Undo(Ctrl+Z)로 되돌릴 수 있다.
        [MenuItem("GatePass/Signs/Build Move Signs")]
        static void Build()
        {
            var destRoot = GameObject.Find(DestRootName);
            if (destRoot == null)
            {
                Debug.LogError($"[MoveSignBuilder] '{DestRootName}' 오브젝트를 찾을 수 없습니다. Airport_Lee Scene을 열어 주세요.");
                return;
            }
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (font == null)
            {
                Debug.LogError($"[MoveSignBuilder] 폰트를 찾을 수 없습니다: {FontPath}");
                return;
            }
            var material = GetOrCreateBoardMaterial();

            var old = GameObject.Find(RootName);
            if (old != null)
            {
                Undo.DestroyObjectImmediate(old);
            }
            var root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Build Move Signs");

            int created = 0;
            foreach (var (from, text, yawOffset, distance) in Signs)
            {
                var dest = destRoot.transform.Find(from);
                if (dest == null)
                {
                    Debug.LogError($"[MoveSignBuilder] '{from}' 지점을 찾을 수 없습니다.");
                    continue;
                }
                if (!FindClearPlacement(dest, yawOffset, distance, out float placedYaw, out float placedDistance))
                {
                    Debug.LogWarning($"[MoveSignBuilder] {from} '{text}': 가리지 않는 자리를 찾지 못해 표의 위치에 둡니다. 표 값을 조정해 주세요.");
                    placedYaw = yawOffset;
                    placedDistance = distance;
                }
                else if (!Mathf.Approximately(placedYaw, yawOffset) || !Mathf.Approximately(placedDistance, distance))
                {
                    Debug.Log($"[MoveSignBuilder] {from} '{text}': 표의 위치({yawOffset:+0;-0;0}°, {distance}m)가 가려져 " +
                              $"{placedYaw:+0;-0;0}°, {placedDistance}m로 옮겼습니다.");
                }
                CreateSign(root.transform, dest, text, placedYaw, placedDistance, font, material);
                created++;
            }

            EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log($"[MoveSignBuilder] 이동 표지판 {created}개 생성. Scene을 저장해 주세요.", root);
        }

        const float SearchMaxYaw = 40f;
        const float SearchMinDistance = 2f;
        const float SearchMaxDistance = 6f;

        // 표의 위치에서 가까운 순서로 정면 ±40°, 2~6m 후보를 시험해, 눈에서 판의 가운데와 네 모서리가
        // 모두 보이고 판이 다른 Collider와 겹치지 않는 첫 자리를 고른다.
        static bool FindClearPlacement(Transform dest, float preferredYaw, float preferredDistance,
            out float yawOffset, out float distance)
        {
            var candidates = new List<(float yaw, float distance, float cost)>();
            for (float yaw = -SearchMaxYaw; yaw <= SearchMaxYaw + 0.01f; yaw += 5f)
            {
                for (float d = SearchMinDistance; d <= SearchMaxDistance + 0.01f; d += 0.5f)
                {
                    float cost = Mathf.Abs(yaw - preferredYaw) / 5f + Mathf.Abs(d - preferredDistance) / 0.5f;
                    candidates.Add((yaw, d, cost));
                }
            }
            candidates.Insert(0, (preferredYaw, preferredDistance, -1f));
            candidates.Sort((a, b) => a.cost.CompareTo(b.cost));

            Physics.SyncTransforms();
            foreach (var candidate in candidates)
            {
                GetPlacement(dest, candidate.yaw, candidate.distance, out var position, out var rotation);
                if (IsClear(dest.position + Vector3.up * EyeHeight, position, rotation))
                {
                    yawOffset = candidate.yaw;
                    distance = candidate.distance;
                    return true;
                }
            }
            yawOffset = preferredYaw;
            distance = preferredDistance;
            return false;
        }

        static bool IsClear(Vector3 eye, Vector3 center, Quaternion rotation)
        {
            if (Physics.CheckBox(center, BoardSize / 2f, rotation, ~0, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            var right = rotation * Vector3.right * (BoardSize.x / 2f);
            var up = rotation * Vector3.up * (BoardSize.y / 2f);
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
        static void GetPlacement(Transform dest, float yawOffset, float distance, out Vector3 position, out Quaternion rotation)
        {
            var direction = Quaternion.Euler(0f, dest.eulerAngles.y + yawOffset, 0f) * Vector3.forward;
            position = dest.position + direction * distance + Vector3.up * BoardCenterHeight;
            var toSign = position - (dest.position + Vector3.up * EyeHeight);
            toSign.y = 0f;
            rotation = Quaternion.LookRotation(toSign);
        }

        static void CreateSign(Transform root, Transform dest, string text, float yawOffset, float distance,
            TMP_FontAsset font, Material material)
        {
            GetPlacement(dest, yawOffset, distance, out var position, out var rotation);
            var sign = new GameObject($"MoveSign_{dest.name}");
            sign.transform.SetParent(root, false);
            sign.transform.SetPositionAndRotation(position, rotation);
            sign.AddComponent<BoxCollider>().size = BoardSize;

            var board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "Board";
            Object.DestroyImmediate(board.GetComponent<Collider>());
            board.transform.SetParent(sign.transform, false);
            board.transform.localScale = BoardSize;
            board.GetComponent<MeshRenderer>().sharedMaterial = material;

            var label = new GameObject("Label");
            label.transform.SetParent(sign.transform, false);
            label.transform.localPosition = new Vector3(0f, 0f, -BoardSize.z / 2f - 0.005f);
            var tmp = label.AddComponent<TextMeshPro>();
            tmp.font = font;
            tmp.text = text;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 0.1f;
            tmp.fontSizeMax = 10f;
            tmp.rectTransform.sizeDelta = new Vector2(BoardSize.x - 0.1f, BoardSize.y - 0.08f);
        }

        // 표지판 판이 모두 같은 Material을 쓰도록 하나만 만들어 공유한다(§24 Material 복제 금지).
        static Material GetOrCreateBoardMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(BoardMaterialPath);
            if (material != null)
            {
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

            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = BoardColor };
            AssetDatabase.CreateAsset(material, BoardMaterialPath);
            AssetDatabase.SaveAssets();
            return material;
        }
    }
}
