namespace PxSharp;

internal static class BmpLoader
{
    public static (int width, int height, Color[] pixels) Load(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"BMP file not found: {path}", path);

        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);

        // File header (14 bytes)
        var magic = reader.ReadUInt16();
        if (magic != 0x4D42) // "BM" in little-endian
            throw new InvalidDataException($"Invalid BMP file: expected 'BM' magic bytes, got 0x{magic:X4}");

        reader.ReadUInt32(); // file size (ignore)
        reader.ReadUInt32(); // reserved (ignore)
        var pixelOffset = reader.ReadUInt32();

        // DIB header
        var headerSize = reader.ReadUInt32();
        if (headerSize < 40)
            throw new InvalidDataException($"Unsupported BMP header size: {headerSize}. Expected BITMAPINFOHEADER (40+).");

        var width = reader.ReadInt32();
        var height = reader.ReadInt32();
        var topDown = height < 0;
        height = Math.Abs(height);

        reader.ReadUInt16(); // planes (ignore)
        var bitsPerPixel = reader.ReadUInt16();

        if (bitsPerPixel != 24 && bitsPerPixel != 32)
            throw new InvalidDataException($"Unsupported BMP bit depth: {bitsPerPixel}. Only 24-bit and 32-bit are supported.");

        var compression = reader.ReadUInt32();
        // compression 0 = BI_RGB (uncompressed)
        // compression 3 = BI_BITFIELDS (we'll accept it for 32-bit, assume BGRA)
        if (compression != 0 && compression != 3)
            throw new InvalidDataException($"Unsupported BMP compression: {compression}. Only uncompressed BMPs are supported.");

        // Skip rest of header and seek to pixel data
        stream.Seek(pixelOffset, SeekOrigin.Begin);

        var pixels = new Color[width * height];
        var bytesPerPixel = bitsPerPixel / 8;
        var rowPadding = (4 - (width * bytesPerPixel) % 4) % 4;

        for (var y = 0; y < height; y++)
        {
            // BMP stores rows bottom-to-top by default, unless height is negative
            var destY = topDown ? y : height - 1 - y;

            for (var x = 0; x < width; x++)
            {
                var b = reader.ReadByte();
                var g = reader.ReadByte();
                var r = reader.ReadByte();
                var a = bitsPerPixel == 32 ? reader.ReadByte() : (byte)255;

                pixels[destY * width + x] = new Color(r, g, b, a);
            }

            // Skip row padding
            if (rowPadding > 0)
                reader.ReadBytes(rowPadding);
        }

        return (width, height, pixels);
    }
}
