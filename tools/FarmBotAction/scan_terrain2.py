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
    name = data[o : o + 8].split(b"\x00")[0].decode("latin1")
    vsz, va, rsz, raw = struct.unpack_from("<IIII", data, o + 8)
    sects.append((name, va, vsz, raw, rsz))


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


def va_to_rva(va):
    return (va - image_base) & 0xFFFFFFFF


def find_dwords(value):
    pat = struct.pack("<I", value)
    hits = []
    start = 0
    while True:
        j = data.find(pat, start)
        if j < 0:
            break
        hits.append(j)
        start = j + 1
        if len(hits) > 40:
            break
    return hits


type_rva = 0x3C07BC0
type_va = image_base + type_rva
print("CMapOutdoor type va", hex(type_va), "rva", hex(type_rva))
hits = find_dwords(type_va)
print("ptrs to type desc:", [(hex(h), hex(off_to_rva(h)) if off_to_rva(h) else None) for h in hits])

# CompleteObjectLocator: +12 = pTypeDescriptor
# vtable is immediately after COL, and vtable[-1] = COL
for h in hits:
    r = off_to_rva(h)
    if r is None:
        continue
    # if this is COL.pTypeDescriptor, COL starts at h-12
    col_off = h - 12
    col_rva = off_to_rva(col_off)
    sig, offset, cd, ptd, pchd = struct.unpack_from("<IIIII", data, col_off)
    print(" possible COL at", hex(col_rva), "sig", sig, "offset", offset, "ptd", hex(ptd), "chd", hex(pchd))
    col_va = image_base + col_rva
    vhits = find_dwords(col_va)
    print("  refs to this COL:", [(hex(vh), hex(off_to_rva(vh)) if off_to_rva(vh) else None) for vh in vhits])
    for vh in vhits:
        vt_off = vh + 4
        vt_rva = off_to_rva(vt_off)
        print("  vtable candidate", hex(vt_rva))
        for i in range(24):
            fn = struct.unpack_from("<I", data, vt_off + i * 4)[0]
            fr = va_to_rva(fn)
            print("    [%2d] +0x%02X va=%s rva=%s" % (i, i * 4, hex(fn), hex(fr)))


def dump_fn(rva, n=500, label=""):
    off = rva_to_off(rva)
    if off is None:
        print("no", hex(rva))
        return
    buf = data[off : off + n]
    print("\n====", label, hex(rva), "====")
    print("head", buf[:48].hex())
    i = 0
    while i < n - 6:
        b0 = buf[i]
        if b0 in (0xE8, 0xE9):
            rel = struct.unpack_from("<i", buf, i + 1)[0]
            tgt = (rva + i + 5 + rel) & 0xFFFFFFFF
            print("  %+4d %s %s" % (i, "call" if b0 == 0xE8 else "jmp ", hex(tgt)))
            i += 5
            continue
        if b0 == 0xFF and buf[i + 1] in (0x50, 0x51, 0x52, 0x53, 0x56, 0x57):
            print("  %+4d call [e?x+0x%02X]" % (i, buf[i + 2]))
            i += 3
            continue
        if b0 == 0xFF and buf[i + 1] in (0x10, 0x11, 0x12, 0x13, 0x16, 0x17):
            print("  %+4d call [e?x]" % i)
            i += 2
            continue
        if b0 == 0xC2:
            imm = struct.unpack_from("<H", buf, i + 1)[0]
            print("  %+4d ret 0x%X  (end?)" % (i, imm))
            if i > 8:
                break
            i += 3
            continue
        if b0 == 0xC3:
            print("  %+4d ret" % i)
            if i > 8:
                break
            i += 1
            continue
        i += 1


# RenderGame with virtual calls
dump_fn(0x20BC70, 900, "RenderGame")
dump_fn(0x130540, 120, "BgRender")
dump_fn(0x3C46E0, 80, "bg 3c46e0")
dump_fn(0x20E210, 80, "RenderGame+20")
dump_fn(0x33C1B0, 80, "33c1b0")

# CPythonBackground Render inlined map methods
for rva, name in [
    (0x3CFE80, "after OnRender? 3cfe80"),
    (0x3D22C0, "3d22c0"),
    (0x3D2060, "3d2060 maybe sky/cloud"),
    (0x3D3150, "3d3150"),
    (0x3D20C0, "3d20c0"),
    (0x3D1BE0, "3d1be0"),
    (0x3D2120, "3d2120"),
    (0x3D1F90, "3d1f90"),
    (0x3D2040, "3d2040"),
]:
    dump_fn(rva, 220, name)

# GetMapOutdoorRef remaining xrefs after first 30
text_va = 0x1000
text_off = rva_to_off(text_va)
text = data[text_off : text_off + 0x3421A7C]


def xrefs_to(target_rva, limit=80):
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


print("\n=== GetMapOutdoorRef all xrefs ===")
xs = xrefs_to(0x3C44C0, 80)
print([hex(x) for x in xs])

print("\n=== PART_TERRAIN string xrefs (push imm / mov) ===")
# PART_TERRAIN rva 0x3428848 -> va
part_va = image_base + 0x3428848
print("PART_TERRAIN va", hex(part_va))
ph = find_dwords(part_va)
print("direct ptrs", [(hex(off_to_rva(h)) if off_to_rva(h) else hex(h)) for h in ph])

print("\n=== Splat strings around 0x34282d2 ===")
for rva in [0x34282D2, 0x34282DE, 0x34286BB, 0x34286D3, 0x347992E]:
    off = rva_to_off(rva)
    s = data[off : data.find(b"\x00", off)]
    print(hex(rva), s)
