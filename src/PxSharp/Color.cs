namespace PxSharp;

/// <summary>
/// Represents an RGBA color.
/// </summary>
public readonly struct Color : IEquatable<Color>
{
    public byte R { get; }
    public byte G { get; }
    public byte B { get; }
    public byte A { get; }

    public Color(byte r, byte g, byte b, byte a = 255)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public bool IsTransparent
    {
        get
        {
            var key = PxSharpSettings.TransparentColor;
            return R == key.R && G == key.G && B == key.B;
        }
    }

    public bool IsOpaque => !IsTransparent;

    public static Color Transparent
    {
        get
        {
            var key = PxSharpSettings.TransparentColor;
            return new(key.R, key.G, key.B);
        }
    }

    public bool Equals(Color other) => R == other.R && G == other.G && B == other.B && A == other.A;
    public override bool Equals(object? obj) => obj is Color other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(R, G, B, A);
    public static bool operator ==(Color left, Color right) => left.Equals(right);
    public static bool operator !=(Color left, Color right) => !left.Equals(right);

    public override string ToString() => $"#{R:X2}{G:X2}{B:X2}{(A < 255 ? A.ToString("X2") : "")}";
}
