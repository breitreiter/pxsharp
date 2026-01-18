// Simple tool to generate test BMP files
var samplesDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "assets");
Directory.CreateDirectory(samplesDir);

CreateGradientBmp(Path.Combine(samplesDir, "gradient.bmp"));

Console.WriteLine($"Test assets created in: {Path.GetFullPath(samplesDir)}");

static void CreateGradientBmp(string path)
{
    var width = 16;
    var height = 8;

    using var stream = File.Create(path);
    using var writer = new BinaryWriter(stream);

    var bytesPerPixel = 3;
    var rowPadding = (4 - (width * bytesPerPixel) % 4) % 4;
    var rowSize = width * bytesPerPixel + rowPadding;
    var pixelDataSize = rowSize * height;
    var fileSize = 54 + pixelDataSize;

    writer.Write((ushort)0x4D42);
    writer.Write((uint)fileSize);
    writer.Write((uint)0);
    writer.Write((uint)54);

    writer.Write((uint)40);
    writer.Write(width);
    writer.Write(height);
    writer.Write((ushort)1);
    writer.Write((ushort)24);
    writer.Write((uint)0);
    writer.Write((uint)pixelDataSize);
    writer.Write(2835);
    writer.Write(2835);
    writer.Write((uint)0);
    writer.Write((uint)0);

    for (var y = height - 1; y >= 0; y--)
    {
        for (var x = 0; x < width; x++)
        {
            var r = (byte)(255 - x * 16);
            var g = (byte)(y * 32);
            var b = (byte)(x * 16);
            writer.Write(b);
            writer.Write(g);
            writer.Write(r);
        }
        for (var p = 0; p < rowPadding; p++)
            writer.Write((byte)0);
    }

    Console.WriteLine($"  Created: {Path.GetFileName(path)}");
}

