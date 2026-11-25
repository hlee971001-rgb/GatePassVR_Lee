"""GLB(glTF 2.0 바이너리)를 OBJ + MTL + 텍스처 파일로 바꾼다 (이씨, 11/25).

Unity는 GLB를 바로 읽지 못해서, 패키지(glTFast)를 늘리는 대신 이 스크립트로 OBJ로 바꿔 넣는다.
외부 라이브러리 없이 표준 라이브러리만 쓴다. 삼각형(mode 4) 메시만 다룬다.

사용: python3 Tools/glb_to_obj.py <입력.glb> <출력 폴더> <이름>
"""
import json
import math
import os
import struct
import sys

COMPONENT = {5120: ("b", 1), 5121: ("B", 1), 5122: ("h", 2), 5123: ("H", 2), 5125: ("I", 4), 5126: ("f", 4)}
COUNT = {"SCALAR": 1, "VEC2": 2, "VEC3": 3, "VEC4": 4, "MAT4": 16}


def read_glb(path):
    data = open(path, "rb").read()
    magic, version, length = struct.unpack_from("<III", data, 0)
    if magic != 0x46546C67:
        raise SystemExit("GLB가 아닙니다")
    offset, gltf, binary = 12, None, b""
    while offset < length:
        chunk_len, chunk_type = struct.unpack_from("<II", data, offset)
        chunk = data[offset + 8: offset + 8 + chunk_len]
        if chunk_type == 0x4E4F534A:
            gltf = json.loads(chunk.decode("utf-8"))
        elif chunk_type == 0x004E4942:
            binary = chunk
        offset += 8 + chunk_len
    return gltf, binary


def accessor(gltf, binary, index):
    acc = gltf["accessors"][index]
    view = gltf["bufferViews"][acc["bufferView"]]
    fmt, size = COMPONENT[acc["componentType"]]
    n = COUNT[acc["type"]]
    stride = view.get("byteStride", size * n)
    base = view.get("byteOffset", 0) + acc.get("byteOffset", 0)
    out = []
    for i in range(acc["count"]):
        vals = struct.unpack_from("<" + fmt * n, binary, base + i * stride)
        out.append(vals if n > 1 else vals[0])
    return out


def mat_mul(a, b):
    return [sum(a[r + 4 * k] * b[k + 4 * c] for k in range(4)) for c in range(4) for r in range(4)]


def node_matrix(node):
    if "matrix" in node:
        return node["matrix"]
    t = node.get("translation", [0, 0, 0])
    x, y, z, w = node.get("rotation", [0, 0, 0, 1])
    s = node.get("scale", [1, 1, 1])
    r = [1 - 2 * (y * y + z * z), 2 * (x * y + z * w), 2 * (x * z - y * w), 0,
         2 * (x * y - z * w), 1 - 2 * (x * x + z * z), 2 * (y * z + x * w), 0,
         2 * (x * z + y * w), 2 * (y * z - x * w), 1 - 2 * (x * x + y * y), 0,
         0, 0, 0, 1]
    for c in range(3):
        for rr in range(3):
            r[rr + 4 * c] *= s[c]
    r[12], r[13], r[14] = t
    return r


def transform(m, p, w):
    return tuple(m[r] * p[0] + m[r + 4] * p[1] + m[r + 8] * p[2] + m[r + 12] * w for r in range(3))


def main():
    src, out_dir, name = sys.argv[1], sys.argv[2], sys.argv[3]
    os.makedirs(out_dir, exist_ok=True)
    gltf, binary = read_glb(src)

    # 텍스처 이미지 꺼내기
    image_files = {}
    for i, img in enumerate(gltf.get("images", [])):
        view = gltf["bufferViews"][img["bufferView"]]
        raw = binary[view.get("byteOffset", 0): view.get("byteOffset", 0) + view["byteLength"]]
        ext = ".png" if img.get("mimeType") == "image/png" else ".jpg"
        image_files[i] = f"{name}_tex{i}{ext}"
        open(os.path.join(out_dir, image_files[i]), "wb").write(raw)

    def tex_file(info):
        if not info:
            return None
        return image_files.get(gltf["textures"][info["index"]]["source"])

    # MTL
    roles = {}
    with open(os.path.join(out_dir, name + ".mtl"), "w") as mtl:
        for i, m in enumerate(gltf.get("materials", [{}])):
            pbr = m.get("pbrMetallicRoughness", {})
            mtl.write(f"newmtl mat{i}\nKd 1 1 1\n")
            files = {
                "baseColor": tex_file(pbr.get("baseColorTexture")),
                "normal": tex_file(m.get("normalTexture")),
                "metallicRoughness": tex_file(pbr.get("metallicRoughnessTexture")),
                "occlusion": tex_file(m.get("occlusionTexture")),
            }
            if files["baseColor"]:
                mtl.write(f"map_Kd {files['baseColor']}\n")
            mtl.write("\n")
            roles[f"mat{i}"] = files

    # 노드를 따라가며 메시를 월드 좌표로 모은다
    lines_v, lines_vt, lines_vn, faces = [], [], [], []
    counter = [0]

    def visit(node_index, parent):
        node = gltf["nodes"][node_index]
        m = mat_mul(parent, node_matrix(node))
        if "mesh" in node:
            for prim in gltf["meshes"][node["mesh"]]["primitives"]:
                if prim.get("mode", 4) != 4:
                    continue
                attrs = prim["attributes"]
                pos = accessor(gltf, binary, attrs["POSITION"])
                nor = accessor(gltf, binary, attrs["NORMAL"]) if "NORMAL" in attrs else None
                uv = accessor(gltf, binary, attrs["TEXCOORD_0"]) if "TEXCOORD_0" in attrs else None
                idx = accessor(gltf, binary, prim["indices"]) if "indices" in prim else list(range(len(pos)))
                base = counter[0]
                for k, p in enumerate(pos):
                    lines_v.append("v %.6f %.6f %.6f" % transform(m, p, 1))
                    if nor:
                        n = transform(m, nor[k], 0)
                        l = math.sqrt(sum(c * c for c in n)) or 1
                        lines_vn.append("vn %.6f %.6f %.6f" % tuple(c / l for c in n))
                    if uv:
                        lines_vt.append("vt %.6f %.6f" % (uv[k][0], 1 - uv[k][1]))
                counter[0] += len(pos)
                mat = f"mat{prim.get('material', 0)}"
                tris = []
                for t in range(0, len(idx), 3):
                    a, b, c = (base + idx[t] + 1, base + idx[t + 1] + 1, base + idx[t + 2] + 1)
                    tris.append("f %d/%d/%d %d/%d/%d %d/%d/%d" % (a, a, a, b, b, b, c, c, c))
                faces.append((mat, tris))
        for child in node.get("children", []):
            visit(child, m)

    identity = [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1]
    scene = gltf["scenes"][gltf.get("scene", 0)]
    for n in scene["nodes"]:
        visit(n, identity)

    with open(os.path.join(out_dir, name + ".obj"), "w") as obj:
        obj.write(f"mtllib {name}.mtl\n")
        obj.write("\n".join(lines_v) + "\n")
        obj.write("\n".join(lines_vt) + "\n")
        obj.write("\n".join(lines_vn) + "\n")
        for mat, tris in faces:
            obj.write(f"usemtl {mat}\n" + "\n".join(tris) + "\n")

    tri_total = sum(len(t) for _, t in faces)
    print(f"vertices {counter[0]}, triangles {tri_total}, images {len(image_files)}")
    print("texture roles:", json.dumps(roles, ensure_ascii=False))


if __name__ == "__main__":
    main()
