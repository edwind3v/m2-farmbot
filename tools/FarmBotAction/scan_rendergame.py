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
print("==== RenderGame 0x20BC70 ====")
o = rva_to_off(0x20BC70)
n = 0
for insn in md.disasm(data[o : o + 0x500], 0x20BC70):
    extra = ""
    if insn.mnemonic == "lea" and "edi" in insn.op_str:
        extra = "  # member"
    if insn.mnemonic == "call":
        extra = "  # CALL"
    print("  %s: %s %s%s" % (hex(insn.address), insn.mnemonic, insn.op_str, extra))
    n += 1
    if insn.mnemonic.startswith("ret") and n > 20:
        break
    if n > 160:
        break

# FF 15 / FF 10 calls to 0x3409E0
print("\n==== abs/indirect refs to 0x3409E0 ====")
va, vsz, raw, rsz = max(sects, key=lambda s: s[1])
blob = data[raw : raw + min(rsz, vsz)]
pat = struct.pack("<I", 0x3409E0)
hits = []
i = 0
while True:
    j = blob.find(pat, i)
    if j < 0:
        break
    hits.append(va + j)
    i = j + 1
    if len(hits) >= 30:
        break
print("imm hits", [hex(x) for x in hits], "count", len(hits))

# also image-base VA
pat2 = struct.pack("<I", 0x400000 + 0x3409E0)
hits2 = []
i = 0
while True:
    j = blob.find(pat2, i)
    if j < 0:
        break
    hits2.append(va + j)
    i = j + 1
    if len(hits2) >= 20:
        break
print("va hits", [hex(x) for x in hits2], "count", len(hits2))
