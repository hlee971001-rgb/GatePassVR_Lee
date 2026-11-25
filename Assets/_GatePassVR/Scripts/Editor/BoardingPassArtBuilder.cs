// 탑승권 겉모양(세로형 탑승권 그림·얇은 카드 메시·머티리얼)을 직접 만드는 에디터 도구 (이씨).
// 외부 에셋 없이 Noto Sans KR(OFL) 글자와 사각형으로 그린다. 실제 항공사 이름·로고는 쓰지 않고 "GATEPASS AIR"(가상)를 쓴다.
// 바코드는 읽히지 않는 장식 줄무늬다. 카운터 위에서도, 주먹에 세워 쥘 때도 바로 읽히도록 세로형으로 그린다.
// 사용: 메뉴 GatePassVR/Build Boarding Pass Art, 또는 배치 모드 -executeMethod GatePassVR.Editor.BoardingPassArtBuilder.BuildAndApply
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GatePassVR.Editor
{
    public static class BoardingPassArtBuilder
    {
        const string Folder = "Assets/_GatePassVR/Art/Props/BoardingPass";
        const string TexturePath = Folder + "/T_BoardingPass.png";
        const string MeshPath = Folder + "/BoardingPass_Mesh.asset";
        const string MaterialPath = Folder + "/M_BoardingPass.mat";
        const string FontPath = "Assets/Fonts/NotoSansKR-Medium SDF Dynamic.asset";
        const string ScenePath = "Assets/_GatePassVR/Scenes/Airport_Lee.unity";
        const string TargetName = "Obj_BoardingPass_CheckIn";

        // 아틀라스 2048x2048. 탑승권 80x200mm(1:2.5)를 800x2000으로.
        const int Atlas = 2048;
        static readonly RectInt FrontRect = new RectInt(0, 0, 800, 2000);
        static readonly RectInt BackRect = new RectInt(800, 0, 800, 2000);
        static readonly RectInt EdgeRect = new RectInt(1700, 100, 200, 200);

        static readonly Color Paper = new Color32(0xFB, 0xFA, 0xF6, 0xFF);
        static readonly Color Brand = new Color32(0x0E, 0x5E, 0x8A, 0xFF);
        static readonly Color Ink = new Color32(0x1E, 0x2A, 0x3A, 0xFF);
        static readonly Color Label = new Color32(0x7A, 0x85, 0x94, 0xFF);
        static readonly Color Rule = new Color32(0xD5, 0xDA, 0xE0, 0xFF);

        [MenuItem("GatePassVR/Build Boarding Pass Art")]
        public static void BuildAndApplyMenu()
        {
            BuildAssets();
            ApplyToScene();
        }

        public static void BuildAndApply()
        {
            BuildAndApplyMenu();
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }

        static void BuildAssets()
        {
            Directory.CreateDirectory(Folder);
            var front = RenderFront();
            var atlas = new Texture2D(Atlas, Atlas, TextureFormat.RGBA32, false);
            var px = new Color[Atlas * Atlas];
            for (int i = 0; i < px.Length; i++) px[i] = Paper;
            atlas.SetPixels(px);
            atlas.SetPixels(FrontRect.x, FrontRect.y, FrontRect.width, FrontRect.height, front.GetPixels());
            atlas.Apply();
            File.WriteAllBytes(TexturePath, atlas.EncodeToPNG());
            Object.DestroyImmediate(front);
            Object.DestroyImmediate(atlas);

            AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
            importer.maxTextureSize = 1024;
            importer.anisoLevel = 4;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.SaveAndReimport();
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);

            var mesh = BuildMesh();
            var old = AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath);
            if (old != null) { EditorUtility.CopySerialized(mesh, old); Object.DestroyImmediate(mesh); }
            else AssetDatabase.CreateAsset(mesh, MeshPath);

            var mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, MaterialPath);
            }
            mat.SetTexture("_BaseMap", tex);
            mat.SetColor("_BaseColor", Color.white);
            mat.SetFloat("_Smoothness", 0.2f);
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            Debug.Log("[BoardingPassArt] assets built");
        }

        static void ApplyToScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var go = GameObject.Find(TargetName);
            if (go == null) { Debug.LogError("[BoardingPassArt] not found"); return; }
            go.GetComponent<MeshFilter>().sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath);
            go.GetComponent<MeshRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[BoardingPassArt] applied to " + TargetName);
        }

        // 앞면: 80x200mm를 0.8x2.0 단위로 두고 정사영 카메라로 찍는다. 위가 +y.
        static Texture2D RenderFront()
        {
            var preview = EditorSceneManager.NewPreviewScene();
            var root = new GameObject("BoardingPass");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, preview);
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            var mats = new List<Material>();
            Material Flat(Color c)
            {
                var m = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                m.SetColor("_BaseColor", c);
                mats.Add(m);
                return m;
            }
            var brand = Flat(Brand);
            var rule = Flat(Rule);
            var ink = Flat(Ink);

            // 머리 띠
            Quad(root, brand, new Vector2(0f, 0.89f), new Vector2(0.8f, 0.22f));
            Text(root, font, "GATEPASS AIR", new Vector2(0f, 0.925f), new Vector2(0.66f, 0.07f), Color.white, FontStyles.Bold, 6f);
            Text(root, font, "BOARDING PASS · 탑승권", new Vector2(0f, 0.835f), new Vector2(0.62f, 0.045f), Color.white, FontStyles.Normal, 4f);

            // 구간
            Text(root, font, "ICN", new Vector2(-0.2f, 0.63f), new Vector2(0.3f, 0.13f), Ink, FontStyles.Bold);
            Text(root, font, "NRT", new Vector2(0.2f, 0.63f), new Vector2(0.3f, 0.13f), Ink, FontStyles.Bold);
            Text(root, font, "→", new Vector2(0f, 0.635f), new Vector2(0.1f, 0.09f), Brand, FontStyles.Normal);
            Text(root, font, "서울/인천", new Vector2(-0.2f, 0.525f), new Vector2(0.3f, 0.04f), Label, FontStyles.Normal);
            Text(root, font, "도쿄/나리타", new Vector2(0.2f, 0.525f), new Vector2(0.3f, 0.04f), Label, FontStyles.Normal);
            Quad(root, rule, new Vector2(0f, 0.45f), new Vector2(0.68f, 0.006f));

            // 항목
            Field(root, font, "편명 FLIGHT", "GP 101", new Vector2(-0.18f, 0.37f));
            Field(root, font, "좌석 SEAT", "12A", new Vector2(0.18f, 0.37f));
            Field(root, font, "탑승구 GATE", "15", new Vector2(-0.18f, 0.19f));
            Field(root, font, "탑승 BOARDING", "10:20", new Vector2(0.18f, 0.19f));
            Field(root, font, "승객 PASSENGER", "GATEPASS / TRAVELER", new Vector2(0f, 0.01f), 0.66f);

            // 장식 바코드(읽히지 않음)
            Barcode(root, ink, new Vector2(0f, -0.25f), new Vector2(0.64f, 0.13f), 7);

            // 절취선과 떼는 쪽
            for (float x = -0.37f; x < 0.38f; x += 0.04f)
                Quad(root, rule, new Vector2(x + 0.01f, -0.43f), new Vector2(0.02f, 0.006f));
            Text(root, font, "GP 101 · 12A · GATE 15", new Vector2(0f, -0.53f), new Vector2(0.66f, 0.05f), Ink, FontStyles.Bold, 2f);
            Text(root, font, "ICN → NRT", new Vector2(0f, -0.61f), new Vector2(0.4f, 0.045f), Label, FontStyles.Normal, 2f);
            Barcode(root, ink, new Vector2(0f, -0.75f), new Vector2(0.5f, 0.09f), 11);

            Text(root, font, "체험용 · NOT A REAL TICKET", new Vector2(0f, -0.93f), new Vector2(0.6f, 0.035f), Label, FontStyles.Normal, 2f);

            var camGo = new GameObject("Cam");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(camGo, preview);
            var cam = camGo.AddComponent<Camera>();
            cam.scene = preview;
            cam.orthographic = true;
            cam.orthographicSize = 1.0f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Paper;
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
            foreach (var m in mats) Object.DestroyImmediate(m);
            EditorSceneManager.ClosePreviewScene(preview);
            return tex;
        }

        static void Field(GameObject root, TMP_FontAsset font, string label, string value, Vector2 pos, float width = 0.3f)
        {
            Text(root, font, label, pos + new Vector2(0f, 0.035f), new Vector2(width, 0.033f), Label, FontStyles.Normal);
            Text(root, font, value, pos + new Vector2(0f, -0.035f), new Vector2(width, 0.075f), Ink, FontStyles.Bold);
        }

        static void Text(GameObject root, TMP_FontAsset font, string text, Vector2 pos, Vector2 size, Color color, FontStyles style, float spacing = 0f)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(root.transform, false);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.font = font;
            tmp.text = text;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 0.1f;
            tmp.fontSizeMax = 100f;
            tmp.characterSpacing = spacing;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.rectTransform.sizeDelta = size;
            tmp.rectTransform.localPosition = new Vector3(pos.x, pos.y, 0f);
            tmp.ForceMeshUpdate();
        }

        static void Quad(GameObject root, Material mat, Vector2 center, Vector2 size)
        {
            var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Object.DestroyImmediate(q.GetComponent<Collider>());
            q.transform.SetParent(root.transform, false);
            q.transform.localPosition = new Vector3(center.x, center.y, 0.01f);
            q.transform.localScale = new Vector3(size.x, size.y, 1f);
            q.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        // 고정 시드로 만든 굵기 다른 세로줄
        static void Barcode(GameObject root, Material mat, Vector2 center, Vector2 size, int seed)
        {
            var rng = new System.Random(seed);
            float x = center.x - size.x * 0.5f;
            float end = center.x + size.x * 0.5f;
            while (x < end)
            {
                float w = 0.004f + 0.004f * rng.Next(0, 3);
                float gap = 0.004f + 0.004f * rng.Next(0, 2);
                if (x + w > end) break;
                Quad(root, mat, new Vector2(x + w * 0.5f, center.y), new Vector2(w, size.y));
                x += w + gap;
            }
        }

        // 단위 상자. 오브젝트 크기(0.08, 0.002, 0.2)로 실제 크기. 윗면(+y)=앞면(위쪽이 +z), 아랫면=뒷면, 옆면=종이.
        static Mesh BuildMesh()
        {
            var mesh = new Mesh { name = "BoardingPass_Mesh" };
            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

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
            Face(Vector3.left, Vector3.up, EdgeRect);
            Face(Vector3.right, Vector3.up, EdgeRect);
            Face(Vector3.forward, Vector3.up, EdgeRect);
            Face(Vector3.back, Vector3.up, EdgeRect);

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
