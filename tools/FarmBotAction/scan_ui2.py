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
    for sva, svsz, sraw, srsz in sects:
        if sva <= rva < sva + max(svsz, srsz):
            return sraw + (rva - sva)
    return None


md = Cs(CS_ARCH_X86, CS_MODE_32)
o = rva_to_off(0x213C80)
print("==== around RenderGame call 0x213d46 ====")
c = 0
for insn in md.disasm(data[o : o + 0x280], 0x213C80):
    mark = " <<<" if insn.address == 0x213D46 else ""
    print("  %s: %s %s%s" % (hex(insn.address), insn.mnemonic, insn.op_str, mark))
    c += 1
    if insn.mnemonic.startswith("ret") and insn.address > 0x213D46:
        break
    if c > 90:
        break
