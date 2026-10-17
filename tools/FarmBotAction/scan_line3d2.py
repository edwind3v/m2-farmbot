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

va, vsz, raw, rsz = max(sects, key=lambda s: s[1])
blob = data[raw : raw + min(rsz, vsz)]
md = Cs(CS_ARCH_X86, CS_MODE_32)


def rva_to_off(rva):
    for sva, svsz, sraw, srsz in sects:
        if sva <= rva < sva + max(svsz, srsz):
            return sraw + (rva - sva)
    return None


target = 0x3409E0
hits = []
for i in range(len(blob) - 5):
    if blob[i] != 0xE8:
        continue
    rel = struct.unpack_from("<i", blob, i + 1)[0]
    dest = va + i + 5 + rel
    if dest == target:
        hits.append(va + i)
print("RenderLine3d xrefs", len(hits))
for src in hits[:15]:
    print("\n---- caller", hex(src), "----")
    o = rva_to_off(src - 0x30)
    n = 0
    for insn in md.disasm(data[o : o + 0x50], src - 0x30):
        mark = " <<<" if insn.address == src else ""
        print("  %s: %s %s%s" % (hex(insn.address), insn.mnemonic, insn.op_str, mark))
        n += 1
        if insn.address > src:
            break
        if n > 25:
            break

print("\n==== 0x33f2e0 (line ctor?) ====")
o = rva_to_off(0x33F2E0)
n = 0
for insn in md.disasm(data[o : o + 0x80], 0x33F2E0):
    print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
    n += 1
    if insn.mnemonic.startswith("ret") and n > 3:
        break
    if n > 30:
        break

print("\n==== 0x45c160 ====")
o = rva_to_off(0x45C160)
n = 0
for insn in md.disasm(data[o : o + 0x80], 0x45C160):
    print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
    n += 1
    if insn.mnemonic.startswith("ret") and n > 3:
        break
    if n > 25:
        break

print("\n==== 0x438f40 (submit?) ====")
o = rva_to_off(0x438F40)
n = 0
for insn in md.disasm(data[o : o + 0x80], 0x438F40):
    print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
    n += 1
    if insn.mnemonic.startswith("ret") and n > 3:
        break
    if n > 25:
        break

# app singleton 0x401e7b8 - find CPythonGraphic-ish by looking at RenderGame using this
print("\n==== RenderGame 0x20BC70 start ====")
o = rva_to_off(0x20BC70)
n = 0
for insn in md.disasm(data[o : o + 0x120], 0x20BC70):
    print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
    n += 1
    if n > 40:
        break
