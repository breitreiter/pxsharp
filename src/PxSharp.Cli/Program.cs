using PxSharp;

if (args.Length == 0)
{
    ShowHelp();
    return 0;
}

var command = args[0].ToLowerInvariant();

return command switch
{
    "preview" => Preview(args.Skip(1).ToArray()),
    "validate" => Validate(args.Skip(1).ToArray()),
    "help" or "--help" or "-h" => ShowHelp(),
    _ => UnknownCommand(command)
};

int Preview(string[] args)
{
    if (args.Length == 0)
    {
        Console.Error.WriteLine("Usage: pxsharp preview <file.bmp> [--256]");
        return 1;
    }

    var path = args[0];
    var force256 = args.Contains("--256");

    if (force256)
        PxSharpSettings.ColorMode = ColorMode.Palette256;

    try
    {
        var image = PxImage.Load(path);

        if (PxSharpSettings.ResolvedColorMode == ColorMode.Palette256)
        {
            Console.Error.WriteLine("(Rendering in 256-color mode - colors may appear degraded)");
        }

        image.WriteAnsi(Console.Out);

        Console.Error.WriteLine();
        Console.Error.WriteLine($"Image: {image.PixelWidth}x{image.PixelHeight} pixels, rendered as {image.CharWidth}x{image.CharHeight} characters");

        return 0;
    }
    catch (FileNotFoundException ex)
    {
        Console.Error.WriteLine($"Error: {ex.Message}");
        return 1;
    }
    catch (InvalidDataException ex)
    {
        Console.Error.WriteLine($"Error: {ex.Message}");
        return 1;
    }
}

int Validate(string[] args)
{
    if (args.Length == 0)
    {
        Console.Error.WriteLine("Usage: pxsharp validate <file.bmp>");
        return 1;
    }

    var path = args[0];

    try
    {
        var image = PxImage.Load(path);
        Console.WriteLine($"Valid BMP: {image.PixelWidth}x{image.PixelHeight} pixels");

        if (image.HasOddHeight)
            Console.WriteLine("Warning: Odd pixel height - bottom row will be padded with transparency");

        return 0;
    }
    catch (FileNotFoundException ex)
    {
        Console.Error.WriteLine($"Invalid: {ex.Message}");
        return 1;
    }
    catch (InvalidDataException ex)
    {
        Console.Error.WriteLine($"Invalid: {ex.Message}");
        return 1;
    }
}

int ShowHelp()
{
    Console.WriteLine("pxsharp - Render BMP images in the terminal");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    Console.WriteLine("  preview <file.bmp> [--256]  Render image to terminal");
    Console.WriteLine("  validate <file.bmp>         Check if BMP is compatible");
    Console.WriteLine("  help                        Show this help message");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --256  Force 256-color mode (auto-detected by default)");
    return 0;
}

int UnknownCommand(string command)
{
    Console.Error.WriteLine($"Unknown command: {command}");
    Console.Error.WriteLine("Run 'pxsharp help' for usage information.");
    return 1;
}
