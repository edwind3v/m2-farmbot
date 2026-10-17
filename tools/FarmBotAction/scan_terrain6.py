import struct
from capstone import Cs, CS_ARCH_X86, CS_MODE_32

path = r"E:\Baerim\client\baerim.exe"
data = open(path, "rb").read()
e_lfanew = struct.unpack_from("<I", data, 0x3C)[0]
opt = e_lfanew + 24
image_base = struct.unpack_from("<I", data, opt + 28)[0]
num_sec = struct.unpack_from("<H", data, e_lfanew + 6)[0]
size_opt = struct.unpack_from("<H", data, e_lfanew + 20)[0]
sec_off = e_lfanew + 24 + size_opt
sects = []
for i in range(num_sec):
    o = sec_off + i * 40
    vsz, va, rsz, raw = struct.unpack_from("<IIII", data, o + 8)
    sects.append((va, vsz, raw, rsz))


def rva_to_off(rva):
    for va, vsz, raw, rsz in sects:
        if va <= rva < va + max(vsz, rsz):
            return raw + (rva - va)
    return None


md = Cs(CS_ARCH_X86, CS_MODE_32)
md.detail = False


def disasm(rva, size=500, label=""):
    off = rva_to_off(rva)
    buf = data[off : off + size]
    print("\n========", label, hex(rva), "========")
    n = 0
    for insn in md.disasm(buf, rva):
        if insn.mnemonic in ("call", "jmp") or insn.op_str.startswith("dword ptr"):
            print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
        n += 1
        if insn.mnemonic in ("ret", "retn") and n > 8:
            print("  %s: ret" % hex(insn.address))
            break
        if n > 180:
            print("  ... truncated")
            break


def disasm_all_calls(rva, size=800, label=""):
    off = rva_to_off(rva)
    buf = data[off : off + size]
    print("\n========", label, hex(rva), "head", buf[:16].hex(), "========")
    n = 0
    for insn in md.disasm(buf, rva):
        print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
        n += 1
        if insn.mnemonic in ("ret", "retn") and n > 5:
            break
        if n > 80:
            print("  ...")
            break


for rva, sz, name in [
    (0x20BC70, 400, "RenderGame 20bc70"),
    (0x20E210, 600, "20e210"),
    (0x12FFB0, 250, "12ffb0 from 20e210"),
    (0x130640, 120, "130640 called from 20c80a"),
    (0x21CA60, 120, "21ca60 from 20e210"),
    (0x3D22E0, 220, "RenderTerrain"),
    (0x400180, 200, "HTP 400180"),
    (0x3D1E90, 80, "OnRender"),
    (0x20C7F0, 80, "around 20c80a"),
]:
    disasm_all_calls(rva, sz, name)
