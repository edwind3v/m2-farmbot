import struct

path = r"E:\Baerim\client\baerim.exe"
data = open(path, "rb").read()
e_lfanew = struct.unpack_from("<I", data, 0x3C)[0]
opt = e_lfanew + 24
image_base = struct.unpack_from("<I", data, opt + 28)[0]
num_sec = struct.unpack_from("<H", data, e_lfanew + 6)[0]
size_opt = struct.unpack_from("<H", data, e_lfanew + 20)[0]
sec_off = e_lfanew + 24 + size_opt
sects = []
for i in range(num_sec):
    o = sec_off + i * 40
    vsz, va, rsz, raw = struct.unpack_from("<IIII", data, o + 8)
    sects.append((va, vsz, raw, rsz))


def rva_to_off(rva):
    for va, vsz, raw, rsz in sects:
        if va <= rva < va + max(vsz, rsz):
            return raw + (rva - va)
    return None


text_va = 0x1000
text = data[rva_to_off(text_va) : rva_to_off(text_va) + 0x3421A7C]


def rel_xrefs(target, limit=50):
    hits = []
    for i in range(0, len(text) - 5):
        if text[i] in (0xE8, 0xE9):
            rel = struct.unpack_from("<i", text, i + 1)[0]
            tgt = (text_va + i + 5 + rel) & 0xFFFFFFFF
            if tgt == target:
                hits.append((text_va + i, "call" if text[i] == 0xE8 else "jmp"))
                if len(hits) >= limit:
                    break
    return hits


def virt_xrefs(disp, limit=40):
    # FF 5x disp  call [reg+disp]
    hits = []
    for i in range(0, len(text) - 3):
        if text[i] == 0xFF and text[i + 1] in (0x50, 0x51, 0x52, 0x53, 0x55, 0x56, 0x57) and text[i + 2] == disp:
            hits.append(text_va + i)
            if len(hits) >= limit:
                break
    return hits


print("CMapBase::Render 0x3cfe80", rel_xrefs(0x3CFE80))
print("OnRender 0x3d1e90", rel_xrefs(0x3D1E90))
print("RenderTerrain 0x3d22e0", rel_xrefs(0x3D22E0))
print("SetInv/shadow 0x3d1e30", rel_xrefs(0x3D1E30))
print("RenderArea 0x3d1fa0", rel_xrefs(0x3D1FA0))
print("RenderTree 0x3d23e0", rel_xrefs(0x3D23E0))
print("maybe 0x3d2280", rel_xrefs(0x3D2280))
print("HTP? 0x400180", rel_xrefs(0x400180)[:20])
print("3d27d0", rel_xrefs(0x3D27D0)[:15])
print("3d1760", rel_xrefs(0x3D1760)[:15])

print("\ncall [reg+0x20] count", len(virt_xrefs(0x20, 200)), "first", [hex(x) for x in virt_xrefs(0x20, 25)])
print("call [reg+0x24]", [hex(x) for x in virt_xrefs(0x24, 40)])
print("call [reg+0x28]", [hex(x) for x in virt_xrefs(0x28, 25)])

# dump 0x3d2280 and 0x3d1e30 and OnRender exactly until first jmp/ret-ish
for rva, n, name in [
    (0x3D1E90, 80, "OnRender only"),
    (0x3D1E30, 80, "SetInverse"),
    (0x3D2280, 80, "3d2280"),
    (0x3D22E0, 220, "RenderTerrain"),
    (0x400180, 120, "400180 HTP?"),
    (0x3D27D0, 80, "3d27d0 recurse?"),
    (0x3D1760, 80, "3d1760"),
]:
    off = rva_to_off(rva)
    buf = data[off : off + n]
    print("\n====", name, hex(rva), buf[:32].hex())
    i = 0
    while i < n - 6:
        if buf[i] in (0xE8, 0xE9):
            rel = struct.unpack_from("<i", buf, i + 1)[0]
            tgt = (rva + i + 5 + rel) & 0xFFFFFFFF
            print("  %+4d %s %s" % (i, "call" if buf[i] == 0xE8 else "jmp ", hex(tgt)))
            i += 5
            continue
        if buf[i] == 0xC3:
            print("  %+4d ret" % i)
            i += 1
            continue
        i += 1

# PythonBackground Render sky/cloud cluster vs C++ RenderGame 0x20e210
print("\n=== 20e210 more targeted: look for calls into 0x3d???? ===")
rva = 0x20E210
off = rva_to_off(rva)
buf = data[off : off + 2000]
i = 0
interesting = []
while i < 1990:
    if buf[i] == 0xE8:
        rel = struct.unpack_from("<i", buf, i + 1)[0]
        tgt = (rva + i + 5 + rel) & 0xFFFFFFFF
        if 0x3C0000 <= tgt <= 0x410000 or 0x12F000 <= tgt <= 0x132000 or tgt in (0x3CFE80, 0x3D1E90, 0x3D22E0, 0x130540):
            interesting.append((i, tgt))
        i += 5
        continue
    i += 1
print("interesting calls from 20e210", [(d, hex(t)) for d, t in interesting[:40]])

print("\n=== 20bc70 interesting ===")
rva = 0x20BC70
off = rva_to_off(rva)
buf = data[off : off + 1200]
i = 0
interesting = []
while i < 1190:
    if buf[i] == 0xE8:
        rel = struct.unpack_from("<i", buf, i + 1)[0]
        tgt = (rva + i + 5 + rel) & 0xFFFFFFFF
        if 0x12F000 <= tgt <= 0x132000 or 0x3C0000 <= tgt <= 0x410000:
            interesting.append((i, tgt))
        i += 5
        continue
    i += 1
print([(d, hex(t)) for d, t in interesting])

# CPythonApplication members - RenderGame should call m_pyBackground methods
# wrappers at 0x1305xx
print("\n=== xrefs to python bg wrappers 1305xx-1307xx ===")
for tgt in [0x130540, 0x1305A0, 0x1305C0, 0x1305E0, 0x130600, 0x130620, 0x130640, 0x130690, 0x1306D0, 0x130720, 0x130770]:
    xs = rel_xrefs(tgt, 10)
    if xs:
        print(hex(tgt), xs)
