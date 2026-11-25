// 여권 겉모양(표지 그림 아틀라스·상자 메시·머티리얼)을 직접 만드는 에디터 도구 (이씨).
// 외부 에셋 없이 Noto Sans KR(OFL) 글자와 선으로 그려서 라이선스 걱정이 없다. 나라 문장 대신 지구본 문양을 쓴다.
// 사용: 메뉴 GatePassVR/Build Passport Art, 또는 배치 모드 -executeMethod GatePassVR.Editor.PassportArtBuilder.BuildAndApply
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GatePassVR.Editor
{
    public static class PassportArtBuilder
    {
        const string Folder = "Assets/_GatePassVR/Art/Props/Passport";
        const string TexturePath = Folder + "/T_Passport.png";
        const string MeshPath = Folder + "/Passport_Mesh.asset";
        const string MaterialPath = Folder + "/M_Passport.mat";
        const string FontPath = "Assets/Fonts/NotoSansKR-Medium SDF Dynamic.asset";
        const string ScenePath = "Assets/_GatePassVR/Scenes/Airport_Lee.unity";
        const string PassportName = "Obj_Passport_Start";

        // 아틀라스 2048x2048 (텍스처 좌표는 왼쪽 아래가 원점)
        const int Atlas = 2048;
        static readonly RectInt FrontRect = new RectInt(1024, 592, 1024, 1456); // 앞표지 (88:125 비율)
        static readonly RectInt BackRect = new RectInt(0, 592, 1024, 1456);     // 뒤표지
        static readonly RectInt SpineRect = new RectInt(0, 296, 2048, 296);     // 책등
        static readonly RectInt PaperRect = new RectInt(0, 0, 2048, 296);       // 속지 옆면

        static readonly Color Navy = new Color32(0x1F, 0x2D, 0x4F, 0xFF);
        static readonly Color Gold = new Color32(0xD4, 0xB2, 0x66, 0xFF);
        static readonly Color Paper = new Color32(0xF4, 0xF0, 0xE2, 0xFF);
        static readonly Color PaperLine = new Color32(0xD8, 0xD2, 0xC2, 0xFF);

        [MenuItem("GatePassVR/Build Passport Art")]
        public static void BuildAndApplyMenu()
        {
            BuildAssets();
            ApplyToScene();
        }

        public static void BuildAndApply()
        {
            BuildAssets();
            ApplyToScene();
            if (Application.isBatchMode)
                EditorApplication.Exit(0);
        }

        static void BuildAssets()
        {
            Directory.CreateDirectory(Folder);
            var front = RenderFrontCover();
            var atlas = new Texture2D(Atlas, Atlas, TextureFormat.RGBA32, false);
            Fill(atlas, new RectInt(0, 0, Atlas, Atlas), Navy);
            PaintPaper(atlas);
            atlas.SetPixels(FrontRect.x, FrontRect.y, FrontRect.width, FrontRect.height, front.GetPixels());
            atlas.Apply();
            File.WriteAllBytes(TexturePath, atlas.EncodeToPNG());
            Object.DestroyImmediate(front);
            Object.DestroyImmediate(atlas);

            AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
            importer.maxTextureSize = 1024; // Quest용
            importer.anisoLevel = 4;        // 비스듬히 볼 때 글자가 덜 뭉개지게
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.SaveAndReimport();
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);

            var mesh = BuildMesh();
            var oldMesh = AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath);
            if (oldMesh != null)
            {
                EditorUtility.CopySerialized(mesh, oldMesh);
                Object.DestroyImmediate(mesh);
            }
            else
            {
                AssetDatabase.CreateAsset(mesh, MeshPath);
            }

            var mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, MaterialPath);
            }
            mat.SetTexture("_BaseMap", tex);
            mat.SetColor("_BaseColor", Color.white);
            mat.SetFloat("_Smoothness", 0.25f);
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            Debug.Log("[PassportArt] assets built");
        }

        static void ApplyToScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var go = GameObject.Find(PassportName);
            if (go == null)
            {
                Debug.LogError("[PassportArt] passport not found");
                return;
            }
            go.GetComponent<MeshFilter>().sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath);
            go.GetComponent<MeshRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[PassportArt] applied to " + PassportName);
        }

        // 앞표지: 빈 장면에 글자·선을 놓고 정사영 카메라로 찍는다. 단위는 표지 88x125mm를 0.88x1.25로 둔다.
        static Texture2D RenderFrontCover()
        {
            var preview = EditorSceneManager.NewPreviewScene();
            var root = new GameObject("PassportCover");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, preview);

            var lineMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            lineMat.SetColor("_BaseColor", Gold);
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

            AddText(root, font, "여권", new Vector2(0f, 0.40f), new Vector2(0.62f, 0.15f), FontStyles.Normal, 10f);
            AddText(root, font, "PASSPORT", new Vector2(0f, 0.255f), new Vector2(0.56f, 0.075f), FontStyles.Normal, 18f);
            AddText(root, font, "체험용 · NOT A TRAVEL DOCUMENT", new Vector2(0f, -0.50f), new Vector2(0.66f, 0.035f), FontStyles.Normal, 4f);

            // 테두리
            AddRect(root, lineMat, 0.39f, 0.57f, 0.006f);
            AddRect(root, lineMat, 0.375f, 0.555f, 0.003f);
            // 지구본 문양 (나라 문장 대신)
            var c = new Vector2(0f, -0.06f);
            const float r = 0.17f;
            const float w = 0.009f;
            AddEllipse(root, lineMat, c, r, r, w);
            AddEllipse(root, lineMat, c, r * 0.5f, r, w);
            AddLine(root, lineMat, c + new Vector2(0f, -r), c + new Vector2(0f, r), w);
            AddLine(root, lineMat, c + new Vector2(-r, 0f), c + new Vector2(r, 0f), w);
            float chord = Mathf.Sqrt(1f - 0.25f) * r;
            AddLine(root, lineMat, c + new Vector2(-chord, r * 0.5f), c + new Vector2(chord, r * 0.5f), w);
            AddLine(root, lineMat, c + new Vector2(-chord, -r * 0.5f), c + new Vector2(chord, -r * 0.5f), w);

            var camGo = new GameObject("CoverCam");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(camGo, preview);
            var cam = camGo.AddComponent<Camera>();
            cam.scene = preview;
            cam.orthographic = true;
            cam.orthographicSize = 0.625f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Navy;
            cam.transform.position = new Vector3(0f, 0f, -5f);
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 20f;

            var rt = new RenderTexture(FrontRect.width, FrontRect.height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 8 };
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(FrontRect.width, FrontRect.height, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, FrontRect.width, FrontRect.height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            cam.targetTexture = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(lineMat);
            EditorSceneManager.ClosePreviewScene(preview);
            return tex;
        }

        static void AddText(GameObject root, TMP_FontAsset font, string text, Vector2 pos, Vector2 size, FontStyles style, float spacing = 0f)
        {
            var go = new GameObject("Text_" + text);
            go.transform.SetParent(root.transform, false);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.font = font;
            tmp.text = text;
            tmp.fontStyle = style;
            tmp.color = Gold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 0.1f;
            tmp.fontSizeMax = 100f;
            tmp.characterSpacing = spacing;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.rectTransform.sizeDelta = size;
            tmp.rectTransform.localPosition = pos;
            tmp.ForceMeshUpdate();
        }

        static LineRenderer NewLine(GameObject root, Material mat, float width, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root.transform, false);
            var lr = go.AddComponent<LineRenderer>();
            lr.sharedMaterial = mat;
            lr.useWorldSpace = false;
            lr.widthMultiplier = width;
            lr.numCapVertices = 4;
            lr.numCornerVertices = 4;
            lr.alignment = LineAlignment.TransformZ;
            return lr;
        }

        static void AddLine(GameObject root, Material mat, Vector2 a, Vector2 b, float width)
        {
            var lr = NewLine(root, mat, width, "Line");
            lr.positionCount = 2;
            lr.SetPosition(0, a);
            lr.SetPosition(1, b);
        }

        static void AddRect(GameObject root, Material mat, float halfW, float halfH, float width)
        {
            var lr = NewLine(root, mat, width, "Rect");
            lr.loop = true;
            lr.positionCount = 4;
            lr.SetPosition(0, new Vector3(-halfW, -halfH));
            lr.SetPosition(1, new Vector3(-halfW, halfH));
            lr.SetPosition(2, new Vector3(halfW, halfH));
            lr.SetPosition(3, new Vector3(halfW, -halfH));
        }

        static void AddEllipse(GameObject root, Material mat, Vector2 c, float rx, float ry, float width)
        {
            AddArc(root, mat, c, rx, ry, 0f, 0f, 360f, width, true);
        }

        static void AddArc(GameObject root, Material mat, Vector2 c, float rx, float ry, float tiltDeg, float fromDeg, float toDeg, float width, bool loop = false)
        {
            var lr = NewLine(root, mat, width, "Arc");
            const int steps = 96;
            lr.loop = loop;
            lr.positionCount = loop ? steps : steps + 1;
            var tilt = Quaternion.Euler(0f, 0f, tiltDeg);
            for (int i = 0; i < lr.positionCount; i++)
            {
                float t = Mathf.Lerp(fromDeg, toDeg, i / (float)steps) * Mathf.Deg2Rad;
                var p = tilt * new Vector3(Mathf.Cos(t) * rx, Mathf.Sin(t) * ry, 0f);
                lr.SetPosition(i, (Vector3)c + p);
            }
        }

        static void Fill(Texture2D tex, RectInt r, Color color)
        {
            var px = new Color[r.width * r.height];
            for (int i = 0; i < px.Length; i++) px[i] = color;
            tex.SetPixels(r.x, r.y, r.width, r.height, px);
        }

        // 속지 옆면: 위아래 끝은 표지 두께(남색), 가운데는 종이색에 얇은 줄을 겹겹이
        static void PaintPaper(Texture2D tex)
        {
            const int coverPx = 16;
            for (int y = 0; y < PaperRect.height; y++)
            {
                Color row;
                if (y < coverPx || y >= PaperRect.height - coverPx) row = Navy;
                else row = ((y - coverPx) % 18 == 0) ? PaperLine : Paper;
                var px = new Color[PaperRect.width];
                for (int x = 0; x < px.Length; x++) px[x] = row;
                tex.SetPixels(PaperRect.x, PaperRect.y + y, PaperRect.width, 1, px);
            }
        }

        // 단위 상자(-0.5~0.5). 오브젝트 크기(0.088, 0.005, 0.125)로 실제 여권 크기가 된다.
        // 윗면(+y)이 앞표지, 왼쪽(-x)이 책등, 나머지 옆면은 속지.
        static Mesh BuildMesh()
        {
            var mesh = new Mesh { name = "Passport_Mesh" };
            var verts = new System.Collections.Generic.List<Vector3>();
            var normals = new System.Collections.Generic.List<Vector3>();
            var uvs = new System.Collections.Generic.List<Vector2>();
            var tris = new System.Collections.Generic.List<int>();

            void Face(Vector3 n, Vector3 up, RectInt region)
            {
                var right = Vector3.Cross(up, -n);
                var c = n * 0.5f;
                var r = right * 0.5f;
                var u = up * 0.5f;
                int start = verts.Count;
                verts.Add(c - r - u); verts.Add(c - r + u); verts.Add(c + r + u); verts.Add(c + r - u);
                for (int i = 0; i < 4; i++) normals.Add(n);
                float x0 = region.xMin / (float)Atlas, x1 = region.xMax / (float)Atlas;
                float y0 = region.yMin / (float)Atlas, y1 = region.yMax / (float)Atlas;
                uvs.Add(new Vector2(x0, y0)); uvs.Add(new Vector2(x0, y1)); uvs.Add(new Vector2(x1, y1)); uvs.Add(new Vector2(x1, y0));
                tris.Add(start); tris.Add(start + 1); tris.Add(start + 2);
                tris.Add(start); tris.Add(start + 2); tris.Add(start + 3);
            }

            Face(Vector3.up, Vector3.forward, FrontRect);
            Face(Vector3.down, Vector3.forward, BackRect);
            Face(Vector3.left, Vector3.up, SpineRect);
            Face(Vector3.right, Vector3.up, PaperRect);
            Face(Vector3.forward, Vector3.up, PaperRect);
            Face(Vector3.back, Vector3.up, PaperRect);

            mesh.SetVertices(verts);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }
    }
}
