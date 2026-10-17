import struct

path = r"E:\Baerim\client\baerim.exe"
data = open(path, "rb").read()
e = struct.unpack_from("<I", data, 0x3C)[0]
image = struct.unpack_from("<I", data, e + 52)[0]
print("imagebase", hex(image))

# import names
imp = struct.unpack_from("<I", data, e + 128)[0]
impsz = struct.unpack_from("<I", data, e + 132)[0]


def rva_to_off(rva):
    ns = struct.unpack_from("<H", data, e + 6)[0]
    so = struct.unpack_from("<H", data, e + 20)[0]
    off = e + 24 + so
    for i in range(ns):
        o = off + i * 40
        vsz, va, rsz, raw = struct.unpack_from("<IIII", data, o + 8)
        if va <= rva < va + max(vsz, rsz):
            return raw + (rva - va)
    return None


# parse imports
off = rva_to_off(imp)
print("import dir fileoff", hex(off) if off else None)
hits = []
if off is not None:
    i = 0
    while True:
        orig, _, _, name, first = struct.unpack_from("<IIIII", data, off + i * 20)
        if orig == 0 and name == 0:
            break
        dll_off = rva_to_off(name)
        dll = data[dll_off:].split(b"\0", 1)[0].decode("latin1") if dll_off else ""
        thunk = orig if orig else first
        t_off = rva_to_off(thunk)
        idx = 0
        while t_off is not None:
            val = struct.unpack_from("<I", data, t_off + idx * 4)[0]
            if val == 0:
                break
            if val & 0x80000000:
                idx += 1
                continue
            n_off = rva_to_off(val)
            if n_off is not None:
                hint_name = data[n_off + 2 :].split(b"\0", 1)[0]
                if b"Project" in hint_name or b"CreateLine" in hint_name or b"Vec3" in hint_name:
                    iat = image + first + idx * 4
                    print(dll, hint_name.decode("latin1", "replace"), "iat rva", hex(first + idx * 4), "va", hex(iat))
                    hits.append((hint_name, first + idx * 4))
            idx += 1
            if idx > 4000:
                break
        i += 1
        if i > 80:
            break

# xref to IAT slots
text_va, text_raw, text_sz = 0x1000, 0x400, 0
ns = struct.unpack_from("<H", data, e + 6)[0]
so = struct.unpack_from("<H", data, e + 20)[0]
off = e + 24 + so
for i in range(ns):
    o = off + i * 40
    vsz, va, rsz, raw = struct.unpack_from("<IIII", data, o + 8)
    if va == 0x1000:
        text_va, text_raw, text_sz = va, raw, min(vsz, rsz)
        break
blob = data[text_raw : text_raw + text_sz]
for name, iat_rva in hits:
    iat_va = image + iat_rva
    pat = struct.pack("<I", iat_va)
    loc = 0
    n = 0
    print("\nxrefs to", name.decode(), hex(iat_va))
    while True:
        j = blob.find(pat, loc)
        if j < 0:
            break
        rva = text_va + j
        print(" ", hex(rva), "bytes", blob[max(0, j - 8) : j + 8].hex())
        loc = j + 1
        n += 1
        if n >= 20:
            break
