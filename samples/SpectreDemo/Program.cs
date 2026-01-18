// Demo: Using PxSharp with Spectre.Console

using PxSharp;
using Spectre.Console;

var robot = PxImage.Load("../assets/robot-logo.bmp");

// Simple: just dump the image
AnsiConsole.MarkupLine("[bold]Simple render:[/]");
robot.WriteAnsi(Console.Out);
Console.WriteLine();

// Composed: image alongside text (nb-style banner)
AnsiConsole.MarkupLine("[bold]Composed with text:[/]");
robot.Print(0); AnsiConsole.MarkupLine("  [bold blue]nb[/] v1.0.0");
robot.Print(1); AnsiConsole.MarkupLine("  [grey]A notebook CLI[/]");
robot.Print(2); AnsiConsole.MarkupLine("  [grey]MIT License[/]");
