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


def dump(rva, n=80, title=""):
    print("\n====", title or hex(rva), "====")
    o = rva_to_off(rva)
    c = 0
    for insn in md.disasm(data[o : o + 0x180], rva):
        print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
        c += 1
        if insn.mnemonic.startswith("ret") and c > 8:
            break
        if c >= n:
            break


dump(0x33E680, 40, "write ms_matView")
dump(0x33F1B0, 50, "push ms_matView / ProjectPosition?")
dump(0x33E5B0, 30, "identity eax")
dump(0x33E380, 25, "zero identity")
