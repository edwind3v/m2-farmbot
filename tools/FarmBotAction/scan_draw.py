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
    print("\n====", label, hex(rva), buf[:16].hex(), "====")
    n = 0
    for insn in md.disasm(buf, rva):
        print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
        n += 1
        if insn.mnemonic == "ret" and n > 5:
            break
        if n > 70:
            print("  ...")
            break


for rva, n, name in [
    (0x439B30, 200, "439b30 splat d3d"),
    (0x438C60, 120, "438c60 none d3d"),
    (0x439B50, 120, "439b50 htp2 d3d"),
    (0x462B30, 80, "462b30"),
    (0x420F90, 60, "420f90"),
    (0x402330, 80, "402330"),
    (0x3D1DF0, 40, "3d1df0 SelectIndexBuffer?"),
]:
    dump(rva, n, name)
