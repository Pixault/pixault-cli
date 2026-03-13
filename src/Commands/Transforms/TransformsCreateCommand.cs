using System.ComponentModel;
using Pixault.Client;
using Pixault.Cli.Infrastructure;
using Pixault.Cli.Shared;
using Spectre.Console.Cli;

namespace Pixault.Cli.Commands.Transforms;

public sealed class TransformsCreateSettings : ProjectSettings
{
    [CommandArgument(0, "<name>")]
    [Description("Transform name")]
    public string Name { get; set; } = "";

    [CommandOption("-w|--width")]
    [Description("Width in pixels")]
    public int? Width { get; set; }

    [CommandOption("-h|--height")]
    [Description("Height in pixels")]
    public int? Height { get; set; }

    [CommandOption("-q|--quality")]
    [Description("Quality (1-100)")]
    public int? Quality { get; set; }

    [CommandOption("--fit")]
    [Description("Fit mode: cover, contain, fill, pad")]
    public string? Fit { get; set; }

    [CommandOption("--blur")]
    [Description("Blur radius")]
    public int? Blur { get; set; }

    [CommandOption("--watermark")]
    [Description("Watermark image ID")]
    public string? WatermarkId { get; set; }

    [CommandOption("--wm-position")]
    [Description("Watermark position")]
    public string? WatermarkPosition { get; set; }

    [CommandOption("--wm-opacity")]
    [Description("Watermark opacity (0-100)")]
    public int? WatermarkOpacity { get; set; }

    [CommandOption("--lock")]
    [Description("Comma-separated parameter names to lock")]
    public string? Lock { get; set; }
}

public sealed class TransformsCreateCommand(ClientFactory factory) : AsyncCommand<TransformsCreateSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, TransformsCreateSettings settings)
    {
        OutputHelpers.WriteHeader("transforms create");

        var save = new NamedTransformSave
        {
            Width = settings.Width,
            Height = settings.Height,
            Quality = settings.Quality,
            FitMode = settings.Fit,
            Blur = settings.Blur,
            WatermarkId = settings.WatermarkId,
            WatermarkPosition = settings.WatermarkPosition,
            WatermarkOpacity = settings.WatermarkOpacity,
            LockedParameters = settings.Lock?
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToHashSet()
        };

        var client = factory.CreateAdminClient(settings.Project);
        await client.SaveTransformAsync(settings.Name, save, settings.Project);

        OutputHelpers.WriteSuccess($"Created transform: {settings.Name}");
        return 0;
    }
}
