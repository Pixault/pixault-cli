using System.ComponentModel;
using Pixault.Cli.Infrastructure;
using Pixault.Cli.Shared;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Pixault.Cli.Commands.Images;

public sealed class ImagesDerivedSettings : ProjectSettings
{
    [CommandArgument(0, "<id>")]
    [Description("Image ID")]
    public string ImageId { get; set; } = "";
}

public sealed class ImagesDerivedCommand(ClientFactory factory) : AsyncCommand<ImagesDerivedSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ImagesDerivedSettings settings)
    {
        OutputHelpers.WriteHeader("images derived");

        var client = factory.CreateAdminClient(settings.Project);
        var derived = await client.GetDerivedAssetsAsync(settings.ImageId, settings.Project);

        if (derived.Count == 0)
        {
            AnsiConsole.MarkupLine($"[{CliConstants.Muted}]No derived assets found for {settings.ImageId.EscapeMarkup()}[/]");
            return 0;
        }

        var table = new Table()
            .RoundedBorder()
            .BorderColor(Color.Grey)
            .AddColumn("Derived ID")
            .AddColumn("Type")
            .AddColumn("Dimensions")
            .AddColumn("Size")
            .AddColumn("Content Type")
            .AddColumn("Created");

        foreach (var d in derived)
        {
            table.AddRow(
                new Markup($"[{CliConstants.BrandAccent}]{d.ImageId.EscapeMarkup()}[/]"),
                new Text(d.DerivationType ?? "—"),
                new Text($"{d.Width}x{d.Height}"),
                new Text(OutputHelpers.FormatBytes(d.SizeBytes)),
                new Text(d.ContentType),
                new Markup(OutputHelpers.FormatDate(d.UploadedAt)));
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[{CliConstants.Muted}]{derived.Count} derived asset(s)[/]");

        return 0;
    }
}
