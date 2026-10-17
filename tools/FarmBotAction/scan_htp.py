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


def dump(rva, nbytes, label):
    buf = data[rva_to_off(rva) : rva_to_off(rva) + nbytes]
    print("\n====", label, hex(rva), "====")
    n = 0
    for insn in md.disasm(buf, rva):
        if insn.mnemonic in ("call", "jmp", "ret"):
            print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
        n += 1
        if insn.mnemonic == "ret" and n > 15:
            print("  ... ret at", hex(insn.address), "insns", n)
            break
        if n > 400:
            print("  truncated", hex(insn.address))
            break


dump(0x400180, 2500, "HTP 400180")
dump(0x4003A0, 800, "HTP2 4003A0")
dump(0x3FFB30, 120, "3ffb30")
dump(0x3D2730, 80, "3d2730 GetNoFog?")
