using System.ComponentModel;
using Pixault.Client;
using Pixault.Cli.Infrastructure;
using Pixault.Cli.Shared;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Pixault.Cli.Commands.Images;

public sealed class ImagesUrlSettings : ProjectSettings
{
    [CommandArgument(0, "<id>")]
    [Description("Image ID")]
    public string ImageId { get; set; } = "";

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

    [CommandOption("-t|--transform")]
    [Description("Named transform to apply")]
    public string? Transform { get; set; }

    [CommandOption("--format")]
    [Description("Output format (webp, png, jpg, avif)")]
    public string? Format { get; set; }
}

public sealed class ImagesUrlCommand(ClientFactory factory) : Command<ImagesUrlSettings>
{
    public override int Execute(CommandContext context, ImagesUrlSettings settings)
    {
        OutputHelpers.WriteHeader("images url");

        var builder = factory.CreateUrlBuilder(settings.ImageId, settings.Project);

        if (settings.Transform is not null) builder.Transform(settings.Transform);
        if (settings.Width.HasValue) builder.Width(settings.Width.Value);
        if (settings.Height.HasValue) builder.Height(settings.Height.Value);
        if (settings.Quality.HasValue) builder.Quality(settings.Quality.Value);
        if (settings.Blur.HasValue) builder.Blur(settings.Blur.Value);
        if (settings.Format is not null) builder.Format(settings.Format);

        if (settings.Fit is not null && Enum.TryParse<FitMode>(settings.Fit, ignoreCase: true, out var fitMode))
            builder.Fit(fitMode);

        var url = builder.Build();

        AnsiConsole.Write(new Panel(new Markup($"[link={url}]{url.EscapeMarkup()}[/]"))
            .RoundedBorder()
            .BorderColor(Color.DodgerBlue1)
            .Header($"[{CliConstants.Brand}]CDN URL[/]"));

        return 0;
    }
}
