# PxSharp

Lightweight library for rendering pixel art in .NET console applications.

## Features

- Load 24-bit and 32-bit BMP images
- Render using half-block characters (▀▄) for compact output that preserves the original aspect ratio
- True color with automatic 256-color fallback

## Installation

```bash
dotnet add package PxSharp
```

## Quick Start

```csharp
using PxSharp;

var image = PxImage.Load("logo.bmp");

// Simple: dump to console
image.WriteAnsi(Console.Out);

// Composed: image alongside text
image.Print(0); Console.WriteLine("  MyApp v1.0");
image.Print(1); Console.WriteLine("  MIT License");
```

## API

```csharp
// Loading
var img = PxImage.Load("file.bmp");      // from file path
var img = PxImage.Load(stream);          // from Stream (for embedded resources)

// Dimensions
img.PixelWidth      // source width in pixels
img.PixelHeight     // source height in pixels
img.CharWidth       // output width in characters
img.CharHeight      // output height in characters (pixels / 2)

// Output
img.Print(row)      // print a row to stdout (no newline)
img.PrintLine(row)  // print a row to stdout (with newline)
img.WriteAnsi(tw)   // write all rows to TextWriter
img.GetAnsiLine(i)  // get row as ANSI string
img.GetAnsiLines()  // get all rows as string[]
```

## Color Mode

Auto-detects terminal capability. Override globally:

```csharp
PxSharpSettings.ColorMode = ColorMode.TrueColor;   // force 24-bit
PxSharpSettings.ColorMode = ColorMode.Palette256;  // force 256-color
```

## Transparency

Use a **color key** for transparency (like GIF). Pixels matching the key are transparent. Default is magenta (`#FF00FF`):

```csharp
PxSharpSettings.TransparentColor = (255, 0, 255);  // magenta (default)
PxSharpSettings.TransparentColor = (255, 0, 0);    // red
```

## Embedding Images

Embed BMP files as assembly resources for distribution:

```xml
<!-- In your .csproj -->
<ItemGroup>
  <EmbeddedResource Include="logo.bmp" />
</ItemGroup>
```

```csharp
using System.Reflection;

var asm = Assembly.GetExecutingAssembly();
using var stream = asm.GetManifestResourceStream("MyApp.logo.bmp");
var logo = PxImage.Load(stream);
```

Resource name format is `{Namespace}.{Filename}`.

## License

MIT
