namespace PxSharp.Tests;

/// <summary>
/// Helper to generate test BMP files for integration testing.
/// </summary>
public static class TestBmpGenerator
{
    /// <summary>
    /// Creates a colorful gradient BMP for visual testing.
    /// </summary>
    public static void CreateGradientBmp(string path, int width = 16, int height = 8)
    {
        using var stream = File.Create(path);
        using var writer = new BinaryWriter(stream);

        var bytesPerPixel = 3; // 24-bit
        var rowPadding = (4 - (width * bytesPerPixel) % 4) % 4;
        var rowSize = width * bytesPerPixel + rowPadding;
        var pixelDataSize = rowSize * height;
        var fileSize = 54 + pixelDataSize;

        // File header
        writer.Write((ushort)0x4D42);
        writer.Write((uint)fileSize);
        writer.Write((uint)0);
        writer.Write((uint)54);

        // DIB header
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

        // Pixel data - gradient from red to blue with green varying by row
        for (var y = height - 1; y >= 0; y--)
        {
            for (var x = 0; x < width; x++)
            {
                var r = (byte)(255 - x * (255 / width));
                var g = (byte)(y * (255 / height));
                var b = (byte)(x * (255 / width));
                writer.Write(b);
                writer.Write(g);
                writer.Write(r);
            }
            for (var p = 0; p < rowPadding; p++)
                writer.Write((byte)0);
        }
    }

    /// <summary>
    /// Creates a simple robot-like face BMP for testing.
    /// </summary>
    public static void CreateRobotBmp(string path)
    {
        // 8x8 robot face
        var width = 8;
        var height = 8;

        // Define colors
        var bg = (r: (byte)50, g: (byte)50, b: (byte)60);      // dark gray background
        var face = (r: (byte)180, g: (byte)180, b: (byte)190); // light gray face
        var eye = (r: (byte)0, g: (byte)200, b: (byte)100);    // green eyes
        var mouth = (r: (byte)200, g: (byte)50, b: (byte)50);  // red mouth

        // 8x8 pixel art (row 0 is top)
        var pixels = new (byte r, byte g, byte b)[]
        {
            // Row 0 - top
            bg, face, face, face, face, face, face, bg,
            // Row 1
            face, face, face, face, face, face, face, face,
            // Row 2 - eyes
            face, eye, face, face, face, eye, face, face,
            // Row 3 - eyes
            face, eye, face, face, face, eye, face, face,
            // Row 4
            face, face, face, face, face, face, face, face,
            // Row 5 - mouth
            face, face, mouth, mouth, mouth, mouth, face, face,
            // Row 6
            face, face, face, face, face, face, face, face,
            // Row 7 - bottom
            bg, face, face, face, face, face, face, bg,
        };

        using var stream = File.Create(path);
        using var writer = new BinaryWriter(stream);

        var bytesPerPixel = 3;
        var rowPadding = (4 - (width * bytesPerPixel) % 4) % 4;
        var rowSize = width * bytesPerPixel + rowPadding;
        var pixelDataSize = rowSize * height;
        var fileSize = 54 + pixelDataSize;

        // File header
        writer.Write((ushort)0x4D42);
        writer.Write((uint)fileSize);
        writer.Write((uint)0);
        writer.Write((uint)54);

        // DIB header
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

        // Pixel data (bottom-up)
        for (var y = height - 1; y >= 0; y--)
        {
            for (var x = 0; x < width; x++)
            {
                var p = pixels[y * width + x];
                writer.Write(p.b);
                writer.Write(p.g);
                writer.Write(p.r);
            }
            for (var i = 0; i < rowPadding; i++)
                writer.Write((byte)0);
        }
    }
}
