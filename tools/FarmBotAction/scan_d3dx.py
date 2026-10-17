import struct

path = r"E:\Baerim\client\baerim.exe"
data = open(path, "rb").read()
e = struct.unpack_from("<I", data, 0x3C)[0]
image = struct.unpack_from("<I", data, e + 52)[0]
imp = struct.unpack_from("<I", data, e + 128)[0]
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


off = rva_to_off(imp)
i = 0
while True:
    orig, _, _, name, first = struct.unpack_from("<IIIII", data, off + i * 20)
    if orig == 0 and name == 0:
        break
    dll_off = rva_to_off(name)
    dll = data[dll_off:].split(b"\0", 1)[0].decode("latin1") if dll_off else ""
    if "d3dx" in dll.lower() or "d3d9" in dll.lower():
        thunk = orig if orig else first
        t_off = rva_to_off(thunk)
        idx = 0
        print("DLL", dll)
        while t_off:
            val = struct.unpack_from("<I", data, t_off + idx * 4)[0]
            if val == 0:
                break
            if not (val & 0x80000000):
                n_off = rva_to_off(val)
                if n_off is not None:
                    hint_name = data[n_off + 2 :].split(b"\0", 1)[0].decode("latin1", "replace")
                    if any(k in hint_name for k in ("Matrix", "Project", "Line", "Look", "Perspective", "Ortho", "Viewport")):
                        print(" ", hint_name, "iat", hex(first + idx * 4))
            idx += 1
            if idx > 5000:
                break
    i += 1
    if i > 80:
        break

# search ascii ProjectPosition
for s in (b"ProjectPosition", b"ms_matView", b"ms_matProj", b"GetHeight"):
    p = data.find(s)
    print("string", s, "fileoff", hex(p) if p >= 0 else None)
