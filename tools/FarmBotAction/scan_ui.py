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
va, vsz, raw, rsz = [s for s in sects if s[0] == 0x1000][0]
blob = data[raw : raw + min(vsz, rsz)]


def rva_to_off(rva):
    for sva, svsz, sraw, srsz in sects:
        if sva <= rva < sva + max(svsz, srsz):
            return sraw + (rva - sva)
    return None


md = Cs(CS_ARCH_X86, CS_MODE_32)
target = 0x20BC70
print("calls to RenderGame 0x20BC70")
n = 0
for i in range(len(blob) - 5):
    if blob[i] != 0xE8:
        continue
    rel = struct.unpack_from("<i", blob, i + 1)[0]
    dest = va + i + 5 + rel
    if dest == target:
        print(" ", hex(va + i))
        n += 1
        if n >= 12:
            break
print("count", n)

# dump likely Process around 0x20C780
o = rva_to_off(0x20C780)
print("\n==== 0x20C780 ====")
c = 0
for insn in md.disasm(data[o : o + 0x200], 0x20C780):
    print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
    c += 1
    if insn.mnemonic.startswith("ret") and c > 8:
        break
    if c > 70:
        break
