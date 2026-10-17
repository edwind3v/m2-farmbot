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
text = sects[0]
va, vsz, raw, rsz = text
blob = data[raw : raw + min(rsz, vsz)]


def rva_to_off(rva):
    for sva, svsz, sraw, srsz in sects:
        if sva <= rva < sva + max(svsz, srsz):
            return sraw + (rva - sva)
    return None


md = Cs(CS_ARCH_X86, CS_MODE_32)


def dump(rva, nbytes, label):
    o = rva_to_off(rva)
    print("\n====", label, hex(rva), "====")
    n = 0
    for insn in md.disasm(data[o : o + nbytes], rva):
        print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
        n += 1
        if insn.mnemonic.startswith("ret") and n > 12:
            break
        if n > 80:
            print("  truncated")
            break


dump(0x4384A0, 0x120, "stm 4384a0 batch?")

# xrefs to RenderLine3d 0x3409E0
target = 0x3409E0
print("\n==== E8 xrefs to RenderLine3d ====")
hits = []
for i in range(len(blob) - 5):
    if blob[i] != 0xE8:
        continue
    rel = struct.unpack_from("<i", blob, i + 1)[0]
    dest = va + i + 5 + rel
    if dest == target:
        hits.append(va + i)
print("count", len(hits))
for h in hits[:30]:
    print(" ", hex(h))

# find CInstanceBase::Render: lock inc or inc [imm32] early in function, then calls
print("\n==== candidate InstanceBase::Render (inc global + two calls) ====")
found = 0
for i in range(len(blob) - 20):
    # inc dword ptr [imm32] = FF 05 xx xx xx xx
    if blob[i] == 0xFF and blob[i + 1] == 0x05:
        start = None
        for b in range(1, 16):
            if i >= b and blob[i - b] in (0x56, 0x57, 0x53, 0x55, 0x51, 0x8B):
                # look for function start push ecx / mov esi,ecx
                pass
        # function start: push esi; mov esi, ecx nearby
        for s in range(max(0, i - 24), i):
            if blob[s] == 0x56 and s + 2 < len(blob) and blob[s + 1] == 0x8B and blob[s + 2] == 0xF1:
                start = va + s
                break
            if blob[s] == 0x53 and s + 2 < len(blob) and blob[s + 1] == 0x8B and blob[s + 2] == 0xDC:
                start = va + s
                break
        if start is None:
            continue
        # count E8 in next 80 bytes
        calls = []
        for j in range(i, min(i + 80, len(blob) - 5)):
            if blob[j] == 0xE8:
                rel = struct.unpack_from("<i", blob, j + 1)[0]
                dest = va + j + 5 + rel
                if 0x1000 < dest < 0x3500000:
                    calls.append(dest)
        if len(calls) >= 2:
            print("  start", hex(start), "inc at", hex(va + i), "calls", [hex(c) for c in calls[:4]])
            found += 1
            if found >= 15:
                break
