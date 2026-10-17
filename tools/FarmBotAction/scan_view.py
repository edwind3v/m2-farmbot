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
    for va, vsz, raw, rsz in sects:
        if va <= rva < va + max(vsz, rsz):
            return raw + (rva - va)
    return None


md = Cs(CS_ARCH_X86, CS_MODE_32)


def dump(rva, n, label):
    o = rva_to_off(rva)
    print("\n====", label, hex(rva), "====")
    c = 0
    for insn in md.disasm(data[o : o + n], rva):
        print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
        c += 1
        if insn.mnemonic.startswith("ret") and c > 8:
            break
        if c > 70:
            break


# function start before LookAtRH call
dump(0x33CFC0, 0x180, "camera lookat area")
dump(0x33D000, 0x160, "33d000")

# PerspectiveFovRH stub 0x4d6a53
# find calls
target = 0x4D6A53
va, vsz, raw, rsz = sects[0]
blob = data[raw : raw + min(vsz, rsz)]
print("\nPerspectiveFovRH calls")
hits = []
for i in range(len(blob) - 5):
    if blob[i] != 0xE8:
        continue
    rel = struct.unpack_from("<i", blob, i + 1)[0]
    dest = va + i + 5 + rel
    if dest == target:
        hits.append(va + i)
        if len(hits) >= 8:
            break
print("count", len(hits), [hex(h) for h in hits])
for h in hits[:4]:
    dump(h - 20, 0x50, "fov near " + hex(h))
