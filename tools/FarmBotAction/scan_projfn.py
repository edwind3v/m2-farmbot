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
    vsz, va, rsz, raw = struct.unpack_from("<IIII", data, o + 8)
    sects.append((va, vsz, raw, rsz))


def rva_to_off(rva):
    for va, vsz, raw, rsz in sects:
        if va <= rva < va + max(vsz, rsz):
            return raw + (rva - va)
    return None


def file_to_rva(fo):
    for va, vsz, raw, rsz in sects:
        if raw <= fo < raw + rsz:
            return va + (fo - raw)
    return None


# ProjectPosition string
s = data.find(b"ProjectPosition")
print("str file", hex(s), "rva", hex(file_to_rva(s) or 0), "va", hex(image + (file_to_rva(s) or 0)))
sva = image + file_to_rva(s)
pat = struct.pack("<I", sva)
text = None
for va, vsz, raw, rsz in sects:
    if va == 0x1000:
        blob = data[raw : raw + min(vsz, rsz)]
        text = (va, blob)
        break
va, blob = text
print("xrefs to ProjectPosition string")
loc = 0
while True:
    j = blob.find(pat, loc)
    if j < 0:
        break
    print(" ", hex(va + j))
    loc = j + 1

md = Cs(CS_ARCH_X86, CS_MODE_32)

# GetHeight string
s = data.find(b"GetHeight")
print("GetHeight str rva", hex(file_to_rva(s) or 0))
sva = image + file_to_rva(s)
pat = struct.pack("<I", sva)
loc = 0
n = 0
print("xrefs GetHeight")
while n < 15:
    j = blob.find(pat, loc)
    if j < 0:
        break
    print(" ", hex(va + j))
    loc = j + 1
    n += 1

# dump function around ProjectPosition python table: look for push offset string then nearby native
# search D3DXMatrixLookAtRH iat 0x3423958 -> va 0x7423958? image+iat = 0x400000+0x3423958 = 0x3823958
lookat = 0x400000 + 0x3423958
pat = struct.pack("<I", lookat)
print("\nLookAtRH iat va", hex(lookat))
loc = 0
n = 0
while n < 10:
    j = blob.find(pat, loc)
    if j < 0:
        break
    rva = va + j
    print(" xref", hex(rva))
    o = rva_to_off(max(0x1000, rva - 40))
    buf = data[o : o + 80]
    print("  nearby")
    for insn in md.disasm(buf, rva - 40):
        if insn.address > rva + 8:
            break
        if "0x3823958" in insn.op_str or insn.address >= rva - 16:
            print("   %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
    loc = j + 1
    n += 1
