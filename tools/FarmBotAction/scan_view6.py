import struct

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
va, vsz, raw, rsz = [s for s in sects if s[0] == 0x1000][0]
blob = data[raw : raw + min(vsz, rsz)]
target = 0x33F1C0
print("E8 calls to UpdateViewMatrix 0x33F1C0")
n = 0
for i in range(len(blob) - 5):
    if blob[i] != 0xE8:
        continue
    rel = struct.unpack_from("<i", blob, i + 1)[0]
    dest = va + i + 5 + rel
    if dest == target:
        print(" ", hex(va + i))
        n += 1
        if n >= 15:
            break
print("count", n)
target = 0x33C1B0
print("E8 calls to GetCurrentCamera 0x33C1B0")
n = 0
for i in range(len(blob) - 5):
    if blob[i] != 0xE8:
        continue
    rel = struct.unpack_from("<i", blob, i + 1)[0]
    dest = va + i + 5 + rel
    if dest == target:
        print(" ", hex(va + i))
        n += 1
        if n >= 12:
            break
print("count", n)
