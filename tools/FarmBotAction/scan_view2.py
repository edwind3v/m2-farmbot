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
rva = 0x33EC80
o = rva_to_off(rva)
print("==== UpdateProj around PerspectiveFovRH ====")
n = 0
for insn in md.disasm(data[o : o + 0x200], rva):
    print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
    n += 1
    if insn.mnemonic.startswith("ret") and n > 20:
        break
    if n > 90:
        break
