// Dest_ 지점마다 눈높이 사방 시야와 위에서 본 모습을 PNG로 저장하는 배치 점검용 Editor 도구
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GatePassVR.EditorTools
{
    public static class DestViewCapture
    {
        const string RootName = "DestinationPoints";
        const string OutputFolder = "LayoutCaptures";
        const float EyeHeight = 1.6f;
        const float TopDownHeight = 3f;
        const float TopDownHalfSize = 10f;
        const int Width = 1280;
        const int Height = 720;

        // 결과: 프로젝트 루트/LayoutCaptures/{지점이름}_{000|090|180|270|fwd|top}.png
        // 000은 +Z(북쪽), 090은 +X 방향, fwd는 지점이 바라보는 방향. top 이미지는 위쪽이 +Z다.
        [MenuItem("GatePass/Capture Dest Views")]
        static void CaptureAll()
        {
            var root = GameObject.Find(RootName);
            if (root == null)
            {
                Debug.LogError($"[DestViewCapture] '{RootName}' 오브젝트를 찾을 수 없습니다. Airport_Lee Scene을 열어 주세요.");
                return;
            }

            Directory.CreateDirectory(OutputFolder);

            var cameraObject = new GameObject("DestViewCaptureCamera") { hideFlags = HideFlags.HideAndDontSave };
            var cam = cameraObject.AddComponent<Camera>();
            cam.enabled = false;
            cam.fieldOfView = 90f;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 300f;
            var renderTexture = new RenderTexture(Width, Height, 24);
            cam.targetTexture = renderTexture;

            try
            {
                foreach (Transform dest in root.transform)
                {
                    cam.orthographic = false;
                    for (int yaw = 0; yaw < 360; yaw += 90)
                    {
                        cam.transform.SetPositionAndRotation(dest.position + Vector3.up * EyeHeight, Quaternion.Euler(0f, yaw, 0f));
                        Save(cam, renderTexture, $"{dest.name}_{yaw:000}.png");
                    }

                    // 지점이 실제로 바라보는 방향 (이동 후 플레이어가 처음 보는 화면)
                    cam.transform.SetPositionAndRotation(dest.position + Vector3.up * EyeHeight, Quaternion.Euler(0f, dest.eulerAngles.y, 0f));
                    Save(cam, renderTexture, $"{dest.name}_fwd.png");

                    cam.orthographic = true;
                    cam.orthographicSize = TopDownHalfSize;
                    cam.transform.SetPositionAndRotation(dest.position + Vector3.up * TopDownHeight, Quaternion.Euler(90f, 0f, 0f));
                    Save(cam, renderTexture, $"{dest.name}_top.png");
                }
            }
            finally
            {
                cam.targetTexture = null;
                Object.DestroyImmediate(cameraObject);
                renderTexture.Release();
                Object.DestroyImmediate(renderTexture);
            }

            Debug.Log($"[DestViewCapture] {root.transform.childCount}개 지점 캡처 완료: {Path.GetFullPath(OutputFolder)}");
        }

        static void Save(Camera cam, RenderTexture renderTexture, string fileName)
        {
            cam.Render();

            var previous = RenderTexture.active;
            RenderTexture.active = renderTexture;
            var texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();
            RenderTexture.active = previous;

            File.WriteAllBytes(Path.Combine(OutputFolder, fileName), texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }
    }
}
