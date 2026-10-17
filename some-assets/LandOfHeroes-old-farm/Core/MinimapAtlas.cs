using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

// One game map assembled from its extracted area folders (XXXYYY/minimap.dds and XXXYYY/attr.atr).
// Every area is 256x256 pixels and 25600 game units, so one pixel / attribute cell is 100 units.
public sealed class MinimapAtlas : IDisposable
{
    public const float UnitsPerPixel = 100f;
    public string Name { get; private set; }
    public Bitmap Image { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    // Attribute byte per cell (bit 0 = blocked); 0xFF where no area exists.
    public byte[] Attr { get; private set; }

    // Shared by every copy of the app (bin\Release, app\ …), so maps extracted once are found by all of them.
    public static string SharedFolder { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Onur", "maps"); } }

    public static IEnumerable<string> DefaultRoots()
    {
        yield return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "maps");
        yield return SharedFolder;
        yield return @"D:\pack_cikti\files";
    }

    public static MinimapAtlas Load(string mapName, IEnumerable<string> roots)
    {
        if (string.IsNullOrWhiteSpace(mapName)) return null;
        foreach (var root in roots)
        {
            var dir = Path.Combine(root, mapName.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(dir)) continue;
            var areas = Directory.GetDirectories(dir).Select(Path.GetFileName).Where(n => n.Length == 6 && n.All(char.IsDigit)).ToList();
            if (areas.Count == 0) continue;
            return Build(mapName, dir, areas);
        }
        return null;
    }

    static MinimapAtlas Build(string name, string dir, List<string> areas)
    {
        int cols = areas.Max(a => int.Parse(a.Substring(0, 3))) + 1, rows = areas.Max(a => int.Parse(a.Substring(3))) + 1;
        int width = cols * 256, height = rows * 256;
        var pixels = new int[width * height];
        var attr = Enumerable.Repeat((byte)0xFF, width * height).ToArray();
        for (int i = 0; i < pixels.Length; i++) pixels[i] = unchecked((int)0xFF0A0F1A);
        foreach (var area in areas)
        {
            int ax = int.Parse(area.Substring(0, 3)) * 256, ay = int.Parse(area.Substring(3)) * 256;
            var attrFile = Path.Combine(dir, area, "attr.atr");
            if (File.Exists(attrFile))
            {
                var b = File.ReadAllBytes(attrFile);
                int w = BitConverter.ToUInt16(b, 2), h = BitConverter.ToUInt16(b, 4);
                for (int y = 0; y < Math.Min(h, 256); y++)
                    for (int x = 0; x < Math.Min(w, 256); x++)
                        attr[(ay + y) * width + ax + x] = b[6 + y * w + x];
            }
            int[] tile = null;
            var dds = Path.Combine(dir, area, "minimap.dds");
            if (File.Exists(dds)) { try { tile = DecodeDds(File.ReadAllBytes(dds), 256, 256); } catch { tile = null; } }
            // Some maps ship the minimap as PNG or BMP instead of DDS.
            foreach (var picture in new[] { "minimap.png", "minimap.bmp" })
                if (tile == null && File.Exists(Path.Combine(dir, area, picture))) { try { tile = LoadPicture(Path.Combine(dir, area, picture)); } catch { tile = null; } }
            for (int y = 0; y < 256; y++)
                for (int x = 0; x < 256; x++)
                {
                    int cell = (ay + y) * width + ax + x;
                    int color;
                    if (tile != null) color = tile[y * 256 + x];
                    else { byte a = attr[cell]; color = a == 0xFF ? unchecked((int)0xFF0A0F1A) : (a & 1) != 0 ? unchecked((int)0xFF1E1E1E) : unchecked((int)0xFFE1D7BE); }
                    // Blocked cells are darkened so walls stay readable on top of the terrain picture.
                    if (tile != null && attr[cell] != 0xFF && (attr[cell] & 1) != 0) color = Darken(color);
                    // The game's minimap textures are very dark; walkable ground is lifted so it reads on a small radar.
                    else if (tile != null && attr[cell] != 0xFF) color = Brighten(color);
                    pixels[cell] = color;
                }
        }
        var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        var data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        for (int y = 0; y < height; y++) Marshal.Copy(pixels, y * width, data.Scan0 + y * data.Stride, width);
        bmp.UnlockBits(data);
        return new MinimapAtlas { Name = name, Image = bmp, Width = width, Height = height, Attr = attr };
    }

    static int[] LoadPicture(string file)
    {
        using (var source = new Bitmap(file))
        using (var scaled = new Bitmap(256, 256, PixelFormat.Format32bppArgb))
        {
            using (var g = Graphics.FromImage(scaled)) { g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear; g.DrawImage(source, 0, 0, 256, 256); }
            var data = scaled.LockBits(new Rectangle(0, 0, 256, 256), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var px = new int[256 * 256]; for (int y = 0; y < 256; y++) Marshal.Copy(data.Scan0 + y * data.Stride, px, y * 256, 256);
            scaled.UnlockBits(data); return px;
        }
    }

    static int Darken(int argb)
    {
        int r = (argb >> 16) & 255, g = (argb >> 8) & 255, b = argb & 255;
        return unchecked((int)0xFF000000) | ((r * 2 / 5) << 16) | ((g * 2 / 5) << 8) | (b * 2 / 5);
    }

    static int Brighten(int argb)
    {
        int r = Math.Min(255, ((argb >> 16) & 255) * 2 + 24), g = Math.Min(255, ((argb >> 8) & 255) * 2 + 24), b = Math.Min(255, (argb & 255) * 2 + 24);
        return unchecked((int)0xFF000000) | (r << 16) | (g << 8) | b;
    }

    public bool IsBlocked(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height) return true;
        byte a = Attr[y * Width + x];
        return a == 0xFF || (a & 1) != 0;
    }

    // Top mip of a DDS (BC1 / BC3, legacy FourCC or DX10 header) as ARGB pixels.
    public static int[] DecodeDds(byte[] b, int maxW, int maxH)
    {
        if (b.Length < 128 || b[0] != 'D' || b[1] != 'D' || b[2] != 'S') throw new InvalidDataException("DDS değil");
        int height = BitConverter.ToInt32(b, 12), width = BitConverter.ToInt32(b, 16);
        string fourCC = System.Text.Encoding.ASCII.GetString(b, 84, 4);
        int offset = 128; bool bc3;
        if (fourCC == "DX10") { int dxgi = BitConverter.ToInt32(b, 128); offset = 148; if (dxgi == 77 || dxgi == 78) bc3 = true; else if (dxgi == 71 || dxgi == 72) bc3 = false; else throw new InvalidDataException("DXGI " + dxgi); }
        else if (fourCC == "DXT5" || fourCC == "DXT4") bc3 = true;
        else if (fourCC == "DXT1") bc3 = false;
        else throw new InvalidDataException("Format " + fourCC);
        var px = new int[width * height];
        int blockSize = bc3 ? 16 : 8, colors = 0;
        var palette = new int[4];
        for (int by = 0; by < (height + 3) / 4; by++)
            for (int bx = 0; bx < (width + 3) / 4; bx++)
            {
                int p = offset + (by * ((width + 3) / 4) + bx) * blockSize;
                int c = bc3 ? p + 8 : p;
                int c0 = BitConverter.ToUInt16(b, c), c1 = BitConverter.ToUInt16(b, c + 2);
                palette[0] = Rgb565(c0); palette[1] = Rgb565(c1);
                if (c0 > c1 || bc3) { palette[2] = Mix(palette[0], palette[1], 2, 1); palette[3] = Mix(palette[0], palette[1], 1, 2); }
                else { palette[2] = Mix(palette[0], palette[1], 1, 1); palette[3] = 0; }
                uint bits = BitConverter.ToUInt32(b, c + 4);
                for (int k = 0; k < 16; k++)
                {
                    int x = bx * 4 + (k & 3), y = by * 4 + (k >> 2);
                    if (x >= width || y >= height) continue;
                    px[y * width + x] = palette[(bits >> (2 * k)) & 3] | unchecked((int)0xFF000000);
                }
                colors++;
            }
        if (width == maxW && height == maxH) return px;
        var scaled = new int[maxW * maxH];
        for (int y = 0; y < maxH; y++) for (int x = 0; x < maxW; x++) scaled[y * maxW + x] = px[(y * height / maxH) * width + x * width / maxW];
        return scaled;
    }
    static int Rgb565(int c) { int r = (c >> 11) & 31, g = (c >> 5) & 63, b = c & 31; return ((r * 255 / 31) << 16) | ((g * 255 / 63) << 8) | (b * 255 / 31); }
    static int Mix(int a, int b, int wa, int wb)
    {
        int r = (((a >> 16) & 255) * wa + ((b >> 16) & 255) * wb) / (wa + wb), g = (((a >> 8) & 255) * wa + ((b >> 8) & 255) * wb) / (wa + wb), bl = ((a & 255) * wa + (b & 255) * wb) / (wa + wb);
        return (r << 16) | (g << 8) | bl;
    }

    public void Dispose() { Image?.Dispose(); Image = null; }
}
