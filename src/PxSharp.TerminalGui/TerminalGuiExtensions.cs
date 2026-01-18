using Terminal.Gui;
using TgColor = Terminal.Gui.Color;
using TgAttribute = Terminal.Gui.Attribute;

namespace PxSharp.TerminalGui;

/// <summary>
/// Extension methods for integrating PxSharp with Terminal.Gui.
/// </summary>
public static class TerminalGuiExtensions
{
    /// <summary>
    /// Creates a Terminal.Gui View that renders the image.
    /// </summary>
    public static View ToTerminalGuiView(this PxImage image) => new PxImageView(image);
}

/// <summary>
/// A Terminal.Gui View that renders a PxImage using half-block characters.
/// </summary>
public class PxImageView : View
{
    readonly PxImage _image;

    public PxImageView(PxImage image)
    {
        _image = image;
        Width = image.CharWidth;
        Height = image.CharHeight;
    }

    public override void Redraw(Rect bounds)
    {
        base.Redraw(bounds);

        for (var y = 0; y < _image.CharHeight && y < bounds.Height; y++)
        {
            for (var x = 0; x < _image.CharWidth && x < bounds.Width; x++)
            {
                var (glyph, fg, bg) = _image.GetCell(x, y);

                var fgColor = ToTerminalGuiColor(fg);
                var bgColor = ToTerminalGuiColor(bg);

                // Use default colors for transparent parts
                if (fg.IsTransparent) fgColor = ColorScheme?.Normal.Foreground ?? TgColor.White;
                if (bg.IsTransparent) bgColor = ColorScheme?.Normal.Background ?? TgColor.Black;

                var attr = new TgAttribute(fgColor, bgColor);
                Driver.SetAttribute(attr);

                Move(x, y);
                Driver.AddRune(glyph);
            }
        }
    }

    static TgColor ToTerminalGuiColor(Color c)
    {
        if (c.IsTransparent)
            return TgColor.Black;

        // Terminal.Gui v1 uses 16-color palette by default
        // We'll map to the closest ANSI color
        return MapToAnsi16(c);
    }

    static TgColor MapToAnsi16(Color c)
    {
        // Simple mapping to 16 ANSI colors based on RGB intensity
        var r = c.R >= 128;
        var g = c.G >= 128;
        var b = c.B >= 128;
        var bright = c.R >= 192 || c.G >= 192 || c.B >= 192;

        // Basic 8 colors + bright variants
        var baseColor = (r, g, b) switch
        {
            (false, false, false) => bright ? TgColor.DarkGray : TgColor.Black,
            (true, false, false) => bright ? TgColor.BrightRed : TgColor.Red,
            (false, true, false) => bright ? TgColor.BrightGreen : TgColor.Green,
            (true, true, false) => bright ? TgColor.BrightYellow : TgColor.Brown,
            (false, false, true) => bright ? TgColor.BrightBlue : TgColor.Blue,
            (true, false, true) => bright ? TgColor.BrightMagenta : TgColor.Magenta,
            (false, true, true) => bright ? TgColor.BrightCyan : TgColor.Cyan,
            (true, true, true) => bright ? TgColor.White : TgColor.Gray,
        };

        return baseColor;
    }
}
