using System.Text;

namespace PxSharp;

internal static class AnsiRenderer
{
    const string Reset = "\x1b[0m";

    public static string RenderLine(PxImage image, int lineIndex)
    {
        var sb = new StringBuilder();
        var colorMode = PxSharpSettings.ResolvedColorMode;

        for (var x = 0; x < image.CharWidth; x++)
        {
            var (glyph, fg, bg) = image.GetCell(x, lineIndex);

            if (glyph == ' ' && bg.IsTransparent)
            {
                // Fully transparent cell - just output a space
                sb.Append(' ');
                continue;
            }

            // Build escape sequence
            var hasFg = !fg.IsTransparent;
            var hasBg = !bg.IsTransparent;

            if (hasFg || hasBg)
            {
                sb.Append("\x1b[");
                var needSeparator = false;

                if (hasFg)
                {
                    AppendFgColor(sb, fg, colorMode);
                    needSeparator = true;
                }

                if (hasBg)
                {
                    if (needSeparator) sb.Append(';');
                    AppendBgColor(sb, bg, colorMode);
                }

                sb.Append('m');
            }

            sb.Append(glyph);
            sb.Append(Reset);
        }

        return sb.ToString();
    }

    static void AppendFgColor(StringBuilder sb, Color c, ColorMode mode)
    {
        if (mode == ColorMode.TrueColor)
        {
            sb.Append($"38;2;{c.R};{c.G};{c.B}");
        }
        else
        {
            sb.Append($"38;5;{ToAnsi256(c)}");
        }
    }

    static void AppendBgColor(StringBuilder sb, Color c, ColorMode mode)
    {
        if (mode == ColorMode.TrueColor)
        {
            sb.Append($"48;2;{c.R};{c.G};{c.B}");
        }
        else
        {
            sb.Append($"48;5;{ToAnsi256(c)}");
        }
    }

    /// <summary>
    /// Converts an RGB color to the nearest ANSI 256-color palette index.
    /// Uses a simple nearest-match algorithm - no dithering.
    /// </summary>
    static int ToAnsi256(Color c)
    {
        // ANSI 256 color palette:
        // 0-15: Standard colors (we'll skip these for simplicity)
        // 16-231: 6x6x6 color cube
        // 232-255: Grayscale ramp

        // Check if it's close to grayscale
        var isGray = Math.Abs(c.R - c.G) < 8 && Math.Abs(c.G - c.B) < 8 && Math.Abs(c.R - c.B) < 8;

        if (isGray)
        {
            // Use grayscale ramp (232-255, 24 shades from dark to light)
            // Each step is about 10 units (range 8-238)
            var gray = (c.R + c.G + c.B) / 3;
            if (gray < 8) return 16; // black from color cube
            if (gray > 248) return 231; // white from color cube
            return 232 + (gray - 8) / 10;
        }

        // Use 6x6x6 color cube (indices 16-231)
        // Each axis has values: 0, 95, 135, 175, 215, 255 (roughly)
        var ri = ColorTo6(c.R);
        var gi = ColorTo6(c.G);
        var bi = ColorTo6(c.B);

        return 16 + 36 * ri + 6 * gi + bi;
    }

    static int ColorTo6(byte value)
    {
        // Map 0-255 to 0-5 for the 6x6x6 cube
        // Thresholds roughly at: 0-47->0, 48-114->1, 115-154->2, 155-194->3, 195-234->4, 235-255->5
        if (value < 48) return 0;
        if (value < 115) return 1;
        if (value < 155) return 2;
        if (value < 195) return 3;
        if (value < 235) return 4;
        return 5;
    }
}
