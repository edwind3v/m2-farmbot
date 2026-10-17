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


def dump_calls(rva, nbytes, label):
    buf = data[rva_to_off(rva) : rva_to_off(rva) + nbytes]
    print("\n====", label, hex(rva), "head", buf[:12].hex(), "====")
    n = 0
    for insn in md.disasm(buf, rva):
        if insn.mnemonic in ("call", "jmp") or "dword ptr" in insn.op_str:
            print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
        n += 1
        if insn.mnemonic == "ret" and n > 8:
            print("  %s: ret  insns=%d" % (hex(insn.address), n))
            break
        if n > 250:
            print("  truncated at", hex(insn.address))
            break


for rva, n, name in [
    (0x3FFD90, 900, "RenderPatchSplat 3ffd90"),
    (0x3FFC60, 400, "RenderPatchNone 3ffc60"),
    (0x3FFCE0, 400, "htp2 renderer 3ffce0"),
    (0x43A410, 80, "43a410 from fog"),
]:
    dump_calls(rva, n, name)
