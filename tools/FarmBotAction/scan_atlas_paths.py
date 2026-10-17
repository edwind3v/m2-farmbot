import mmap
import os
import re

path = r"E:\Baerim\client\baerim.exe"
needles = [
    b"minimap.dds",
    b"minimap.tga",
    b"atlas.sub",
    b"atlas.dds",
    b"_atlas.dds",
    b"root/atlasinfo.txt",
    b"LoadAtlas",
    b"GetAtlasInfo",
    b"d:/ymir work/ui/atlas",
    b"%s/atlas.sub",
    b"map_a2",
]


def around(buf, idx, n=80):
    start = max(0, idx - n)
    end = min(len(buf), idx + n)
    chunk = buf[start:end]
    return "".join(chr(c) if 32 <= c < 127 else "|" for c in chunk)


with open(path, "rb") as f:
    data = f.read()

print("size", len(data))
for needle in needles:
    i = 0
    hits = 0
    while True:
        j = data.find(needle, i)
        if j < 0:
            break
        hits += 1
        if hits <= 8:
            print("\n%s @ 0x%X" % (needle.decode(), j))
            print(" ", around(data, j, 90))
        i = j + len(needle)
    print("count %s = %d" % (needle.decode(), hits))

# unique path-like strings containing atlas or minimap
pat = re.compile(rb"[\x20-\x7e]{8,160}(?:minimap|atlas)[\x20-\x7e]{0,80}", re.I)
seen = set()
for m in pat.finditer(data):
    s = m.group().decode("ascii", "ignore")
    if s in seen:
        continue
    seen.add(s)
    if any(k in s.lower() for k in ("minimap", "atlas.sub", "atlas.dds", "atlasinfo", "%s")):
        print("PATH", s[:160])
