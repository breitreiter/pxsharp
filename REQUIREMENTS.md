# pxsharp

A zero-dependency library for rendering pixel art in C# console applications.

## Overview

pxsharp lets you load BMP images and render them to the terminal using Unicode block characters. It integrates with common console UI libraries (Spectre.Console, Terminal.Gui) and has no external dependencies.

## Components

1. **Core library** (`PxSharp`)
   - BMP loader (24-bit RGB and 32-bit RGBA, uncompressed)
   - Console renderer using half-block characters (▀▄█)
   - ANSI escape code output (no dependencies)

2. **Terminal.Gui integration** (`PxSharp.TerminalGui`)
   - `ToTerminalGuiView()` extension method for TUI apps

3. **CLI tool** (`PxSharp.Cli`)
   - Preview BMP files: `pxsharp preview foo.bmp`
   - Validate BMP files for compatibility
   - Future: `--simulate-256` flag for testing low-color rendering
   - Future: warning message when terminal forces 256-color mode ("not our fault your picture looks bad")

## Design Decisions

### Why BMP?

We considered PNG with a library like StbImageSharp, but BMP offers:
- Zero dependencies (simple format, ~100-150 lines to parse)
- No licensing concerns for downstream users
- Universal export support (Aseprite, GIMP, Photoshop, etc.)

Tradeoff: BMP files are larger than PNG. For console-sized art (~200x100 pixels), this means ~80KB vs ~5KB. Acceptable for asset files.

### Rendering Approach

Console "pixels" are character cells. We use half-block characters to get 2 vertical pixels per cell:

```
▀ = top half (fg = top pixel, bg = bottom pixel)
▄ = lower half
█ = full block
  = space (both transparent)
```

This means a 200x100 pixel image renders as 200x50 characters.

### Transparency

Handled per-cell based on the two pixels it represents:

| Top | Bottom | Output |
|-----|--------|--------|
| Transparent | Transparent | Space with default background |
| Transparent | Opaque | ▄ with fg=bottom, bg=default |
| Opaque | Transparent | ▀ with fg=top, bg=default |
| Opaque | Opaque | ▀ with fg=top, bg=bottom |

### Color Support

- **Primary:** 24-bit true color (most modern terminals)
- **Fallback:** 256-color palette for limited terminals

Runtime detection determines terminal capability.

## Constraints

- **Input:** 24-bit or 32-bit uncompressed BMP only (no RLE, no indexed color)
- **Animation:** Not supported (single frames only—console rendering is slow)
- **Target framework:** net8.0
- **License:** MIT

## Error Handling

**Exceptions thrown for:**
- File not found
- Invalid BMP (bad magic bytes, corrupted header)
- Unsupported BMP variant (RLE compression, indexed color, 16-bit)

**Graceful degradation for:**
- Odd pixel height: bottom row padded with transparency (no exception)
- Limited terminal color support: auto-downgrades to 256-color palette

Philosophy: throw on "you gave me something I can't use", degrade gracefully on "this will look weird but I can render it."

## API Design

### Core Type

```csharp
var image = PxImage.Load("logo.bmp");

// Dimensions
image.PixelWidth    // source image width in pixels
image.PixelHeight   // source image height in pixels
image.CharWidth     // output width in characters (= PixelWidth)
image.CharHeight    // output height in characters (= PixelHeight / 2)

// Line rendering (for scrolling CLI, composable output)
image.GetAnsiLine(index)     // string - single line with ANSI codes
image.GetAnsiLines()         // string[] - all lines
image.WriteAnsi(TextWriter)  // dump directly to stream

// Convenient printing to stdout
image.Print(row)             // print row without newline (for composing with text)
image.PrintLine(row)         // print row with newline

// Terminal.Gui integration (via PxSharp.TerminalGui)
image.ToTerminalGuiView()    // Terminal.Gui.View
```

### Color Mode

Auto-detect terminal capability by default, with global override:

```csharp
PxSharp.ColorMode = ColorMode.Auto;       // default - detect from environment
PxSharp.ColorMode = ColorMode.TrueColor;  // force 24-bit
PxSharp.ColorMode = ColorMode.Palette256; // force 256-color
```

### Spectre.Console Integration

No separate package needed. Use `Print()` to output image rows alongside Spectre markup:

```csharp
var robot = PxImage.Load("robot.bmp");

robot.Print(0); AnsiConsole.MarkupLine("  [bold blue]myapp[/] v1.0");
robot.Print(1); AnsiConsole.MarkupLine("  [grey]A cool tool[/]");
robot.Print(2); AnsiConsole.MarkupLine("  [grey]MIT License[/]");
```

`Print()` outputs raw ANSI to stdout without a newline, so you can append Spectre markup on the same line.
Spectre's Canvas widget uses full blocks (1 char = 1 pixel)—our half-block approach is more compact.

### Design Principles

- Line-based output for scrolling CLI allows composition (e.g., image alongside version text)
- Framework-agnostic core; integrations are lightweight or documentation-only
- "Cell" is an internal concept; users think in pixels and characters

## Project Structure

Monorepo layout:

```
pxsharp/
├── src/
│   ├── PxSharp/              # Core library (zero deps)
│   ├── PxSharp.TerminalGui/  # Terminal.Gui integration
│   └── PxSharp.Cli/          # CLI tool
├── tests/
│   └── PxSharp.Tests/
├── samples/
│   ├── assets/               # Test BMPs
│   └── SpectreDemo/          # Example Spectre.Console integration
└── PxSharp.sln
```

Package names are PascalCase per NuGet convention. No org prefix for now - `PxSharp` is distinctive enough. Can revisit if there are collisions.

## Expected Workflow

1. Create pixel art in Aseprite, GIMP, etc.
2. Export as 24-bit or 32-bit BMP
3. Load with pxsharp at runtime or preview with CLI