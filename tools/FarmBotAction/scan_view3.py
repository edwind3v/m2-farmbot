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
    name = data[o : o + 8].split(b"\0", 1)[0]
    vsz, va, rsz, raw = struct.unpack_from("<IIII", data, o + 8)
    sects.append((name, va, vsz, raw, rsz))
    print(name, hex(va), hex(va + vsz))


def rva_to_off(rva):
    for name, va, vsz, raw, rsz in sects:
        if va <= rva < va + max(vsz, rsz):
            return raw + (rva - va)
    return None


o = rva_to_off(0x33ED11)
print("bytes", data[o : o + 16].hex())
print("imm", hex(struct.unpack_from("<I", data, o + 1)[0]))

# disassemble with VA = imagebase+rva
md = Cs(CS_ARCH_X86, CS_MODE_32)
o = rva_to_off(0x33ECD0)
for insn in md.disasm(data[o : o + 0xB0], 0x400000 + 0x33ECD0):
    print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
    if insn.address > 0x400000 + 0x33ED70:
        break

# camera manager 0x4017ea8 -> rva 0x17EA8? That's small. 0x4017EA8 - 0x400000 = 0x17EA8
# wait file VA 0x4017ea8 rva 0x17ea8 still .text
# 0x3C17EA8 would be g_base+... 
print("\n4017ea8 as rva", hex(0x4017EA8 - 0x400000))
print("if meant 3C17EA8 rva", hex(0x3C17EA8 - 0x400000))
