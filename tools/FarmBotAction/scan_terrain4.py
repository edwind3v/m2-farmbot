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


def off_to_rva(off):
    for va, vsz, raw, rsz in sects:
        if raw <= off < raw + rsz:
            return va + (off - raw)
    return None


text_va = 0x1000
text = data[rva_to_off(text_va) : rva_to_off(text_va) + 0x3421A7C]


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


def dump_calls(rva, n=700, label=""):
    off = rva_to_off(rva)
    buf = data[off : off + n]
    print("\n==== %s %s head %s ====" % (label, hex(rva), buf[:24].hex()))
    i = 0
    while i < n - 6:
        if buf[i] in (0xE8, 0xE9):
            rel = struct.unpack_from("<i", buf, i + 1)[0]
            tgt = (rva + i + 5 + rel) & 0xFFFFFFFF
            print("  %+4d %s %s" % (i, "call" if buf[i] == 0xE8 else "jmp ", hex(tgt)))
            i += 5
            continue
        if buf[i] == 0xFF and buf[i + 1] in (0x50, 0x51, 0x52, 0x53, 0x55, 0x56, 0x57):
            print("  %+4d call [reg+0x%02X]" % (i, buf[i + 2]))
            i += 3
            continue
        i += 1


# CMapOutdoor vtable functions of interest
for rva, name in [
    (0x3D8020, "vt+0x20 slot8"),
    (0x3D1E90, "vt+0x24 slot9"),
    (0x3D8080, "vt+0x28 slot10 OnRender?"),
    (0x3D8060, "vt+0x2C slot11"),
    (0x3D8050, "vt+0x30 slot12"),
    (0x3D6B20, "vt+0x1C GetHeight"),
    (0x3D22E0, "near IsVisiblePart(0) @3d22eb"),
    (0x3D2380, "near IsVisiblePart(0) @3d23b5"),
    (0x3D23D0, "near IsVisiblePart(4) tree"),
    (0x20E210, "RenderGame callee 20e210"),
]:
    dump_calls(rva, 800 if rva in (0x3D8080, 0x20E210, 0x3D22E0, 0x3D8020) else 250, name)

print("\n=== xrefs to slot8 0x3d8020 ===", [hex(x) for x in xrefs_to(0x3D8020)])
print("=== xrefs to slot10 0x3d8080 ===", [hex(x) for x in xrefs_to(0x3D8080)])
print("=== xrefs to 0x3d22e0 ===", [hex(x) for x in xrefs_to(0x3D22E0)])

# find function that contains 0x3d22eb - look backwards for cc/int3 padding or typical prologue
for loc in [0x3D22EB, 0x3D23B5]:
    off = rva_to_off(loc)
    # scan back 80 bytes
    pre = data[off - 80 : off]
    print("\nbytes before", hex(loc), pre.hex())

# string ptr to CalculateTerrainPatch error
s = b"[CTerrain::CalculateTerrainPatch()]: Failed to create terrain vertex buffer"
j = data.find(s)
print("\nCalculateTerrainPatch string rva", hex(off_to_rva(j)), "va", hex(image_base + off_to_rva(j)))
sva = image_base + off_to_rva(j)
pat = struct.pack("<I", sva)
# find code refs: push imm32
idx = 0
refs = []
while True:
    k = data.find(pat, idx)
    if k < 0:
        break
    refs.append(off_to_rva(k))
    idx = k + 1
print("ptr refs", [hex(r) for r in refs if r])

# also 68 xx xx xx xx push
text_off = rva_to_off(text_va)
ks = []
start = 0
while True:
    k = text.find(b"\x68" + pat, start)
    if k < 0:
        break
    ks.append(text_va + k)
    start = k + 1
print("push string in text", [hex(x) for x in ks])
