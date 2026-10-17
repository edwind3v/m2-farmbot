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
    name = data[o : o + 8].split(b"\0", 1)[0].decode("latin1")
    sects.append((name, va, vsz, raw, rsz))


def rva_to_off(rva):
    for name, va, vsz, raw, rsz in sects:
        if va <= rva < va + max(vsz, rsz):
            return raw + (rva - va)
    return None


md = Cs(CS_ARCH_X86, CS_MODE_32)
LINE3D = 0x3409E0
o = rva_to_off(LINE3D)
print("==== RenderLine3d 0x3409E0 ====")
n = 0
for insn in md.disasm(data[o : o + 0x180], LINE3D):
    print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
    n += 1
    if insn.mnemonic.startswith("ret"):
        break
    if n > 80:
        break

# nearby functions after line2d 0x3407D0
print("\n==== RenderLine2d 0x3407D0 ====")
o = rva_to_off(0x3407D0)
n = 0
for insn in md.disasm(data[o : o + 0x80], 0x3407D0):
    print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
    n += 1
    if insn.mnemonic.startswith("ret"):
        break
    if n > 40:
        break

# find mov to/from likely ms_diffuseColor around line3d
print("\n==== abs mem refs in RenderLine3d ====")
o = rva_to_off(LINE3D)
for insn in md.disasm(data[o : o + 0x200], LINE3D):
    if insn.mnemonic.startswith("ret"):
        break
    if any(x in insn.op_str for x in ("0x3", "dword ptr [0x", "byte ptr [0x")):
        print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))

# dump Process around 0x213d46 a bit further for SetInterface
print("\n==== after RenderGame call ====")
o = rva_to_off(0x213D40)
n = 0
for insn in md.disasm(data[o : o + 0x120], 0x213D40):
    print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
    n += 1
    if n > 50:
        break
