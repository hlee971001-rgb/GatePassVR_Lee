// Dest_ 지점마다 바닥 유무, 몸 겹침, 손 닿는 거리의 상호작용 대상을 Physics로 검사하는 배치 점검용 Editor 도구
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GatePassVR.EditorTools
{
    public static class AirportLayoutValidator
    {
        const string DestRootName = "DestinationPoints";
        const string ModelRootName = "Airport7_FINALc_EscenaMontada";
        const string ReportFolder = "LayoutCaptures";

        const float FloorSearchUp = 0.5f;
        const float FloorSearchDown = 1.0f;
        const float FloorTolerance = 0.15f;
        const float BodyRadius = 0.25f;
        const float BodyBottom = 0.3f;
        const float BodyTop = 1.7f;
        // 손이 닿는 범위: 정면 좌우 30°, 높이 0.5~1.3m, 거리 1m
        const float ReachDistance = 1.0f;
        const float ReachHalfAngle = 30f;
        const float ReachMinHeight = 0.5f;
        const float ReachMaxHeight = 1.3f;
        // 손 닿는 범위에 대상이 없을 때 지점을 옮길 근거를 찾는 탐색 범위
        const float SearchDistance = 3f;
        const float SearchHalfAngle = 90f;
        const float SearchMinHeight = 0.2f;
        static readonly HashSet<string> NoInteraction = new HashSet<string> { "Dest_Start", "Dest_Arrival" };

        // 현재 Scene에 실제로 있는 Collider만으로 검사한다. ② 완료 기준: 경고 0개.
        [MenuItem("GatePass/Layout/Validate Colliders")]
        static void ValidateColliders() => Run(false);

        // 공항 모델 전체에 임시 Mesh Collider를 만들어 검사한 뒤 지운다. Scene은 바뀌지 않는다.
        // Collider를 어디에 둘지 정하기 위한 측정용이다.
        [MenuItem("GatePass/Layout/Analyze Model (Temporary Colliders)")]
        static void AnalyzeModel() => Run(true);

        static void Run(bool analyzeModel)
        {
            var destRoot = GameObject.Find(DestRootName);
            if (destRoot == null)
            {
                Debug.LogError($"[AirportLayoutValidator] '{DestRootName}' 오브젝트를 찾을 수 없습니다. Airport_Lee Scene을 열어 주세요.");
                return;
            }

            var temporaryObjects = new List<GameObject>();
            if (analyzeModel && !CreateTemporaryModelColliders(temporaryObjects))
            {
                return;
            }

            Physics.SyncTransforms();
            var report = new StringBuilder();
            int warnings = 0;
            try
            {
                foreach (Transform dest in destRoot.transform)
                {
                    warnings += Check(dest, report);
                }
            }
            finally
            {
                foreach (var temporary in temporaryObjects)
                {
                    Object.DestroyImmediate(temporary);
                }
                Physics.SyncTransforms();
            }

            string mode = analyzeModel ? "Analyze" : "Validate";
            report.Insert(0, $"[{mode}] 경고 {warnings}개\n\n");
            Directory.CreateDirectory(ReportFolder);
            string path = Path.Combine(ReportFolder, $"LayoutReport_{mode}.txt");
            File.WriteAllText(path, report.ToString(), new UTF8Encoding(false));

            string summary = $"[AirportLayoutValidator] {mode} 경고 {warnings}개. 상세: {Path.GetFullPath(path)}\n{report}";
            if (warnings == 0)
            {
                Debug.Log(summary);
            }
            else
            {
                Debug.LogWarning(summary);
            }
        }

        static bool CreateTemporaryModelColliders(List<GameObject> created)
        {
            var model = GameObject.Find(ModelRootName);
            if (model == null)
            {
                Debug.LogError($"[AirportLayoutValidator] 공항 모델 '{ModelRootName}'을 찾을 수 없습니다.");
                return false;
            }

            // 모델 자체에 컴포넌트를 붙이지 않고, 같은 위치에 숨김 오브젝트를 만들어 Scene 변경을 남기지 않는다.
            foreach (var filter in model.GetComponentsInChildren<MeshFilter>())
            {
                if (filter.sharedMesh == null)
                {
                    continue;
                }

                var source = filter.transform;
                var temporary = new GameObject(source.name) { hideFlags = HideFlags.HideAndDontSave };
                temporary.transform.SetPositionAndRotation(source.position, source.rotation);
                temporary.transform.localScale = source.lossyScale;
                temporary.AddComponent<MeshCollider>().sharedMesh = filter.sharedMesh;
                created.Add(temporary);
            }
            return true;
        }

        static int Check(Transform dest, StringBuilder report)
        {
            int warnings = 0;
            Vector3 p = dest.position;
            report.AppendLine($"## {dest.name}  위치 {p}  방향 {dest.eulerAngles.y:0}°");

            // 1. 발밑 바닥
            Collider floorCollider = null;
            if (Physics.Raycast(p + Vector3.up * FloorSearchUp, Vector3.down, out var floor,
                    FloorSearchUp + FloorSearchDown, ~0, QueryTriggerInteraction.Ignore))
            {
                floorCollider = floor.collider;
                float gap = p.y - floor.point.y;
                report.AppendLine($"- 바닥: {Describe(floor.collider)}, 바닥 높이 {floor.point.y:0.00}, 지점과 차이 {gap:0.00}m");
                if (Mathf.Abs(gap) > FloorTolerance)
                {
                    warnings++;
                    report.AppendLine($"  ! 경고: 지점이 바닥에서 {gap:0.00}m 떨어져 있음");
                }
            }
            else
            {
                warnings++;
                report.AppendLine("  ! 경고: 발밑에 바닥 Collider 없음");
            }

            // 2. 서 있는 몸(반지름 0.25m, 높이 0.3~1.7m)과 겹치는 Collider
            var overlaps = Physics.OverlapCapsule(p + Vector3.up * BodyBottom, p + Vector3.up * BodyTop,
                BodyRadius, ~0, QueryTriggerInteraction.Ignore);
            if (overlaps.Length > 0)
            {
                warnings++;
                var names = new List<string>();
                foreach (var overlap in overlaps)
                {
                    names.Add(Describe(overlap));
                }
                report.AppendLine($"  ! 경고: 몸과 겹침: {string.Join(", ", names)}");
            }

            // 3. 손이 닿는 범위(정면 좌우 30°, 높이 0.5~1.3m, 1m)의 상호작용 대상
            if (!NoInteraction.Contains(dest.name))
            {
                if (SweepNearest(dest, floorCollider, ReachDistance, ReachHalfAngle, ReachMinHeight, ReachMaxHeight,
                        out var reach, out float reachAngle))
                {
                    report.AppendLine($"- 손 닿는 대상: {DescribeHit(dest, reach, reachAngle)}");
                }
                else
                {
                    warnings++;
                    report.AppendLine($"  ! 경고: 손 닿는 범위(정면 ±{ReachHalfAngle:0}°, {ReachDistance}m) 안에 상호작용 대상 Collider 없음");
                    if (SweepNearest(dest, floorCollider, SearchDistance, SearchHalfAngle, SearchMinHeight, ReachMaxHeight,
                            out var nearest, out float nearestAngle))
                    {
                        report.AppendLine($"  - 주변 탐색: 가장 가까운 대상 {DescribeHit(dest, nearest, nearestAngle)}");
                    }
                    else
                    {
                        report.AppendLine($"  - 주변 탐색: 정면 ±{SearchHalfAngle:0}°, {SearchDistance}m 안에 대상 없음");
                    }
                }
            }

            report.AppendLine();
            return warnings;
        }

        // 정면 기준 부채꼴을 높이 0.1m, 각도 5° 간격으로 훑어 바닥을 뺀 가장 가까운 Collider를 찾는다.
        static bool SweepNearest(Transform dest, Collider floorCollider, float distance, float halfAngle,
            float minHeight, float maxHeight, out RaycastHit nearest, out float nearestAngle)
        {
            nearest = default;
            nearestAngle = 0f;
            bool found = false;
            for (float height = minHeight; height <= maxHeight + 0.001f; height += 0.1f)
            {
                for (float angle = -halfAngle; angle <= halfAngle + 0.001f; angle += 5f)
                {
                    var direction = Quaternion.Euler(0f, angle, 0f) * dest.forward;
                    var hits = Physics.RaycastAll(dest.position + Vector3.up * height, direction,
                        distance, ~0, QueryTriggerInteraction.Ignore);
                    foreach (var hit in hits)
                    {
                        if (hit.collider == floorCollider || (found && hit.distance >= nearest.distance))
                        {
                            continue;
                        }
                        nearest = hit;
                        nearestAngle = angle;
                        found = true;
                    }
                }
            }
            return found;
        }

        static string DescribeHit(Transform dest, RaycastHit hit, float angle)
        {
            return $"{Describe(hit.collider)}, 거리 {hit.distance:0.00}m, 정면 기준 {angle:+0;-0;0}° 방향, " +
                   $"바닥에서 {hit.point.y - dest.position.y:0.00}m 높이";
        }

        static string Describe(Collider collider)
        {
            string text = $"{collider.name} ({collider.GetType().Name})";
            if (collider is MeshCollider meshCollider && meshCollider.sharedMesh != null)
            {
                var mesh = meshCollider.sharedMesh;
                long triangles = 0;
                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    triangles += mesh.GetIndexCount(i) / 3;
                }
                text += $" [삼각형 {triangles:N0}개, 크기 {collider.bounds.size.x:0.0}x{collider.bounds.size.y:0.0}x{collider.bounds.size.z:0.0}m]";
            }
            return text;
        }
    }
}
