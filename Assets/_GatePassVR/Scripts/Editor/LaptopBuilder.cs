// 닫힌 노트북 모양(모서리 둥근 판, 덮개·본체 사이 틈, 포트·경첩·고무발·로고 무늬)을 직접 만드는 에디터 도구 (이씨).
// 외부 에셋 없이 코드로 만든 메시와 색 머티리얼만 쓴다. 실제 브랜드 로고는 쓰지 않는다(가운데 둥근 무늬만).
// 오브젝트 크기(0.22, 0.02, 0.30)는 그대로 두고, 메시는 단위 상자 공간에서 만든다(둥근 모서리가 늘어나지 않게 미리 보정).
// 그래서 충돌 상자·손에 쥐는 기준점(GrabPoint)·트레이 칸 연결을 바꾸지 않아도 된다.
// 사용: 메뉴 GatePassVR/Build Laptop, 또는 배치 모드 -executeMethod GatePassVR.Editor.LaptopBuilder.BuildAndApply
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GatePassVR.Editor
{
    public static class LaptopBuilder
    {
        const string Folder = "Assets/_GatePassVR/Art/Props/Laptop";
        const string MeshPath = Folder + "/Laptop_Mesh.asset";
        const string ScenePath = "Assets/_GatePassVR/Scenes/Airport_Lee.unity";
        const string TargetName = "Obj_Laptop_Security";

        // 실제 크기(미터). 오브젝트 스케일과 같아야 한다.
        static readonly Vector3 Size = new Vector3(0.22f, 0.02f, 0.30f);
        const float Radius = 0.014f;   // 모서리 둥글기
        const float Bevel = 0.0012f;   // 윗면·아랫면 가장자리 깎임
        const float GapHalf = 0.0009f; // 덮개·본체 사이 틈(절반)
        const float GapInset = 0.0016f;
        const int CornerSteps = 8;

        // 서브메시: 0 본체(은색) 1 틈·포트·경첩(짙은 색) 2 바닥 3 고무발 4 로고 무늬
        static readonly (string name, Color color, float metallic, float smooth)[] Mats =
        {
            ("M_Laptop_Body",   new Color(0.78f, 0.79f, 0.81f), 0.75f, 0.6f),
            ("M_Laptop_Dark",   new Color(0.10f, 0.10f, 0.11f), 0.2f, 0.3f),
            ("M_Laptop_Bottom", new Color(0.55f, 0.56f, 0.58f), 0.6f, 0.4f),
            ("M_Laptop_Feet",   new Color(0.06f, 0.06f, 0.06f), 0f, 0.2f),
            ("M_Laptop_Logo",   new Color(0.86f, 0.87f, 0.90f), 0.9f, 0.95f), // 본체보다 조금 밝고 매끈하게(튀지 않게)
        };

        static List<Vector3> verts;
        static List<Vector3> normals;
        static List<int>[] subs;

        [MenuItem("GatePassVR/Build Laptop")]
        public static void BuildAndApplyMenu()
        {
            var (mesh, mats) = BuildAssets();
            ApplyToScene(mesh, mats);
        }

        public static void BuildAndApply()
        {
            BuildAndApplyMenu();
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }

        static (Mesh, Material[]) BuildAssets()
        {
            Directory.CreateDirectory(Folder);
            var mesh = BuildMesh();
            var old = AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath);
            if (old != null) { EditorUtility.CopySerialized(mesh, old); Object.DestroyImmediate(mesh); mesh = old; }
            else AssetDatabase.CreateAsset(mesh, MeshPath);

            var mats = new Material[Mats.Length];
            for (int i = 0; i < Mats.Length; i++)
            {
                string path = $"{Folder}/{Mats[i].name}.mat";
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null)
                {
                    mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    AssetDatabase.CreateAsset(mat, path);
                }
                mat.SetColor("_BaseColor", Mats[i].color);
                mat.SetFloat("_Metallic", Mats[i].metallic);
                mat.SetFloat("_Smoothness", Mats[i].smooth);
                EditorUtility.SetDirty(mat);
                mats[i] = mat;
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[Laptop] mesh {mesh.vertexCount} verts, {TriCount(mesh)} tris");
            return (mesh, mats);
        }

        static void ApplyToScene(Mesh mesh, Material[] mats)
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var go = GameObject.Find(TargetName);
            if (go == null) { Debug.LogError("[Laptop] not found"); return; }
            if (Vector3.Distance(go.transform.localScale, Size) > 0.0001f)
                Debug.LogWarning($"[Laptop] scale {go.transform.localScale} differs from {Size}");
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            go.GetComponent<MeshRenderer>().sharedMaterials = mats;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Laptop] applied to " + TargetName);
        }

        static Mesh BuildMesh()
        {
            verts = new List<Vector3>();
            normals = new List<Vector3>();
            subs = new List<int>[Mats.Length];
            for (int i = 0; i < subs.Length; i++) subs[i] = new List<int>();

            float hy = Size.y * 0.5f;
            // 아래부터: 바닥 깎임 → 본체 옆 → 틈 → 덮개 옆 → 윗면 깎임
            var bottomCap = Ring(Bevel, -hy);
            var baseLow = Ring(0f, -hy + Bevel);
            var baseHigh = Ring(0f, -GapHalf);
            var gapLow = Ring(GapInset, -GapHalf);
            var gapHigh = Ring(GapInset, GapHalf);
            var lidLow = Ring(0f, GapHalf);
            var lidHigh = Ring(0f, hy - Bevel);
            var topCap = Ring(Bevel, hy);

            Band(bottomCap, baseLow, 0);
            Band(baseLow, baseHigh, 0);
            Band(baseHigh, gapLow, 1, Facing.Up);   // 본체 윗면 가장자리(틈 바닥)
            Band(gapLow, gapHigh, 1);               // 틈 안쪽 벽
            Band(gapHigh, lidLow, 1, Facing.Down);  // 덮개 아랫면 가장자리(틈 천장)
            Band(lidLow, lidHigh, 0);
            Band(lidHigh, topCap, 0);
            Cap(topCap, Vector3.up, 0);
            Cap(bottomCap, Vector3.down, 2);

            // 로고 무늬(가운데 둥근 판, 윗면 바로 위)
            Disc(new Vector3(0f, hy + 0.0002f, 0.01f), 0.022f, Vector3.up, 4);
            // 경첩(뒤쪽, +z 끝 틈 높이): 짙은 띠
            // Plate는 반지름(절반 크기)을 받는다. 경첩 띠는 폭의 62% → 반지름 31%. (처음에 62%를 반지름으로 넣어 노트북보다 길었다)
            Plate(new Vector3(0f, 0f, Size.z * 0.5f + 0.0003f), new Vector3(Size.x * 0.31f, 0.0042f, 0f), Vector3.forward, 1);
            // 손가락 홈(앞쪽, -z 끝 덮개 높이), 폭 6cm
            Plate(new Vector3(0f, 0.004f, -Size.z * 0.5f - 0.0003f), new Vector3(0.03f, 0.0022f, 0f), Vector3.back, 1);
            // 포트(오른쪽 옆, 본체 높이): USB 두 개와 둥근 잭 하나
            Plate(new Vector3(Size.x * 0.5f + 0.0003f, -0.0045f, -0.035f), new Vector3(0f, 0.0028f, 0.007f), Vector3.right, 1);
            Plate(new Vector3(Size.x * 0.5f + 0.0003f, -0.0045f, -0.015f), new Vector3(0f, 0.0028f, 0.007f), Vector3.right, 1);
            Plate(new Vector3(Size.x * 0.5f + 0.0003f, -0.0045f, 0.01f), new Vector3(0f, 0.0028f, 0.003f), Vector3.right, 1);
            // 고무발 4개(바닥 아래로 1.2mm)
            float fx = Size.x * 0.5f - 0.025f, fz = Size.z * 0.5f - 0.025f;
            foreach (var p in new[] { new Vector2(-fx, -fz), new Vector2(fx, -fz), new Vector2(-fx, fz), new Vector2(fx, fz) })
                Foot(new Vector3(p.x, -hy, p.y), 0.006f, 0.0012f, 3);

            // 실제 크기 → 단위 상자 공간(오브젝트 스케일로 다시 실제 크기가 된다). 법선은 스케일을 곱해 보정.
            var uv = new Vector3[verts.Count];
            var un = new Vector3[verts.Count];
            for (int i = 0; i < verts.Count; i++)
            {
                uv[i] = new Vector3(verts[i].x / Size.x, verts[i].y / Size.y, verts[i].z / Size.z);
                un[i] = Vector3.Scale(normals[i], Size).normalized;
            }
            var mesh = new Mesh { name = "Laptop_Mesh" };
            mesh.SetVertices(uv);
            mesh.SetNormals(un);
            mesh.subMeshCount = subs.Length;
            for (int s = 0; s < subs.Length; s++) mesh.SetTriangles(subs[s], s);
            mesh.RecalculateBounds();
            return mesh;
        }

        // 둥근 모서리 사각형 외곽(위에서 볼 때), inset만큼 안쪽으로. 점마다 바깥 방향도 함께.
        static (Vector3 p, Vector3 n)[] Ring(float inset, float y)
        {
            float hx = Size.x * 0.5f - inset, hz = Size.z * 0.5f - inset;
            float r = Mathf.Max(0.001f, Radius - inset);
            var centers = new[] { new Vector2(hx - r, hz - r), new Vector2(-(hx - r), hz - r), new Vector2(-(hx - r), -(hz - r)), new Vector2(hx - r, -(hz - r)) };
            var list = new List<(Vector3, Vector3)>();
            for (int c = 0; c < 4; c++)
            {
                for (int i = 0; i <= CornerSteps; i++)
                {
                    float a = (c * 90f + 90f * i / CornerSteps) * Mathf.Deg2Rad;
                    var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                    list.Add((new Vector3(centers[c].x, y, centers[c].y) + dir * r, dir));
                }
            }
            return list.ToArray();
        }

        enum Facing { Out, Up, Down }

        // 두 고리 사이 띠. 법선은 띠 면에 수직이고, facing 쪽을 향하게 부호를 정한다.
        // 옆면·깎인 면은 Out(바깥), 틈의 계단 면은 Up/Down으로 직접 정해 뒷면으로 그려지지 않게 한다.
        static void Band((Vector3 p, Vector3 n)[] low, (Vector3 p, Vector3 n)[] high, int sub, Facing facing = Facing.Out)
        {
            int count = low.Length;
            Vector3 Norm(int k)
            {
                var up = high[k].p - low[k].p;
                var along = Vector3.Cross(Vector3.up, low[k].n);
                var n = Vector3.Cross(up, along).normalized;
                var hint = facing == Facing.Up ? Vector3.up : facing == Facing.Down ? Vector3.down : low[k].n;
                return Vector3.Dot(n, hint) < 0f ? -n : n;
            }
            for (int i = 0; i < count; i++)
            {
                int j = (i + 1) % count;
                var ni = Norm(i);
                var nj = Norm(j);
                int a = Add(low[i].p, ni), b = Add(high[i].p, ni), c = Add(high[j].p, nj), d = Add(low[j].p, nj);
                var face = (ni + nj).normalized;
                Tri(a, b, c, face, sub);
                Tri(a, c, d, face, sub);
            }
        }

        static void Cap((Vector3 p, Vector3 n)[] ring, Vector3 normal, int sub)
        {
            var center = Vector3.zero;
            foreach (var r in ring) center += r.p;
            center /= ring.Length;
            int ci = Add(center, normal);
            int first = verts.Count;
            foreach (var r in ring) Add(r.p, normal);
            for (int i = 0; i < ring.Length; i++)
                Tri(ci, first + i, first + (i + 1) % ring.Length, normal, sub);
        }

        static void Disc(Vector3 center, float radius, Vector3 normal, int sub)
        {
            const int steps = 32;
            int ci = Add(center, normal);
            int first = verts.Count;
            for (int i = 0; i < steps; i++)
            {
                float a = 2f * Mathf.PI * i / steps;
                Add(center + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * radius, normal);
            }
            for (int i = 0; i < steps; i++) Tri(ci, first + i, first + (i + 1) % steps, normal, sub);
        }

        // 옆면에 붙는 얇은 사각 판. halfExtents는 법선 방향 성분이 0인 반지름들.
        static void Plate(Vector3 center, Vector3 halfExtents, Vector3 normal, int sub)
        {
            Vector3 u, v;
            if (Mathf.Abs(normal.y) > 0.5f) { u = new Vector3(halfExtents.x, 0, 0); v = new Vector3(0, 0, halfExtents.z); }
            else if (Mathf.Abs(normal.x) > 0.5f) { u = new Vector3(0, 0, halfExtents.z); v = new Vector3(0, halfExtents.y, 0); }
            else { u = new Vector3(halfExtents.x, 0, 0); v = new Vector3(0, halfExtents.y, 0); }
            int a = Add(center - u - v, normal), b = Add(center - u + v, normal), c = Add(center + u + v, normal), d = Add(center + u - v, normal);
            Tri(a, b, c, normal, sub);
            Tri(a, c, d, normal, sub);
        }

        static void Foot(Vector3 top, float radius, float height, int sub)
        {
            const int steps = 12;
            var bottom = top + Vector3.down * height;
            int first = verts.Count;
            for (int i = 0; i < steps; i++)
            {
                float a0 = 2f * Mathf.PI * i / steps, a1 = 2f * Mathf.PI * (i + 1) / steps;
                var d0 = new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0));
                var d1 = new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1));
                int p = Add(top + d0 * radius, d0), q = Add(bottom + d0 * radius, d0), r = Add(bottom + d1 * radius, d1), s = Add(top + d1 * radius, d1);
                Tri(p, q, r, (d0 + d1).normalized, sub);
                Tri(p, r, s, (d0 + d1).normalized, sub);
            }
            Disc(bottom, radius, Vector3.down, sub);
        }

        static int Add(Vector3 p, Vector3 n)
        {
            verts.Add(p);
            normals.Add(n);
            return verts.Count - 1;
        }

        // Unity 앞면 기준(Cross(b-a, c-a)가 법선 방향)에 맞게 감기 방향을 정한다.
        static void Tri(int a, int b, int c, Vector3 outward, int sub)
        {
            var cross = Vector3.Cross(verts[b] - verts[a], verts[c] - verts[a]);
            if (Vector3.Dot(cross, outward) < 0f) { int t = b; b = c; c = t; }
            subs[sub].Add(a); subs[sub].Add(b); subs[sub].Add(c);
        }

        static int TriCount(Mesh m)
        {
            int n = 0;
            for (int s = 0; s < m.subMeshCount; s++) n += (int)m.GetIndexCount(s) / 3;
            return n;
        }
    }
}
