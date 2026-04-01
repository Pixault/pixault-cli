using System.ComponentModel;
using Pixault.Client;
using Pixault.Cli.Infrastructure;
using Pixault.Cli.Shared;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Pixault.Cli.Commands.Images;

public sealed class ImagesEmbedSettings : ProjectSettings
{
    [CommandArgument(0, "<id>")]
    [Description("Image ID")]
    public string ImageId { get; set; } = "";

    [CommandOption("-w|--widths")]
    [Description("Comma-separated breakpoint widths (default: 400,800,1200)")]
    public string? Widths { get; set; }

    [CommandOption("--sizes")]
    [Description("CSS sizes attribute (default: 100vw)")]
    public string? Sizes { get; set; }

    [CommandOption("--alt")]
    [Description("Alt text for the image")]
    public string? Alt { get; set; }

    [CommandOption("-t|--transform")]
    [Description("Named transform to apply")]
    public string? Transform { get; set; }

    [CommandOption("-q|--quality")]
    [Description("Quality (1-100)")]
    public int? Quality { get; set; }

    [CommandOption("--loading")]
    [Description("Loading behavior: lazy or eager (default: lazy)")]
    public string? Loading { get; set; }

    [CommandOption("--class")]
    [Description("CSS class(es) for the img element")]
    public string? CssClass { get; set; }

    [CommandOption("--picture")]
    [DefaultValue(true)]
    [Description("Use <picture> element with AVIF/WebP sources (default: true)")]
    public bool Picture { get; set; } = true;
}

public sealed class ImagesEmbedCommand(ClientFactory factory) : Command<ImagesEmbedSettings>
{
    public override int Execute(CommandContext context, ImagesEmbedSettings settings)
    {
        OutputHelpers.WriteHeader("images embed");

        var builder = factory.CreateUrlBuilder(settings.ImageId, settings.Project);

        if (settings.Transform is not null) builder.Transform(settings.Transform);
        if (settings.Quality.HasValue) builder.Quality(settings.Quality.Value);

        var widths = (settings.Widths ?? "400,800,1200")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(w => int.TryParse(w, out var v) ? v : 0)
            .Where(w => w > 0)
            .ToArray();

        var alt = settings.Alt ?? "";
        var sizes = settings.Sizes ?? "100vw";
        var loading = settings.Loading ?? "lazy";

        string html;
        if (settings.Picture)
        {
            html = builder.ToPictureTag(alt, widths, sizes, loading, settings.CssClass);
        }
        else
        {
            html = builder.ToImgTag(alt, widths, sizes, loading, settings.CssClass);
        }

        AnsiConsole.Write(new Panel(new Text(html))
            .RoundedBorder()
            .BorderColor(Color.DodgerBlue1)
            .Header($"[{CliConstants.Brand}]Embed HTML[/]"));

        return 0;
    }
}
