// 공항 모델 중 바닥·카운터 등 상호작용에 필요한 시설 오브젝트에만 Mesh Collider를 붙이는 Editor 도구
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GatePassVR.EditorTools
{
    public static class AirportModelColliders
    {
        const string ModelRootName = "Airport7_FINALc_EscenaMontada";

        // AirportLayoutValidator의 Analyze 결과로 고른 시설.
        // 07.Modulo1: 1·2층 바닥과 벽 / 08: 체크인 카운터 / 09: 보안검색대 / 11: 심사 부스 / 12: 수하물 컨베이어
        static readonly string[] NamePrefixes = { "07.Modulo1", "08.CheckInDesk", "09.SecurityCheck", "11.BoardingAreaDesk" };
        const string NameContains = "BaggageClaimBand";

        // Undo(Ctrl+Z)로 되돌릴 수 있다. 이미 Collider가 있는 오브젝트는 건너뛰므로 여러 번 실행해도 된다.
        [MenuItem("GatePass/Layout/Add Model Colliders")]
        static void AddColliders()
        {
            var model = GameObject.Find(ModelRootName);
            if (model == null)
            {
                Debug.LogError($"[AirportModelColliders] 공항 모델 '{ModelRootName}'을 찾을 수 없습니다. Airport_Lee Scene을 열어 주세요.");
                return;
            }

            int added = 0;
            int skipped = 0;
            long triangles = 0;
            foreach (var filter in model.GetComponentsInChildren<MeshFilter>(true))
            {
                if (filter.sharedMesh == null || !IsTarget(filter.name))
                {
                    continue;
                }
                if (filter.GetComponent<Collider>() != null)
                {
                    skipped++;
                    continue;
                }

                var meshCollider = Undo.AddComponent<MeshCollider>(filter.gameObject);
                meshCollider.sharedMesh = filter.sharedMesh;
                added++;
                for (int i = 0; i < filter.sharedMesh.subMeshCount; i++)
                {
                    triangles += filter.sharedMesh.GetIndexCount(i) / 3;
                }
                Debug.Log($"[AirportModelColliders] + {filter.name}", filter.gameObject);
            }

            if (added > 0)
            {
                EditorSceneManager.MarkSceneDirty(model.scene);
            }
            Debug.Log($"[AirportModelColliders] 추가 {added}개 (삼각형 {triangles:N0}개), 이미 있어서 건너뜀 {skipped}개. Scene을 저장해 주세요.");
        }

        static bool IsTarget(string objectName)
        {
            foreach (var prefix in NamePrefixes)
            {
                if (objectName.StartsWith(prefix))
                {
                    return true;
                }
            }
            return objectName.Contains(NameContains);
        }
    }
}
