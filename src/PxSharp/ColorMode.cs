namespace PxSharp;

/// <summary>
/// Specifies the color output mode for terminal rendering.
/// </summary>
public enum ColorMode
{
    /// <summary>
    /// Auto-detect terminal capability from environment.
    /// </summary>
    Auto,

    /// <summary>
    /// Force 24-bit true color output.
    /// </summary>
    TrueColor,

    /// <summary>
    /// Force 256-color palette output.
    /// </summary>
    Palette256
}
