import struct, collections

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


def find_dwords(value, limit=50):
    pat = struct.pack("<I", value)
    hits = []
    start = 0
    while True:
        j = data.find(pat, start)
        if j < 0:
            break
        hits.append(j)
        start = j + 1
        if len(hits) >= limit:
            break
    return hits


# TypeDescriptor name at 0x3c07bc0, struct starts 8 bytes before
td_rva = 0x3C07BC0 - 8
td_va = image_base + td_rva
print("TypeDescriptor rva", hex(td_rva), "va", hex(td_va))
print("td bytes", data[rva_to_off(td_rva) : rva_to_off(td_rva) + 32].hex())

hits = find_dwords(td_va)
print("ptrs to TypeDescriptor:", [(hex(off_to_rva(h)), hex(h)) for h in hits if off_to_rva(h)])

for h in hits:
    r = off_to_rva(h)
    if r is None:
        continue
    col_off = h - 12
    col_rva = off_to_rva(col_off)
    if col_rva is None:
        continue
    sig, offset, cd, ptd, pchd = struct.unpack_from("<IIIII", data, col_off)
    print("COL", hex(col_rva), "sig", sig, "offset", offset, "cd", cd, "ptd", hex(ptd), "chd", hex(pchd))
    col_va = image_base + col_rva
    vhits = find_dwords(col_va)
    print(" COL refs", [hex(off_to_rva(vh)) for vh in vhits if off_to_rva(vh)])
    for vh in vhits:
        vt_off = vh + 4
        vt_rva = off_to_rva(vt_off)
        if vt_rva is None:
            continue
        print(" vtable", hex(vt_rva))
        for i in range(20):
            fn = struct.unpack_from("<I", data, vt_off + i * 4)[0]
            fr = (fn - image_base) & 0xFFFFFFFF
            print("   [%2d] +0x%02X rva=%s" % (i, i * 4, hex(fr)))

text_va = 0x1000
text = data[rva_to_off(text_va) : rva_to_off(text_va) + 0x3421A7C]


def xrefs_to(target_rva, limit=60):
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


print("\n=== IsVisiblePart 0x3d1e70 callers with push imm ===")
isvis = 0x3D1E70
for loc in xrefs_to(isvis, 80):
    off = rva_to_off(loc)
    pre = data[off - 8 : off]
    # find push imm before call
    part = None
    if pre[-2] == 0x6A:
        part = pre[-1]
    print(" xref", hex(loc), "pre", pre.hex(), "part", part)

print("\n=== RenderGame raw E8/FF calls (no false ret) ===")
rva = 0x20BC70
off = rva_to_off(rva)
buf = data[off : off + 1200]
i = 0
while i < 1100:
    if buf[i] in (0xE8, 0xE9):
        rel = struct.unpack_from("<i", buf, i + 1)[0]
        tgt = (rva + i + 5 + rel) & 0xFFFFFFFF
        print("  %+4d %s %s" % (i, "call" if buf[i] == 0xE8 else "jmp ", hex(tgt)))
        i += 5
        continue
    if buf[i] == 0xFF and i + 2 < len(buf) and buf[i + 1] in (0x50, 0x51, 0x52, 0x53, 0x55, 0x56, 0x57):
        print("  %+4d call [reg+0x%02X]" % (i, buf[i + 2]))
        i += 3
        continue
    if buf[i] == 0xC3 and i > 40:
        # only trust C3 if previous bytes look like pop/add esp
        print("  %+4d possible ret C3, prev %s" % (i, buf[max(0, i - 8) : i].hex()))
    i += 1

print("\n=== CPythonBackground methods near 0x130540 ===")
# dump each 0x20-byte aligned fn start around 0x130500-0x130800
for rva in range(0x130500, 0x130800, 16):
    off = rva_to_off(rva)
    b = data[off : off + 16]
    if b[0] in (0x55, 0x56, 0x57, 0x53, 0x8B) or b[:2] == b"\x6a":
        pass

# Strings containing Terrain
print("\n=== Terrain-related strings ===")
idx = 0
shown = 0
while shown < 40:
    j = data.find(b"Terrain", idx)
    if j < 0:
        break
    start = max(0, j - 40)
    end = data.find(b"\x00", j)
    s = data[start:end]
    # printable
    s = bytes(c if 32 <= c < 127 else 46 for c in s)
    print(hex(off_to_rva(j) or 0), s)
    shown += 1
    idx = j + 1
