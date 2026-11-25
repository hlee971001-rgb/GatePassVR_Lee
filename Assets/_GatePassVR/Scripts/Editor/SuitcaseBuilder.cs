// 캐리어 모델을 Quest용으로 정리해서 씬의 캐리어 2개(체크인·수하물 찾는 곳)에 입히는 에디터 도구 (이씨).
// 원본: "Suitcase" by moh1t (Sketchfab), CC BY 4.0 — https://sketchfab.com/3d-models/suitcase-af389b9c83c146dd88c3f91246f1fb97
// 바꾼 점: 부품 130여 개를 머티리얼별 메시 하나로 합치고, Unity Mesh LOD로 삼각형 수를 줄이고, 머티리얼 색을 새로 정했다(몸통은 Sketchfab 미리보기와 같은 민트색).
// 사용: 메뉴 GatePassVR/Build Suitcase, 또는 배치 모드 -executeMethod GatePassVR.Editor.SuitcaseBuilder.BuildAndApply
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace GatePassVR.Editor
{
    public static class SuitcaseBuilder
    {
        const string Folder = "Assets/_GatePassVR/Art/Props/Suitcase";
        const string SourcePath = Folder + "/Suitcase.fbx";
        const string MeshPath = Folder + "/Suitcase_Mesh.asset";
        const string ScenePath = "Assets/_GatePassVR/Scenes/Airport_Lee.unity";
        const float ModelScale = 0.4f;   // 원본(손잡이 편 높이 약 2.06m)을 약 0.82m로
        const float ShellRatio = 0.15f;  // 몸통 껍데기(부품당 약 3.4만 → 약 5천)
        const float PartRatio = 0.35f;   // 손잡이·모서리·바퀴
        const string VisualName = "SuitcaseVisual";

        // 원본 머티리얼 이름 → 새 머티리얼 (이름, 색, 금속성, 매끈함)
        static readonly (string src, string name, Color color, float metallic, float smooth)[] MaterialMap =
        {
            ("Mat.1", "M_Suitcase_Shell", new Color(0.33f, 0.83f, 0.80f), 0f, 0.55f), // Sketchfab 미리보기의 민트색
            ("Mat.3", "M_Suitcase_Metal", new Color(0.70f, 0.71f, 0.73f), 0.6f, 0.6f),
            ("Mat.2", "M_Suitcase_Black", new Color(0.05f, 0.05f, 0.06f), 0f, 0.3f),
            ("Mat",   "M_Suitcase_Wheel", new Color(0.16f, 0.16f, 0.17f), 0f, 0.3f),
            ("Lit",   "M_Suitcase_Dark",  new Color(0.22f, 0.22f, 0.24f), 0f, 0.3f),
        };

        static readonly (string obj, bool lying)[] Targets =
        {
            ("Obj_Carrier_CheckIn", false),     // 체크인: 세워 둠
            ("Obj_BaggageBag_Baggage", true),   // 수하물 찾는 곳 벨트: 눕혀 둠
        };

        [MenuItem("GatePassVR/Build Suitcase")]
        public static void BuildAndApplyMenu()
        {
            var (mesh, mats) = BuildAssets();
            ApplyToScene(mesh, mats);
        }

        public static void BuildAndApply()
        {
            BuildAndApplyMenu();
            if (Application.isBatchMode)
                EditorApplication.Exit(0);
        }

        static (Mesh, Material[]) BuildAssets()
        {
            var src = AssetDatabase.LoadAssetAtPath<GameObject>(SourcePath);
            var rootInv = src.transform.worldToLocalMatrix;

            // 1) 부품마다 따로 줄인 뒤 머티리얼별로 모은다.
            //    (색별로 먼저 합쳐 한꺼번에 줄이면 떨어진 부품 사이가 메워져 손잡이가 판처럼, 바퀴가 가시처럼 깨졌다)
            var groups = MaterialMap.Select(_ => new List<CombineInstance>()).ToArray();
            var temps = new List<Mesh>();
            int beforeTris = 0, afterTris = 0;
            foreach (var mf in src.GetComponentsInChildren<MeshFilter>(true))
            {
                var r = mf.GetComponent<MeshRenderer>();
                var m = mf.sharedMesh;
                for (int s = 0; s < m.subMeshCount; s++)
                {
                    string matName = r.sharedMaterials[Mathf.Min(s, r.sharedMaterials.Length - 1)].name;
                    int g = System.Array.FindIndex(MaterialMap, x => x.src == matName);
                    if (g < 0) g = MaterialMap.Length - 1;

                    var part = new Mesh { indexFormat = IndexFormat.UInt32 };
                    part.CombineMeshes(new[] { new CombineInstance
                    {
                        mesh = m,
                        subMeshIndex = s,
                        transform = Matrix4x4.Scale(Vector3.one * ModelScale) * rootInv * mf.transform.localToWorldMatrix
                    } }, true, true);
                    int tris = TriCount(part);
                    beforeTris += tris;
                    var reducedPart = ReducePart(part, PartTarget(tris, g == 0));
                    afterTris += TriCount(reducedPart);
                    if (reducedPart != part) temps.Add(part);
                    temps.Add(reducedPart);
                    groups[g].Add(new CombineInstance { mesh = reducedPart, transform = Matrix4x4.identity });
                }
            }
            Debug.Log($"[Suitcase] per-part reduce: {beforeTris} → {afterTris} tris");

            // 2) 그룹마다 하나로 합친 뒤, 그룹들을 서브메시로 합친다
            var parts = new List<CombineInstance>();
            var usedGroups = new List<int>();
            for (int g = 0; g < groups.Length; g++)
            {
                if (groups[g].Count == 0) continue;
                var gm = new Mesh { indexFormat = IndexFormat.UInt32 };
                gm.CombineMeshes(groups[g].ToArray(), true, true);
                parts.Add(new CombineInstance { mesh = gm, transform = Matrix4x4.identity });
                usedGroups.Add(g);
            }
            var combined = new Mesh { indexFormat = IndexFormat.UInt32 };
            combined.CombineMeshes(parts.ToArray(), false, false);
            // 합친 메시에 빈 LOD 정보(범위 0)가 따라와 아무것도 그려지지 않았다. 정점·인덱스만 옮긴 새 메시로 만든다.
            var reduced = CleanCopy(combined);
            reduced.name = "Suitcase_Mesh";
            temps.Add(combined);
            CenterOnBounds(reduced);
            Debug.Log($"[Suitcase] result {TriCount(reduced)} tris, {reduced.vertexCount} verts, size {reduced.bounds.size}");

            var old = AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath);
            if (old != null) { EditorUtility.CopySerialized(reduced, old); Object.DestroyImmediate(reduced); reduced = old; }
            else AssetDatabase.CreateAsset(reduced, MeshPath);

            // 5) 머티리얼
            var mats = new Material[usedGroups.Count];
            for (int i = 0; i < usedGroups.Count; i++)
            {
                var def = MaterialMap[usedGroups[i]];
                string path = $"{Folder}/{def.name}.mat";
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null)
                {
                    mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    AssetDatabase.CreateAsset(mat, path);
                }
                mat.SetColor("_BaseColor", def.color);
                mat.SetFloat("_Metallic", def.metallic);
                mat.SetFloat("_Smoothness", def.smooth);
                EditorUtility.SetDirty(mat);
                mats[i] = mat;
            }
            AssetDatabase.SaveAssets();
            foreach (var p in parts) Object.DestroyImmediate(p.mesh);
            foreach (var t in temps) Object.DestroyImmediate(t);
            return (reduced, mats);
        }

        // 부품별 목표 삼각형 수: 몸통 껍데기는 많이, 작은 부품은 조금만 줄인다. 아주 작은 부품은 그대로 둔다.
        static int PartTarget(int tris, bool shell)
        {
            if (tris <= 120) return tris;
            float ratio = shell ? ShellRatio : PartRatio;
            return Mathf.Max(80, Mathf.RoundToInt(tris * ratio));
        }

        // 부품 하나에 Mesh LOD를 만들고 목표 이상인 단계 중 가장 가벼운 것을 고른다.
        static Mesh ReducePart(Mesh part, int target)
        {
            int tris = TriCount(part);
            if (target >= tris) return part;
            var work = Object.Instantiate(part); // 원본 부품은 LOD 데이터 없이 그대로 둔다
            MeshLodUtility.GenerateMeshLods(work, 8);
            int best = 0;
            for (int lod = 0; lod < work.lodCount; lod++)
            {
                int t = (int)work.GetLod(0, lod).indexCount / 3;
                if (t >= target) best = lod; else break;
            }
            var result = best == 0 ? part : ExtractLod(work, best);
            Object.DestroyImmediate(work);
            return result;
        }

        static void ApplyToScene(Mesh mesh, Material[] mats)
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            foreach (var (objName, lying) in Targets)
            {
                var go = GameObject.Find(objName);
                if (go == null) { Debug.LogError("[Suitcase] not found " + objName); continue; }
                var t = go.transform;

                // 옛 상자 크기(로컬 스케일)를 기억해 두고, 스케일은 1로 돌린다(자식이 찌그러지지 않게)
                Vector3 oldSize = t.localScale;
                var visual = t.Find(VisualName);
                if (visual != null && oldSize == Vector3.one)
                {
                    // 이미 적용됨: 메시·머티리얼과 충돌 상자 크기만 갱신(바닥 높이 유지)
                    visual.GetComponent<MeshFilter>().sharedMesh = mesh;
                    visual.GetComponent<MeshRenderer>().sharedMaterials = mats;
                    var bb = mesh.bounds;
                    var newSize = lying ? new Vector3(bb.size.x, bb.size.z, bb.size.y) : bb.size;
                    var col = go.GetComponent<BoxCollider>();
                    t.position += t.up * ((newSize.y - col.size.y) * 0.5f);
                    col.size = newSize;
                    continue;
                }
                t.localScale = Vector3.one;

                var oldRenderer = go.GetComponent<MeshRenderer>();
                if (oldRenderer != null) oldRenderer.enabled = false;

                if (visual == null)
                {
                    visual = new GameObject(VisualName).transform;
                    visual.SetParent(t, false);
                    visual.gameObject.AddComponent<MeshFilter>();
                    visual.gameObject.AddComponent<MeshRenderer>();
                }
                visual.localPosition = Vector3.zero;
                visual.localRotation = lying ? Quaternion.Euler(90f, 0f, 0f) : Quaternion.identity;
                visual.localScale = Vector3.one;
                visual.GetComponent<MeshFilter>().sharedMesh = mesh;
                visual.GetComponent<MeshRenderer>().sharedMaterials = mats;

                // 충돌 상자를 모델 크기에 맞춘다
                var b = mesh.bounds;
                var size = lying ? new Vector3(b.size.x, b.size.z, b.size.y) : b.size;
                var box = go.GetComponent<BoxCollider>();
                box.center = Vector3.zero;
                box.size = size;

                // 바닥 높이를 지키도록 위로 올린다(오브젝트 위 방향 기준)
                float oldHalf = oldSize.y * 0.5f;
                float newHalf = size.y * 0.5f;
                t.position += t.up * (newHalf - oldHalf + 0.01f);
                Debug.Log($"[Suitcase] {objName}: old {oldSize} → collider {size}, lying {lying}");
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        static Mesh CleanCopy(Mesh src)
        {
            var m = new Mesh { indexFormat = src.vertexCount > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
            m.vertices = src.vertices;
            m.normals = src.normals;
            if (src.uv.Length > 0) m.uv = src.uv;
            m.subMeshCount = src.subMeshCount;
            for (int s = 0; s < src.subMeshCount; s++) m.SetTriangles(src.GetTriangles(s), s);
            m.RecalculateBounds();
            return m;
        }

        static void CenterOnBounds(Mesh m)
        {
            var c = m.bounds.center;
            var v = m.vertices;
            for (int i = 0; i < v.Length; i++) v[i] -= c;
            m.vertices = v;
            m.RecalculateBounds();
        }

        static int TriCount(Mesh m)
        {
            int n = 0;
            for (int s = 0; s < m.subMeshCount; s++) n += (int)m.GetIndexCount(s) / 3;
            return n;
        }

        static Mesh ExtractLod(Mesh src, int lod)
        {
            using var data = Mesh.AcquireReadOnlyMeshData(src);
            // 작은 부품은 CombineMeshes가 16비트 인덱스로 만든다. 형식에 맞춰 읽어야 한다.
            int[] all;
            if (data[0].indexFormat == IndexFormat.UInt16)
                all = data[0].GetIndexData<ushort>().Select(x => (int)x).ToArray();
            else
                all = data[0].GetIndexData<uint>().Select(x => (int)x).ToArray();
            var verts = src.vertices;
            var normals = src.normals;
            var uvs = src.uv;
            var remap = new Dictionary<int, int>();
            var nv = new List<Vector3>();
            var nn = new List<Vector3>();
            var nu = new List<Vector2>();
            var subs = new List<List<int>>();
            for (int s = 0; s < src.subMeshCount; s++)
            {
                var sd = src.GetSubMesh(s);
                var range = src.GetLod(s, lod);
                // MeshLodRange.indexStart가 서브메시 기준인지 전체 기준인지에 상관없이 맞도록 처리
                long start = range.indexStart;
                if (start < sd.indexStart || start + range.indexCount > all.Length) start += sd.indexStart;
                var list = new List<int>((int)range.indexCount);
                for (long i = start; i < start + range.indexCount; i++)
                {
                    int vi = all[(int)i] + sd.baseVertex;
                    if (!remap.TryGetValue(vi, out int ni))
                    {
                        ni = nv.Count;
                        remap[vi] = ni;
                        nv.Add(verts[vi]);
                        nn.Add(normals.Length > 0 ? normals[vi] : Vector3.up);
                        nu.Add(uvs.Length > 0 ? uvs[vi] : Vector2.zero);
                    }
                    list.Add(ni);
                }
                subs.Add(list);
            }
            var m = new Mesh { indexFormat = nv.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
            m.SetVertices(nv);
            m.SetNormals(nn);
            m.SetUVs(0, nu);
            m.subMeshCount = subs.Count;
            for (int s = 0; s < subs.Count; s++) m.SetTriangles(subs[s], s);
            m.RecalculateBounds();
            return m;
        }
    }
}
