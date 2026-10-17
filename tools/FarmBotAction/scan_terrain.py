import struct, collections, os

path = r"E:\Baerim\client\baerim.exe"
data = open(path, "rb").read()
e_lfanew = struct.unpack_from("<I", data, 0x3C)[0]
opt = e_lfanew + 24
image_base = struct.unpack_from("<I", data, opt + 28)[0]
num_sec = struct.unpack_from("<H", data, e_lfanew + 6)[0]
size_opt = struct.unpack_from("<H", data, e_lfanew + 20)[0]
sec_off = e_lfanew + 24 + size_opt
sects = []
print("image_base", hex(image_base), "size", len(data))
for i in range(num_sec):
    o = sec_off + i * 40
    name = data[o : o + 8].split(b"\x00")[0].decode("latin1")
    vsz, va, rsz, raw = struct.unpack_from("<IIII", data, o + 8)
    sects.append((name, va, vsz, raw, rsz))
    print(" ", name, "va", hex(va), "vsz", hex(vsz), "raw", hex(raw), "rsz", hex(rsz))


def rva_to_off(rva):
    for name, va, vsz, raw, rsz in sects:
        if va <= rva < va + max(vsz, rsz):
            return raw + (rva - va)
    return None


def off_to_rva(off):
    for name, va, vsz, raw, rsz in sects:
        if raw <= off < raw + rsz:
            return va + (off - raw)
    return None


text_va = None
text_off = None
text_len = None
for name, va, vsz, raw, rsz in sects:
    if name == ".text" or (text_va is None and name.startswith(".")):
        if name == ".text" or name == "CODE":
            text_va, text_off, text_len = va, raw, min(vsz, rsz)
            break
if text_va is None:
    # first executable-looking
    name, va, vsz, raw, rsz = sects[0]
    text_va, text_off, text_len = va, raw, min(vsz, rsz)
text = data[text_off : text_off + text_len]
print("text va", hex(text_va), "len", hex(text_len))


def xrefs_to(target_rva, limit=40):
    hits = []
    for i in range(0, len(text) - 5):
        if text[i] == 0xE8:
            rel = struct.unpack_from("<i", text, i + 1)[0]
            tgt = (text_va + i + 5 + rel) & 0xFFFFFFFF
            if tgt == target_rva:
                hits.append(text_va + i)
                if len(hits) >= limit:
                    break
    return hits


def dump_calls(rva, n=400):
    off = rva_to_off(rva)
    if off is None:
        print("  no off for", hex(rva))
        return []
    buf = data[off : off + n]
    print("RVA", hex(rva), "bytes", buf[:32].hex())
    calls = []
    i = 0
    while i < n - 5:
        if buf[i] in (0xE8, 0xE9):
            rel = struct.unpack_from("<i", buf, i + 1)[0]
            tgt = (rva + i + 5 + rel) & 0xFFFFFFFF
            print("  %+4d %s %s" % (i, "call" if buf[i] == 0xE8 else "jmp ", hex(tgt)))
            if buf[i] == 0xE8:
                calls.append(tgt)
            i += 5
            continue
        i += 1
    return calls


needles = [
    b"RenderTerrain",
    b"SoftwareTransformPatch",
    b"TerrainPatch",
    b"PART_TERRAIN",
    b"__RenderTerrain",
    b"perf_map_render",
    b"CMapOutdoor",
    b"MapOutdoor",
    b"CTerrain",
    b"RenderArea",
    b"OnRender",
    b"splat",
    b"Splat",
    b"m_pTerrainPatchProxyList",
    b".?AVCMapOutdoor@@",
    b".?AVCMapBase@@",
    b".?AVCTerrain@@",
    b".?AVCPythonBackground@@",
    b"CPythonBackground::Render",
    b"background.Render",
    b"RenderMarkedArea",
    b"EnableTerrainOnlyForHeight",
    b"HardwareTransformPatch",
    b"SelectIndexBuffer",
    b"DrawWireFrame",
]
print("\n=== strings ===")
for n in needles:
    hits = []
    start = 0
    while True:
        j = data.find(n, start)
        if j < 0:
            break
        r = off_to_rva(j)
        hits.append((j, r))
        start = j + 1
        if len(hits) >= 8:
            break
    print(n, "count~", data.count(n), "first", [(hex(h[0]), hex(h[1]) if h[1] else None) for h in hits[:5]])

print("\n=== RTTI names ===")
for needle in [b".?AVCMapOutdoor", b".?AVCMapBase", b".?AVCTerrain", b".?AVCPythonBackground", b".?AVCMapManager"]:
    j = 0
    while True:
        j = data.find(needle, j)
        if j < 0:
            break
        end = data.find(b"\x00", j)
        name = data[j:end]
        r = off_to_rva(j)
        print(" ", name, "rva", hex(r) if r else None)
        j = end + 1

known = {
    "RenderGame": 0x20BC70,
    "BgRender": 0x130540,
    "GetHeight": 0x3C4450,
    "GetMapOutdoorRef": 0x3C44C0,
    "IsMapReady": 0x3C4730,
    "CharRender": 0x13C930,
    "RenderLine3d": 0x3409E0,
}
print("\n=== known xrefs ===")
for name, rva in known.items():
    xs = xrefs_to(rva, 30)
    print(name, hex(rva), "xrefs", [hex(x) for x in xs])

print("\n=== RenderGame dump ===")
dump_calls(0x20BC70, 600)

print("\n=== BgRender dump ===")
dump_calls(0x130540, 200)

print("\n=== GetMapOutdoorRef dump ===")
dump_calls(0x3C44C0, 64)

print("\n=== IsMapReady dump ===")
dump_calls(0x3C4730, 64)
