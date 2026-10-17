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
    print("\n====", label, hex(rva), buf[:12].hex(), "====")
    n = 0
    for insn in md.disasm(buf, rva):
        if insn.mnemonic in ("call", "jmp", "ret") or "11df40" in insn.op_str or "0x11df" in insn.op_str:
            print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
        n += 1
        if insn.mnemonic == "ret" and n > 8:
            print("  ret", hex(insn.address), "insns", n)
            break
        if n > 80:
            print("  ...", hex(insn.address))
            break


for rva, n, name in [
    (0x490A00, 200, "490a00"),
    (0x4910B0, 120, "4910c3 area"),
    (0x4913B0, 120, "4913ca area"),
    (0x491A90, 120, "491aab area"),
    (0x491BE0, 80, "491bf0 area"),
    (0x491FC0, 80, "491fd2 area"),
    (0x492580, 200, "492580 from RenderGame 43c7a0"),
    (0x43B450, 80, "43b465"),
    (0x439BA0, 80, "439ba7"),
    (0x433A20, 40, "433a28"),
    (0x437840, 40, "437856"),
]:
    dump(rva, n, name)
