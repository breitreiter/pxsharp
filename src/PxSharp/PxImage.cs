namespace PxSharp;

/// <summary>
/// Represents a loaded pixel image ready for console rendering.
/// </summary>
public sealed class PxImage
{
    readonly Color[] _pixels;

    PxImage(int width, int height, Color[] pixels)
    {
        PixelWidth = width;
        PixelHeight = height;
        _pixels = pixels;
    }

    /// <summary>
    /// Source image width in pixels.
    /// </summary>
    public int PixelWidth { get; }

    /// <summary>
    /// Source image height in pixels.
    /// </summary>
    public int PixelHeight { get; }

    /// <summary>
    /// Output width in characters (same as PixelWidth).
    /// </summary>
    public int CharWidth => PixelWidth;

    /// <summary>
    /// Output height in characters (PixelHeight / 2, rounded up).
    /// </summary>
    public int CharHeight => (PixelHeight + 1) / 2;

    /// <summary>
    /// True if the source image has an odd height (bottom row will be padded with transparency).
    /// </summary>
    public bool HasOddHeight => PixelHeight % 2 != 0;

    /// <summary>
    /// Loads a BMP image from the specified file path.
    /// </summary>
    /// <param name="path">Path to a 24-bit or 32-bit uncompressed BMP file.</param>
    /// <returns>A PxImage ready for rendering.</returns>
    /// <exception cref="FileNotFoundException">The file does not exist.</exception>
    /// <exception cref="InvalidDataException">The file is not a valid or supported BMP.</exception>
    public static PxImage Load(string path)
    {
        var (width, height, pixels) = BmpLoader.Load(path);
        return new PxImage(width, height, pixels);
    }

    /// <summary>
    /// Gets the color of a pixel at the specified coordinates.
    /// </summary>
    public Color GetPixel(int x, int y)
    {
        if (x < 0 || x >= PixelWidth || y < 0 || y >= PixelHeight)
            throw new ArgumentOutOfRangeException($"Pixel coordinates ({x}, {y}) are out of bounds.");
        return _pixels[y * PixelWidth + x];
    }

    /// <summary>
    /// Gets the character cell data at the specified character coordinates.
    /// Each cell represents two vertical pixels using half-block characters.
    /// </summary>
    internal (char Glyph, Color Fg, Color Bg) GetCell(int x, int y)
    {
        if (x < 0 || x >= CharWidth || y < 0 || y >= CharHeight)
            throw new ArgumentOutOfRangeException($"Cell coordinates ({x}, {y}) are out of bounds.");

        var topY = y * 2;
        var bottomY = topY + 1;

        var top = GetPixelSafe(x, topY);
        var bottom = GetPixelSafe(x, bottomY);

        return DetermineCell(top, bottom);
    }

    Color GetPixelSafe(int x, int y)
    {
        // Return transparent for pixels outside bounds (handles odd height padding)
        if (x < 0 || x >= PixelWidth || y < 0 || y >= PixelHeight)
            return Color.Transparent;
        return _pixels[y * PixelWidth + x];
    }

    static (char Glyph, Color Fg, Color Bg) DetermineCell(Color top, Color bottom)
    {
        var topTransparent = top.IsTransparent;
        var bottomTransparent = bottom.IsTransparent;

        return (topTransparent, bottomTransparent) switch
        {
            (true, true) => (' ', Color.Transparent, Color.Transparent),
            (true, false) => ('▄', bottom, Color.Transparent),
            (false, true) => ('▀', top, Color.Transparent),
            (false, false) => ('▀', top, bottom),
        };
    }

    /// <summary>
    /// Gets a single line of ANSI-escaped output.
    /// </summary>
    /// <param name="index">Zero-based line index (0 to CharHeight - 1).</param>
    public string GetAnsiLine(int index)
    {
        if (index < 0 || index >= CharHeight)
            throw new ArgumentOutOfRangeException(nameof(index), $"Line index {index} is out of bounds (0 to {CharHeight - 1}).");

        return AnsiRenderer.RenderLine(this, index);
    }

    /// <summary>
    /// Gets all lines of ANSI-escaped output.
    /// </summary>
    public string[] GetAnsiLines()
    {
        var lines = new string[CharHeight];
        for (var i = 0; i < CharHeight; i++)
            lines[i] = GetAnsiLine(i);
        return lines;
    }

    /// <summary>
    /// Writes the image to a TextWriter using ANSI escape codes.
    /// </summary>
    public void WriteAnsi(TextWriter writer)
    {
        for (var i = 0; i < CharHeight; i++)
        {
            writer.WriteLine(GetAnsiLine(i));
        }
    }

    /// <summary>
    /// Prints a single row to stdout without a trailing newline.
    /// Useful for composing image rows with adjacent text.
    /// </summary>
    /// <param name="row">Zero-based row index (0 to CharHeight - 1).</param>
    public void Print(int row) => Console.Write(GetAnsiLine(row));

    /// <summary>
    /// Prints a single row to stdout with a trailing newline.
    /// </summary>
    /// <param name="row">Zero-based row index (0 to CharHeight - 1).</param>
    public void PrintLine(int row) => Console.WriteLine(GetAnsiLine(row));
}
