// 보안검색 가방을 배낭 모델로 바꾸는 에디터 도구 (이씨).
// 원본: "Large Backpack - White Texture (Mockup)" by kane_sk06 (Sketchfab), CC BY 4.0
//   https://sketchfab.com/3d-models/large-backpack-white-texture-mockup-52cf030883e74170b47cd64f349aa7ce
// 바꾼 점: GLB(1K 텍스처판)를 Tools/glb_to_obj.py로 OBJ로 바꾸고, 높이 약 0.42m로 줄이고, Mesh LOD로 삼각형 수를 줄였다.
// 사용: 메뉴 GatePassVR/Build Backpack, 또는 배치 모드 -executeMethod GatePassVR.Editor.BackpackBuilder.BuildAndApply
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace GatePassVR.Editor
{
    public static class BackpackBuilder
    {
        const string Folder = "Assets/_GatePassVR/Art/Props/Backpack";
        const string SourcePath = Folder + "/Backpack.obj";
        const string BaseColorPath = Folder + "/Backpack_BaseColor.png";
        const string NormalPath = Folder + "/Backpack_Normal.png";
        const string MeshPath = Folder + "/Backpack_Mesh.asset";
        const string MaterialPath = Folder + "/M_Backpack.mat";
        const string ScenePath = "Assets/_GatePassVR/Scenes/Airport_Lee.unity";
        const string TargetName = "Obj_Bag_Security";
        public const string VisualName = "BackpackVisual";
        const float TargetHeight = 0.42f;
        const int TargetTriangles = 15000;
        // 모델 앞면이 플레이어 쪽(오브젝트 -z)을 보도록 돌리는 각도
        static readonly Quaternion VisualRotation = Quaternion.Euler(0f, 90f, 0f); // 캡처로 확인: 앞주머니가 플레이어 쪽

        [MenuItem("GatePassVR/Build Backpack")]
        public static void BuildAndApplyMenu()
        {
            var (mesh, mat) = BuildAssets();
            ApplyToScene(mesh, mat);
        }

        public static void BuildAndApply()
        {
            BuildAndApplyMenu();
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }

        static (Mesh, Material) BuildAssets()
        {
            SetTexture(BaseColorPath, false);
            SetTexture(NormalPath, true);

            var src = AssetDatabase.LoadAssetAtPath<GameObject>(SourcePath);
            var rootInv = src.transform.worldToLocalMatrix;
            var parts = new List<CombineInstance>();
            foreach (var mf in src.GetComponentsInChildren<MeshFilter>(true))
                for (int s = 0; s < mf.sharedMesh.subMeshCount; s++)
                    parts.Add(new CombineInstance { mesh = mf.sharedMesh, subMeshIndex = s, transform = rootInv * mf.transform.localToWorldMatrix });

            var full = new Mesh { indexFormat = IndexFormat.UInt32 };
            full.CombineMeshes(parts.ToArray(), true, true);
            // 높이를 맞춘다
            float scale = TargetHeight / full.bounds.size.y;
            var v = full.vertices;
            for (int i = 0; i < v.Length; i++) v[i] *= scale;
            full.vertices = v;
            full.RecalculateBounds();
            SuitcaseBuilder.CenterOnBounds(full);
            int fullTris = SuitcaseBuilder.TriCount(full);

            // Mesh LOD로 목표 이상인 가장 가벼운 단계를 고른다(한 덩어리 메시라 한꺼번에 줄여도 된다)
            var work = Object.Instantiate(full);
            MeshLodUtility.GenerateMeshLods(work, 8);
            int best = 0;
            for (int lod = 0; lod < work.lodCount; lod++)
            {
                int t = (int)work.GetLod(0, lod).indexCount / 3;
                Debug.Log($"[Backpack] lod {lod}: {t} tris");
                if (t >= TargetTriangles) best = lod; else break;
            }
            var reduced = best == 0 ? SuitcaseBuilder.CleanCopy(full) : SuitcaseBuilder.ExtractLod(work, best);
            reduced.name = "Backpack_Mesh";
            reduced.RecalculateTangents(); // 노멀 맵용
            Debug.Log($"[Backpack] scale {scale:F4}, {fullTris} → {SuitcaseBuilder.TriCount(reduced)} tris, size {reduced.bounds.size}");
            Object.DestroyImmediate(work);
            Object.DestroyImmediate(full);

            var old = AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath);
            if (old != null) { EditorUtility.CopySerialized(reduced, old); Object.DestroyImmediate(reduced); reduced = old; }
            else AssetDatabase.CreateAsset(reduced, MeshPath);

            var mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, MaterialPath);
            }
            mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(BaseColorPath));
            mat.SetColor("_BaseColor", Color.white);
            mat.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(NormalPath));
            mat.SetFloat("_BumpScale", 1f);
            mat.EnableKeyword("_NORMALMAP");
            mat.SetFloat("_Metallic", 0f);
            mat.SetFloat("_Smoothness", 0.2f); // 천 가방
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            return (reduced, mat);
        }

        static void SetTexture(string path, bool normal)
        {
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            imp.maxTextureSize = 1024;
            imp.SaveAndReimport();
        }

        static void ApplyToScene(Mesh mesh, Material mat)
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var go = GameObject.Find(TargetName);
            if (go == null) { Debug.LogError("[Backpack] not found"); return; }
            var t = go.transform;
            var visual = t.Find(VisualName);
            var box = go.GetComponent<BoxCollider>();
            var size = VisualRotation * mesh.bounds.size;
            size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));

            if (visual == null)
            {
                Vector3 oldSize = t.localScale;
                t.localScale = Vector3.one;
                var r = go.GetComponent<MeshRenderer>();
                if (r != null) r.enabled = false;
                visual = new GameObject(VisualName).transform;
                visual.SetParent(t, false);
                visual.gameObject.AddComponent<MeshFilter>();
                visual.gameObject.AddComponent<MeshRenderer>();
                box.center = Vector3.zero;
                box.size = size;
                t.position += t.up * (size.y * 0.5f - oldSize.y * 0.5f + 0.01f);
                Debug.Log($"[Backpack] old box {oldSize} → collider {size}");
            }
            else
            {
                t.position += t.up * ((size.y - box.size.y) * 0.5f);
                box.size = size;
            }
            visual.localPosition = Vector3.zero;
            visual.localRotation = VisualRotation;
            visual.localScale = Vector3.one;
            visual.GetComponent<MeshFilter>().sharedMesh = mesh;
            visual.GetComponent<MeshRenderer>().sharedMaterial = mat;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Backpack] applied to " + TargetName);
        }
    }
}
