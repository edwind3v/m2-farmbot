import struct
from capstone import Cs, CS_ARCH_X86, CS_MODE_32

path = r"E:\Baerim\client\baerim.exe"
data = open(path, "rb").read()
e = struct.unpack_from("<I", data, 0x3C)[0]
image = struct.unpack_from("<I", data, e + 52)[0]
ns = struct.unpack_from("<H", data, e + 6)[0]
so = struct.unpack_from("<H", data, e + 20)[0]
off = e + 24 + so
sects = []
for i in range(ns):
    o = off + i * 40
    name = data[o : o + 8].split(b"\0", 1)[0].decode("latin1")
    vsz, va, rsz, raw = struct.unpack_from("<IIII", data, o + 8)
    sects.append((name, va, vsz, raw, rsz))
print("imagebase", hex(image))
for s in sects:
    print(s[0], "va", hex(s[1]), "vsz", hex(s[2]), "raw", hex(s[3]))


def rva_to_off(rva):
    for name, va, vsz, raw, rsz in sects:
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
        if insn.mnemonic.startswith("ret") and n > 8:
            break
        if n > 90:
            print("  truncated")
            break


dump(0x13D090, 0x180, "sorted actor render?")
dump(0x45C160, 0x80, "line helper 45c160")
dump(0x438F40, 0x60, "stm 438f40")
dump(0x33F2E0, 0x40, "33f2e0")

# calls from 13d090
print("\n==== calls from 13d090 ====")
o = rva_to_off(0x13D090)
n = 0
for insn in md.disasm(data[o : o + 0x500], 0x13D090):
    if insn.mnemonic == "call":
        print("  %s: call %s" % (hex(insn.address), insn.op_str))
    if insn.mnemonic.startswith("ret"):
        n += 1
        if n >= 1 and insn.address > 0x13D090 + 0x40:
            break
    if insn.address > 0x13D090 + 0x280:
        break
