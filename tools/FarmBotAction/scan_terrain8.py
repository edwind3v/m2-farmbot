import struct
from capstone import Cs, CS_ARCH_X86, CS_MODE_32

path = r"E:\Baerim\client\baerim.exe"
data = open(path, "rb").read()
e_lfanew = struct.unpack_from("<I", data, 0x3C)[0]
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


def dump(rva, n=120, label=""):
    off = rva_to_off(rva)
    buf = data[off : off + n + 32]
    print("\n====", label, hex(rva), buf[:12].hex(), "====")
    for insn in md.disasm(buf, rva):
        print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
        if insn.mnemonic == "ret" and insn.address > rva + 8:
            break
        if insn.address >= rva + n:
            break


for rva, n, name in [
    (0x3D23B0, 80, "3d23b0 PART_TERRAIN from 1306b0"),
    (0x3D1FF0, 80, "3d1ff0 from 1306b0"),
    (0x3D2400, 40, "3d2400 trees?"),
    (0x20C770, 80, "gap around RenderGame ret / 130640 caller"),
    (0x12FEA0, 80, "12fea0 from RenderGame"),
    (0x21D370, 80, "21d370"),
    (0x21DFC0, 80, "21dfc0"),
    (0x43C5C0, 80, "43c5c0"),
    (0x43BBA0, 80, "43bba0"),
    (0x43C7A0, 40, "43c7a0"),
    (0x13B640, 60, "13b640"),
    (0x178A80, 60, "178a80"),
    (0x12FB70, 40, "12fb70"),
    (0x4380B0, 40, "4380b0"),
    (0x130740, 40, "130740"),
]:
    dump(rva, n, name)

# function containing 0x20c78a
print("\nbytes 20c770-20c830", data[rva_to_off(0x20C770) : rva_to_off(0x20C770) + 0xC0].hex())
