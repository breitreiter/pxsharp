namespace PxSharp;

/// <summary>
/// Global settings for PxSharp rendering.
/// </summary>
public static class PxSharpSettings
{
    /// <summary>
    /// Gets or sets the color mode used for terminal rendering.
    /// Default is Auto, which detects terminal capability from environment.
    /// </summary>
    public static ColorMode ColorMode { get; set; } = ColorMode.Auto;

    /// <summary>
    /// Gets or sets the color key used for transparency.
    /// Pixels matching this RGB value (ignoring alpha) are treated as transparent.
    /// Default is magenta (#FF00FF).
    /// </summary>
    public static (byte R, byte G, byte B) TransparentColor { get; set; } = (255, 0, 255);

    /// <summary>
    /// Gets the effective color mode, evaluating Auto if needed.
    /// Use this to determine what mode was actually selected.
    /// </summary>
    public static ColorMode ResolvedColorMode => ColorMode == ColorMode.Auto
        ? DetectColorMode()
        : ColorMode;

    static ColorMode DetectColorMode()
    {
        // Check COLORTERM environment variable (common on Linux/macOS)
        var colorTerm = Environment.GetEnvironmentVariable("COLORTERM");
        if (colorTerm is "truecolor" or "24bit")
            return ColorMode.TrueColor;

        // Check TERM for common true color terminals
        var term = Environment.GetEnvironmentVariable("TERM");
        if (term != null && (term.Contains("256color") || term.Contains("truecolor")))
            return ColorMode.TrueColor;

        // Windows Terminal, VS Code, and modern terminals often support true color
        // Check for WT_SESSION (Windows Terminal) or TERM_PROGRAM
        if (Environment.GetEnvironmentVariable("WT_SESSION") != null)
            return ColorMode.TrueColor;

        var termProgram = Environment.GetEnvironmentVariable("TERM_PROGRAM");
        if (termProgram is "vscode" or "iTerm.app" or "Apple_Terminal")
            return ColorMode.TrueColor;

        // Default to 256 color for safety
        return ColorMode.Palette256;
    }
}
