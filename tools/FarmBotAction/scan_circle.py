import struct
from capstone import Cs, CS_ARCH_X86, CS_MODE_32

path = r"E:\Baerim\client\baerim.exe"
data = open(path, "rb").read()
e = struct.unpack_from("<I", data, 0x3C)[0]
ns = struct.unpack_from("<H", data, e + 6)[0]
so = struct.unpack_from("<H", data, e + 20)[0]
off = e + 24 + so
sects = []
for i in range(ns):
    o = off + i * 40
    vsz, va, rsz, raw = struct.unpack_from("<IIII", data, o + 8)
    sects.append((va, vsz, raw, rsz))


def rva_to_off(rva):
    for va, vsz, raw, rsz in sects:
        if va <= rva < va + max(vsz, rsz):
            return raw + (rva - va)
    return None


md = Cs(CS_ARCH_X86, CS_MODE_32)
md.detail = False


def dump(rva, nbytes, label):
    o = rva_to_off(rva)
    buf = data[o : o + nbytes]
    print("\n====", label, hex(rva), "====")
    n = 0
    for insn in md.disasm(buf, rva):
        print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
        n += 1
        if insn.mnemonic.startswith("ret"):
            break
        if n > 120:
            print("  truncated")
            break


CHAR = 0x13C930
LINE3D = 0x3409E0
LINE2D = 0x3407D0

dump(CHAR, 0x200, "CharManager::Render")
dump(LINE3D, 0x180, "RenderLine3d")
dump(LINE2D, 0x80, "RenderLine2d")

# find relative calls from CharManager::Render
o = rva_to_off(CHAR)
buf = data[o : o + 0x400]
print("\n==== calls from CharManager::Render ====")
for insn in md.disasm(buf, CHAR):
    if insn.mnemonic == "call":
        print("  %s: call %s" % (hex(insn.address), insn.op_str))
    if insn.mnemonic.startswith("ret"):
        break

# find functions that call RenderLine3d
print("\n==== xrefs to RenderLine3d ====")
target = LINE3D
hits = []
text = None
for va, vsz, raw, rsz in sects:
    if va == 0x1000 or vsz > 0x100000:
        text = (va, raw, min(vsz, rsz) if rsz else vsz)
        break
# use largest executable-looking section
best = max(sects, key=lambda s: s[1])
va, vsz, raw, rsz = best
blob = data[raw : raw + min(rsz, vsz)]
print("scan section va", hex(va), "size", hex(len(blob)))
for i in range(0, len(blob) - 5):
    if blob[i] != 0xE8:
        continue
    rel = struct.unpack_from("<i", blob, i + 1)[0]
    dest = va + i + 5 + rel
    if dest == target:
        src = va + i
        hits.append(src)
        if len(hits) >= 40:
            break
print("hits", len(hits))
for src in hits[:40]:
    print("  call at", hex(src), "from maybe", hex(src - 0x40))
