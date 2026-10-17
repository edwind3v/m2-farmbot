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


def rva_to_off(rva):
    for name, va, vsz, raw, rsz in sects:
        if va <= rva < va + max(vsz, rsz):
            return raw + (rva - va)
    return None


md = Cs(CS_ARCH_X86, CS_MODE_32)
text = [s for s in sects if s[0] == b".text"][0]
name, va, vsz, raw, rsz = text
blob = data[raw : raw + min(vsz, rsz)]


def xref_imm(imm, label, limit=20):
    pat = struct.pack("<I", imm)
    print("\n==== xrefs to", label, hex(imm), "====")
    n = 0
    loc = 0
    while n < limit:
        j = blob.find(pat, loc)
        if j < 0:
            break
        rva = va + j
        # dump surrounding instruction by scanning back a few bytes
        o = rva_to_off(max(0, rva - 8))
        dumped = False
        for insn in md.disasm(data[o : o + 24], rva - 8):
            if hex(imm) in (insn.op_str or ""):
                print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
                dumped = True
                break
        if not dumped:
            print("  raw rva", hex(rva), data[raw + j - 2 : raw + j + 6].hex())
        loc = j + 1
        n += 1
    print("  count shown", n)


# ms_matProj linked VA
xref_imm(0x402A010, "ms_matProj")
# nearby: view = proj-64, identity = proj-128, inverse = proj+64
for delta, lab in ((-128, "ident?"), (-64, "view?"), (64, "inv?"), (128, "invY?"), (192, "world?"), (256, "wv?")):
    xref_imm(0x402A010 + delta, lab)

xref_imm(0x4017EA8, "camMgr?")
xref_imm(0x3B84FC0, "fov")

# dump 33CD10
print("\n==== 0x33CD10 ====")
o = rva_to_off(0x33CD10)
n = 0
for insn in md.disasm(data[o : o + 0x120], 0x33CD10):
    print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
    n += 1
    if insn.mnemonic.startswith("ret") and n > 4:
        break
    if n > 50:
        break
