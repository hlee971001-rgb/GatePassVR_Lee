// Dest_ 지점마다 눈높이 사방 시야, 위에서 본 모습, Quest 3S 시야(서서·앉아서)를 PNG로 저장하는 배치 점검용 Editor 도구
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GatePassVR.EditorTools
{
    public static class DestViewCapture
    {
        const string RootName = "DestinationPoints";
        const string OutputFolder = "LayoutCaptures";
        const float EyeHeight = SignViewRules.StandingEyeHeight;
        const float TopDownHeight = 3f;
        const float TopDownHalfSize = 10f;
        const int Width = 1280;
        const int Height = 720;
        const int Quest3SHeight = 960;
        static readonly Color GuideColor = new Color(0.2f, 1f, 0.3f);

        // 결과: 프로젝트 루트/LayoutCaptures/{지점이름}_{000|090|180|270|fwd|top|q3s|q3s_seated}.png
        // 000은 +Z(북쪽), 090은 +X 방향, fwd는 지점이 바라보는 방향. top 이미지는 위쪽이 +Z다.
        // q3s는 Quest 3S 시야(좌우 96°, 위아래 90°)로 바라보는 방향을 찍은 것이고, 초록 선 안이 편하게 보이는 범위
        // (좌우 ±30°, 위 25°)다. q3s_seated는 앉은 눈높이(1.2m) 기준이다.
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

            // 세로 시야 90°에서 가로 시야가 96°가 되도록 가로 크기를 정한다.
            int quest3SWidth = Mathf.RoundToInt(Quest3SHeight * Mathf.Tan(SignViewRules.Quest3SHorizontalFov / 2f * Mathf.Deg2Rad)
                                                / Mathf.Tan(SignViewRules.Quest3SVerticalFov / 2f * Mathf.Deg2Rad));
            var quest3STexture = new RenderTexture(quest3SWidth, Quest3SHeight, 24);

            try
            {
                foreach (Transform dest in root.transform)
                {
                    cam.targetTexture = renderTexture;
                    cam.fieldOfView = 90f;
                    cam.orthographic = false;
                    for (int yaw = 0; yaw < 360; yaw += 90)
                    {
                        cam.transform.SetPositionAndRotation(dest.position + Vector3.up * EyeHeight, Quaternion.Euler(0f, yaw, 0f));
                        Save(cam, renderTexture, $"{dest.name}_{yaw:000}.png", false);
                    }

                    // 지점이 실제로 바라보는 방향 (이동 후 플레이어가 처음 보는 화면)
                    var facing = Quaternion.Euler(0f, dest.eulerAngles.y, 0f);
                    cam.transform.SetPositionAndRotation(dest.position + Vector3.up * EyeHeight, facing);
                    Save(cam, renderTexture, $"{dest.name}_fwd.png", false);

                    cam.targetTexture = quest3STexture;
                    cam.fieldOfView = SignViewRules.Quest3SVerticalFov;
                    cam.transform.SetPositionAndRotation(SignViewRules.Eye(dest, SignViewRules.StandingEyeHeight), facing);
                    Save(cam, quest3STexture, $"{dest.name}_q3s.png", true);
                    cam.transform.SetPositionAndRotation(SignViewRules.Eye(dest, SignViewRules.SeatedEyeHeight), facing);
                    Save(cam, quest3STexture, $"{dest.name}_q3s_seated.png", true);

                    cam.targetTexture = renderTexture;
                    cam.orthographic = true;
                    cam.orthographicSize = TopDownHalfSize;
                    cam.transform.SetPositionAndRotation(dest.position + Vector3.up * TopDownHeight, Quaternion.Euler(90f, 0f, 0f));
                    Save(cam, renderTexture, $"{dest.name}_top.png", false);
                }
            }
            finally
            {
                cam.targetTexture = null;
                Object.DestroyImmediate(cameraObject);
                renderTexture.Release();
                Object.DestroyImmediate(renderTexture);
                quest3STexture.Release();
                Object.DestroyImmediate(quest3STexture);
            }

            Debug.Log($"[DestViewCapture] {root.transform.childCount}개 지점 캡처 완료: {Path.GetFullPath(OutputFolder)}");
        }

        static void Save(Camera cam, RenderTexture renderTexture, string fileName, bool drawComfortGuide)
        {
            cam.Render();

            var previous = RenderTexture.active;
            RenderTexture.active = renderTexture;
            var texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            RenderTexture.active = previous;

            if (drawComfortGuide)
            {
                DrawComfortGuide(texture);
            }
            texture.Apply();

            File.WriteAllBytes(Path.Combine(OutputFolder, fileName), texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }

        // 편하게 보이는 범위(좌우 ±MaxYaw, 위 MaxElevation)를 초록 선으로 그린다. 화면 가운데 세로줄 기준의 근사다.
        static void DrawComfortGuide(Texture2D texture)
        {
            int w = texture.width;
            int h = texture.height;
            float tanHalfH = Mathf.Tan(SignViewRules.Quest3SHorizontalFov / 2f * Mathf.Deg2Rad);
            float tanHalfV = Mathf.Tan(SignViewRules.Quest3SVerticalFov / 2f * Mathf.Deg2Rad);
            int left = Mathf.RoundToInt(w / 2f - w / 2f * Mathf.Tan(SignViewRules.MaxYaw * Mathf.Deg2Rad) / tanHalfH);
            int right = w - 1 - left;
            int top = Mathf.RoundToInt(h / 2f + h / 2f * Mathf.Tan(SignViewRules.MaxElevation * Mathf.Deg2Rad) / tanHalfV);

            for (int t = -1; t <= 1; t++)
            {
                for (int y = 0; y <= top; y++)
                {
                    texture.SetPixel(left + t, y, GuideColor);
                    texture.SetPixel(right + t, y, GuideColor);
                }
                for (int x = left; x <= right; x++)
                {
                    texture.SetPixel(x, top + t, GuideColor);
                }
            }
        }
    }
}
