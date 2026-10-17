import struct
from capstone import Cs, CS_ARCH_X86, CS_MODE_32

path = r"E:\Baerim\client\baerim.exe"
data = open(path, "rb").read()
e_lfanew = struct.unpack_from("<I", data, 0x3C)[0]
opt = e_lfanew + 24
image_base = struct.unpack_from("<I", data, opt + 28)[0]
num_sec = struct.unpack_from("<H", data, e_lfanew + 6)[0]
size_opt = struct.unpack_from("<H", data, e_lfanew + 20)[0]
sec_off = e_lfanew + 24 + size_opt
sects = []
for i in range(num_sec):
    o = sec_off + i * 40
    vsz, va, rsz, raw = struct.unpack_from("<IIII", data, o + 8)
    sects.append((va, vsz, raw, rsz))


def rva_to_off(rva):
    for va, vsz, raw, rsz in sects:
        if va <= rva < va + max(vsz, rsz):
            return raw + (rva - va)
    return None


md = Cs(CS_ARCH_X86, CS_MODE_32)


def read_str(rva, n=80):
    off = rva_to_off(rva)
    if off is None:
        return None
    s = data[off : off + n]
    z = s.find(b"\x00")
    if z >= 0:
        s = s[:z]
    return s


print("string at 0x383163c", read_str(0x383163c))

# Full RenderGame calls
print("\n======== RenderGame 0x20bc70 ALL call/jmp ========")
off = rva_to_off(0x20BC70)
buf = data[off : off + 0x1400]
for insn in md.disasm(buf, 0x20BC70):
    if insn.mnemonic in ("call", "jmp") and insn.op_str.startswith("0x"):
        print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
    if insn.address > 0x20BC70 + 0x1200:
        break
    if insn.mnemonic in ("ret",) and insn.address > 0x20BD00:
        print("  %s: ret" % hex(insn.address))
        # don't break, might be nested - actually ret ends function if not in middle
        # RenderGame likely has one ret at end

print("\n======== 0x1306b0 (called from C++ RenderGame) ========")
off = rva_to_off(0x1306B0)
buf = data[off : off + 200]
for insn in md.disasm(buf, 0x1306B0):
    print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
    if insn.mnemonic == "ret":
        break

# Nearby background methods 0x130500-0x130800
print("\n======== background methods 0x130500-0x1307c0 ========")
for rva in range(0x130500, 0x1307C0, 16):
    off = rva_to_off(rva)
    b = data[off]
    if b in (0x55, 0x56, 0x57, 0x53):
        buf = data[off : off + 8]
        ins = list(md.disasm(buf, rva))
        if ins:
            print(hex(rva), ins[0].mnemonic, ins[0].op_str, "bytes", data[off:off+6].hex())

print("\n======== each bg wrapper ========")
for rva in [0x130540, 0x1305A0, 0x1305C0, 0x1305E0, 0x130600, 0x130620, 0x130640, 0x130690, 0x1306B0, 0x1306D0, 0x130720, 0x130770]:
    off = rva_to_off(rva)
    buf = data[off : off + 160]
    print("\n---", hex(rva), "---")
    for insn in md.disasm(buf, rva):
        print("  %s: %s %s" % (hex(insn.address), insn.mnemonic, insn.op_str))
        if insn.mnemonic == "ret":
            break
        if insn.address > rva + 140:
            break

# Real capstone xrefs to terrain functions from .text
print("\n======== capstone xrefs ========")
text_va = 0x1000
text = data[rva_to_off(text_va) : rva_to_off(text_va) + 0x3421A7C]
targets = {
    0x130540: "BgRender",
    0x130640: "Bg combined map+sky",
    0x1306B0: "Bg 1306b0",
    0x3CFE80: "CMapBase::Render",
    0x3D1E90: "OnRender",
    0x3D22E0: "RenderTerrain",
    0x400180: "HTP",
}
found = {t: [] for t in targets}
n = 0
for insn in md.disasm(text, text_va):
    n += 1
    if insn.mnemonic in ("call", "jmp") and insn.op_str.startswith("0x"):
        tgt = int(insn.op_str, 16)
        if tgt in found and insn.address not in range(tgt, tgt + 8):
            found[tgt].append((insn.address, insn.mnemonic))
print("insns", n)
for t, name in targets.items():
    print(name, hex(t), [(hex(a), m) for a, m in found[t][:15]], "count", len(found[t]))
