// 잡는 물건마다 "손에 쥐는 기준점"(GrabPoint)을 만들어 XRGrabInteractable.attachTransform에 연결하는 에디터 도구 (이씨).
// 기준점이 없으면 물건 한가운데가 컨트롤러 기준점(손바닥 약 9cm 앞, 엄지 위)에 붙어서 엄지 위에 얹힌 것처럼 보였다.
// 손 모양은 ControllerHandPose로 컨트롤러를 쥔 주먹으로 고정되어 있으므로, 물건이 그 주먹 안쪽 빈 곳에 오도록 기준점을 계산한다.
// 사용: 메뉴 GatePassVR/Build Grab Points, 또는 배치 모드 -executeMethod GatePassVR.Editor.GrabPointBuilder.BuildAndApply
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace GatePassVR.Editor
{
    public static class GrabPointBuilder
    {
        const string ScenePath = "Assets/_GatePassVR/Scenes/Airport_Lee.unity";
        const string PointName = "GrabPoint";

        // 컨트롤러 기준점(Near-Far Interactor attach) 기준 주먹 안쪽 빈 곳. 손 관절 위치를 재서 정했다(좌우 손 대칭이라 x=0).
        static readonly Vector3 FistCenter = new Vector3(0f, -0.05f, -0.075f);

        enum Style { Card, HangFlat, HangUpright }

        // grip: 쥐는 곳(기준 프레임 로컬). Card·HangFlat는 물건 오브젝트 로컬(스케일 전 단위), HangUpright는 메시 프레임(미터)
        static readonly (string obj, Style style, Vector3 grip)[] Items =
        {
            ("Obj_Passport_Start", Style.Card, new Vector3(0f, 0f, -0.30f)),       // 아래쪽 1/5을 쥐고 위로 세움
            ("Obj_BoardingPass_CheckIn", Style.Card, new Vector3(0f, 0f, -0.40f)), // 길어서(20cm) 더 끝을 쥠
            ("Obj_Laptop_Security", Style.HangFlat, new Vector3(0f, 0f, 0.42f)),  // 한쪽 가장자리를 쥐고 아래로 늘어뜨림
            ("Obj_Bag_Security", Style.HangUpright, new Vector3(0f, 0.566f, 0f)), // 손잡이 없는 상자라 윗면 2.5cm 위를 쥠(주먹이 파묻히지 않게)
            ("Obj_Carrier_CheckIn", Style.HangUpright, Vector3.zero),             // 캐리어: 손잡이 위 막대(메시에서 계산)
            ("Obj_BaggageBag_Baggage", Style.HangUpright, Vector3.zero),
        };

        [MenuItem("GatePassVR/Build Grab Points")]
        public static void BuildMenu() => Run(save: true, captureDir: null);

        public static void BuildAndApply()
        {
            Run(save: true, captureDir: null);
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }

        // 저장하지 않고 손에 쥔 모습만 캡처(손가락을 굽혀 보여 주기 위해 ControllerHandPose를 적용하므로 저장하면 안 됨)
        public static void CaptureOnly()
        {
            Run(save: false, captureDir: System.Environment.GetEnvironmentVariable("CAP_DIR"));
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }

        static void Run(bool save, string captureDir)
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var attach = FindRightAttach();
            if (attach == null) { Debug.LogError("[GrabPoint] right near-far interactor not found"); return; }
            if (captureDir != null)
                foreach (var pose in Object.FindObjectsByType<GatePassVR.VR.ControllerHandPose>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    pose.Apply();

            foreach (var (objName, style, gripIn) in Items)
            {
                var go = GameObject.Find(objName);
                if (go == null) { Debug.LogError("[GrabPoint] not found " + objName); continue; }
                var item = go.transform;
                var visual = item.Find("SuitcaseVisual");
                var refT = style == Style.HangUpright && visual != null ? visual : item;
                var grip = gripIn;
                if (objName.StartsWith("Obj_Carrier") || objName.StartsWith("Obj_BaggageBag"))
                    grip = CarrierHandleTop(visual.GetComponent<MeshFilter>().sharedMesh);

                // 기준 프레임이 손(기준점) 공간에서 가질 방향
                Quaternion refInHand = style switch
                {
                    // 길이(z)가 위로 조금 앞으로, 두께(y)가 옆(x)으로: 주먹에 세워 쥔 카드
                    Style.Card => Quaternion.LookRotation(new Vector3(0f, 1f, 0.35f).normalized, Vector3.right),
                    // 길이(z)가 위로 가되 쥐는 곳이 +z 끝이라 몸체는 아래로 늘어진다
                    Style.HangFlat => Quaternion.LookRotation(Vector3.up, Vector3.right),
                    // 폭(x)이 주먹 축(앞뒤, z)을 따라가고 위(y)는 위: 손잡이를 쥐고 매단 모양
                    _ => Quaternion.LookRotation(Vector3.left, Vector3.up),
                };

                var origPos = item.position;
                var origRot = item.rotation;
                var refLocalRot = Quaternion.Inverse(item.rotation) * refT.rotation;

                item.rotation = attach.rotation * refInHand * Quaternion.Inverse(refLocalRot);
                var gripWorld = refT.TransformPoint(grip);
                item.position += attach.TransformPoint(FistCenter) - gripWorld;

                var point = item.Find(PointName);
                if (point == null)
                {
                    point = new GameObject(PointName).transform;
                    point.SetParent(item, false);
                }
                point.SetPositionAndRotation(attach.position, attach.rotation);

                if (captureDir != null)
                {
                    Shot(attach, new Vector3(-0.30f, 0.12f, -0.30f), Path.Combine(captureDir, $"grab_{objName}_a.png"));
                    Shot(attach, new Vector3(0.35f, 0.02f, 0.05f), Path.Combine(captureDir, $"grab_{objName}_b.png"));
                }

                item.SetPositionAndRotation(origPos, origRot);

                var grab = go.GetComponent<XRGrabInteractable>();
                grab.attachTransform = point;
                EditorUtility.SetDirty(grab);
                Debug.Log($"[GrabPoint] {objName}: local pos {point.localPosition} rot {point.localRotation.eulerAngles}");
            }

            if (save)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        static Transform FindRightAttach()
        {
            var near = Object.FindObjectsByType<NearFarInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(i => i.transform.parent != null && i.transform.parent.name == "Right Controller");
            return near != null ? near.GetAttachTransform(null) : null;
        }

        // 캐리어 손잡이 맨 위 막대의 가운데(메시 로컬). 금속(서브메시 1) 정점 중 가장 높은 2cm 안쪽의 평균.
        static Vector3 CarrierHandleTop(Mesh mesh)
        {
            var verts = mesh.vertices;
            var idx = mesh.GetTriangles(1).Distinct().Select(i => verts[i]).ToArray();
            float top = idx.Max(v => v.y);
            var near = idx.Where(v => v.y > top - 0.02f).ToArray();
            var c = near.Aggregate(Vector3.zero, (a, v) => a + v) / near.Length;
            return c + Vector3.down * 0.01f;
        }

        static void Shot(Transform attach, Vector3 camOffset, string path)
        {
            var go = new GameObject("TempCam");
            var cam = go.AddComponent<Camera>();
            cam.fieldOfView = 50f;
            cam.nearClipPlane = 0.01f;
            go.transform.position = attach.TransformPoint(camOffset);
            go.transform.LookAt(attach.TransformPoint(FistCenter + new Vector3(0f, -0.02f, 0f)));
            var rt = new RenderTexture(960, 720, 24) { antiAliasing = 4 };
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(960, 720, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 960, 720), 0, 0);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            RenderTexture.active = null;
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(rt);
        }
    }
}
