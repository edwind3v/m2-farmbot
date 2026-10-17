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
    o = rva_to_off(rva)
    print("\n====", label, hex(rva), "====")
    n = 0
    for insn in md.disasm(data[o : o + nbytes], rva):
        print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
        n += 1
        if insn.mnemonic.startswith("ret") and n > 10:
            break
        if n > 100:
            print("  truncated")
            break


dump(0x1406C0, 0x160, "list Render 1406c0")

print("\n==== calls/virtuals in 1406c0 ====")
o = rva_to_off(0x1406C0)
for insn in md.disasm(data[o : o + 0x200], 0x1406C0):
    if insn.mnemonic == "call":
        print("  %s: call %s" % (hex(insn.address), insn.op_str))
    if insn.mnemonic.startswith("ret") and insn.address > 0x1406C0 + 0x20:
        break
