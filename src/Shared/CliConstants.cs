using Spectre.Console;

namespace Pixault.Cli.Shared;

public static class CliConstants
{
    public const string Brand = "dodgerblue1";
    public const string BrandAccent = "steelblue1";
    public const string Success = "green";
    public const string Warning = "yellow";
    public const string Error = "red";
    public const string Muted = "dim";

    public static Style BrandStyle { get; } = new(Color.DodgerBlue1);
    public static Style MutedStyle { get; } = new(decoration: Decoration.Dim);
}
