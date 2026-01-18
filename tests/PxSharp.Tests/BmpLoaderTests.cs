namespace PxSharp.Tests;

public class BmpLoaderTests
{
    [Fact]
    public void Load_ValidBmp24Bit_ReturnsCorrectDimensions()
    {
        var path = CreateTestBmp(4, 4, bitsPerPixel: 24);
        try
        {
            var image = PxImage.Load(path);

            Assert.Equal(4, image.PixelWidth);
            Assert.Equal(4, image.PixelHeight);
            Assert.Equal(4, image.CharWidth);
            Assert.Equal(2, image.CharHeight);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Load_ValidBmp32Bit_ReturnsCorrectDimensions()
    {
        var path = CreateTestBmp(8, 6, bitsPerPixel: 32);
        try
        {
            var image = PxImage.Load(path);

            Assert.Equal(8, image.PixelWidth);
            Assert.Equal(6, image.PixelHeight);
            Assert.Equal(8, image.CharWidth);
            Assert.Equal(3, image.CharHeight);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Load_OddHeight_HasOddHeightIsTrue()
    {
        var path = CreateTestBmp(4, 5, bitsPerPixel: 24);
        try
        {
            var image = PxImage.Load(path);

            Assert.True(image.HasOddHeight);
            Assert.Equal(3, image.CharHeight); // (5 + 1) / 2 = 3
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Load_NonExistentFile_ThrowsFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(() => PxImage.Load("/nonexistent/file.bmp"));
    }

    [Fact]
    public void Load_InvalidMagicBytes_ThrowsInvalidDataException()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, new byte[] { 0x00, 0x00, 0x00, 0x00 });
            Assert.Throws<InvalidDataException>(() => PxImage.Load(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Load_PixelColors_AreCorrect()
    {
        // Create a 2x2 BMP with known colors
        var path = CreateTestBmpWithColors(2, 2, new Color[]
        {
            new(255, 0, 0),   // top-left: red
            new(0, 255, 0),   // top-right: green
            new(0, 0, 255),   // bottom-left: blue
            new(255, 255, 0), // bottom-right: yellow
        });

        try
        {
            var image = PxImage.Load(path);

            Assert.Equal(new Color(255, 0, 0), image.GetPixel(0, 0));
            Assert.Equal(new Color(0, 255, 0), image.GetPixel(1, 0));
            Assert.Equal(new Color(0, 0, 255), image.GetPixel(0, 1));
            Assert.Equal(new Color(255, 255, 0), image.GetPixel(1, 1));
        }
        finally
        {
            File.Delete(path);
        }
    }

    static string CreateTestBmp(int width, int height, int bitsPerPixel)
    {
        var colors = new Color[width * height];
        for (var i = 0; i < colors.Length; i++)
            colors[i] = new Color((byte)(i * 17), (byte)(i * 23), (byte)(i * 31));
        return CreateTestBmpWithColors(width, height, colors, bitsPerPixel);
    }

    static string CreateTestBmpWithColors(int width, int height, Color[] colors, int bitsPerPixel = 24)
    {
        var path = Path.GetTempFileName();
        using var stream = File.Create(path);
        using var writer = new BinaryWriter(stream);

        var bytesPerPixel = bitsPerPixel / 8;
        var rowPadding = (4 - (width * bytesPerPixel) % 4) % 4;
        var rowSize = width * bytesPerPixel + rowPadding;
        var pixelDataSize = rowSize * height;
        var fileSize = 54 + pixelDataSize; // 14 (file header) + 40 (DIB header) + pixels

        // File header (14 bytes)
        writer.Write((ushort)0x4D42); // "BM"
        writer.Write((uint)fileSize);
        writer.Write((uint)0); // reserved
        writer.Write((uint)54); // pixel data offset

        // DIB header (BITMAPINFOHEADER, 40 bytes)
        writer.Write((uint)40); // header size
        writer.Write(width);
        writer.Write(height); // positive = bottom-up
        writer.Write((ushort)1); // planes
        writer.Write((ushort)bitsPerPixel);
        writer.Write((uint)0); // compression (none)
        writer.Write((uint)pixelDataSize);
        writer.Write(2835); // horizontal resolution (72 DPI)
        writer.Write(2835); // vertical resolution
        writer.Write((uint)0); // colors in palette
        writer.Write((uint)0); // important colors

        // Pixel data (bottom-up)
        for (var y = height - 1; y >= 0; y--)
        {
            for (var x = 0; x < width; x++)
            {
                var color = colors[y * width + x];
                writer.Write(color.B);
                writer.Write(color.G);
                writer.Write(color.R);
                if (bitsPerPixel == 32)
                    writer.Write(color.A);
            }
            for (var p = 0; p < rowPadding; p++)
                writer.Write((byte)0);
        }

        return path;
    }
}
