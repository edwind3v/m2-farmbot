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
            return sraw + (rva - va)
    return None


md = Cs(CS_ARCH_X86, CS_MODE_32)

print("==== 0x45c160 full-ish ====")
o = rva_to_off(0x45C160)
n = 0
for insn in md.disasm(data[o : o + 0x200], 0x45C160):
    print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
    n += 1
    if insn.mnemonic.startswith("ret") and n > 8:
        break
    if n > 70:
        break

print("\n==== functions after RenderLine3d ====")
o = rva_to_off(0x340AE0)
n = 0
addr = 0x340AE0
buf = data[o : o + 0x300]
for insn in md.disasm(buf, addr):
    print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
    n += 1
    if n > 90:
        break

# screen singleton 0x3C18BB8 in rdata/data?
print("\n==== xrefs to 0x3C18BB8 ====")
va, vsz, raw, rsz = max(sects, key=lambda s: s[1])
blob = data[raw : raw + min(rsz, vsz)]
pat = struct.pack("<I", 0x3C18BB8)
hits = []
i = 0
while True:
    j = blob.find(pat, i)
    if j < 0:
        break
    hits.append(va + j)
    i = j + 1
    if len(hits) >= 20:
        break
print("hits", hits[:20], "count", len(hits))

# CPythonGraphic instance? search SetInterfaceRenderState string
idx = data.find(b"SetInterfaceRenderState")
print("\nSetInterfaceRenderState string fileoff", idx)
if idx >= 0:
    # find pointer to this string
    # need VA of string
    for sva, svsz, sraw, srsz in sects:
        if sraw <= idx < sraw + srsz:
            sva_str = sva + (idx - sraw)
            print("string va", hex(sva_str))
            pat = struct.pack("<I", sva_str)
            refs = []
            k = 0
            while True:
                j = blob.find(pat, k)
                if j < 0:
                    break
                refs.append(va + j)
                k = j + 1
                if len(refs) >= 8:
                    break
            print("refs", [hex(x) for x in refs])
            break
