using System.ComponentModel;
using Pixault.Cli.Infrastructure;
using Pixault.Cli.Shared;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Pixault.Cli.Commands.Images;

public sealed class ImagesGetSettings : ProjectSettings
{
    [CommandArgument(0, "<id>")]
    [Description("Image ID")]
    public string ImageId { get; set; } = "";
}

public sealed class ImagesGetCommand(ClientFactory factory) : AsyncCommand<ImagesGetSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ImagesGetSettings settings)
    {
        OutputHelpers.WriteHeader("images get");

        var client = factory.CreateAdminClient(settings.Project);
        var img = await client.GetMetadataAsync(settings.ImageId, settings.Project);

        if (img is null)
        {
            OutputHelpers.WriteError($"Image not found: {settings.ImageId}");
            return 1;
        }

        var grid = new Grid().AddColumn().AddColumn();

        void Row(string label, string? value) =>
            grid.AddRow(new Markup($"[bold]{label.EscapeMarkup()}[/]"), new Markup(value ?? $"[{CliConstants.Muted}]—[/]"));

        Row("Image ID", $"[{CliConstants.BrandAccent}]{img.ImageId.EscapeMarkup()}[/]");
        Row("Project", img.ProjectId);
        Row("File Name", img.OriginalFileName);
        Row("Name", img.Name);
        Row("Description", img.Description);
        Row("Caption", img.Caption);
        Row("Content Type", img.ContentType);
        Row("Size", img.FormattedSize);
        Row("Dimensions", $"{img.Width} x {img.Height}");
        Row("Category", img.Category);
        Row("Folder", img.Folder);
        Row("Author", img.Author);
        Row("Copyright", img.CopyrightHolder is not null
            ? $"{img.CopyrightHolder} ({img.CopyrightYear})"
            : null);
        Row("License", img.License);
        Row("Uploaded", OutputHelpers.FormatDate(img.UploadedAt));
        Row("Created", OutputHelpers.FormatDate(img.DateCreated));
        Row("Published", OutputHelpers.FormatDate(img.DatePublished));

        if (img.IsVideo)
        {
            Row("Duration", img.FormattedDuration);
            Row("Has Audio", img.HasAudio?.ToString() ?? "—");
        }

        if (img.IsEps)
        {
            Row("Source Asset", img.SourceAssetId);
            Row("Derivation", img.DerivationType);
        }

        if (img.LocationName is not null || img.LocationLatitude.HasValue)
        {
            Row("Location", img.LocationName);
            if (img.LocationLatitude.HasValue)
                Row("Coordinates", $"{img.LocationLatitude}, {img.LocationLongitude}");
        }

        AnsiConsole.Write(new Panel(grid)
            .Header($"[{CliConstants.Brand}]{img.ImageId.EscapeMarkup()}[/]")
            .RoundedBorder()
            .BorderColor(Color.DodgerBlue1));

        // Keywords
        if (img.Keywords is { Count: > 0 })
        {
            var keywords = string.Join("  ", img.Keywords.Select(k => $"[on grey23] {k.EscapeMarkup()} [/]"));
            AnsiConsole.MarkupLine($"\n  Keywords: {keywords}");
        }

        // CDN URL
        var cdnUrl = factory.CreateUrlBuilder(img.ImageId, settings.Project).Build();
        AnsiConsole.MarkupLine($"\n  [link={cdnUrl}]{cdnUrl}[/]");

        return 0;
    }
}
