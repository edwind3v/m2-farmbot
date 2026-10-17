import struct
from capstone import Cs, CS_ARCH_X86, CS_MODE_32

path = r"E:\Baerim\client\baerim.exe"
data = open(path, "rb").read()
e = struct.unpack_from("<I", data, 0x3C)[0]
image = 0x400000
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


sva = 0x382B797
pat = struct.pack("<I", sva)
print("search ProjectPosition va", hex(sva))
for name, va, vsz, raw, rsz in sects:
    blob = data[raw : raw + min(vsz, rsz)]
    loc = 0
    n = 0
    while n < 8:
        j = blob.find(pat, loc)
        if j < 0:
            break
        print(name, "rva", hex(va + j), "off", hex(j))
        # dump dwords around as python method table
        fo = raw + j
        for k in range(-16, 20, 4):
            if fo + k >= 0:
                v = struct.unpack_from("<I", data, fo + k)[0]
                print("   %+d: %08X" % (k, v))
        loc = j + 1
        n += 1

# GetHeight python table
sva = 0x400000 + 0x3428618
pat = struct.pack("<I", sva)
print("\nsearch GetHeight va", hex(sva))
for name, va, vsz, raw, rsz in sects:
    blob = data[raw : raw + min(vsz, rsz)]
    loc = 0
    n = 0
    while n < 6:
        j = blob.find(pat, loc)
        if j < 0:
            break
        print(name, "rva", hex(va + j))
        fo = raw + j
        for k in range(-16, 20, 4):
            v = struct.unpack_from("<I", data, fo + k)[0]
            print("   %+d: %08X" % (k, v))
        loc = j + 1
        n += 1

# calls to LookAtRH stub 0x4d6a4d
# E8 rel to 0x4d6a4d
target = 0x4D6A4D
print("\ncalls to LookAtRH stub")
va, vsz, raw, rsz = [s for s in sects if s[1] == 0x1000][0][1:]
# wait sects[0]
name, va, vsz, raw, rsz = sects[0]
blob = data[raw : raw + min(vsz, rsz)]
hits = []
for i in range(len(blob) - 5):
    if blob[i] != 0xE8:
        continue
    rel = struct.unpack_from("<i", blob, i + 1)[0]
    dest = va + i + 5 + rel
    if dest == target:
        hits.append(va + i)
        if len(hits) >= 15:
            break
print("count", len(hits))
md = Cs(CS_ARCH_X86, CS_MODE_32)
for h in hits[:8]:
    print("call", hex(h))
    o = rva_to_off(h) - 24
    for insn in md.disasm(data[o : o + 48], h - 24):
        print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
        if insn.address > h:
            break
