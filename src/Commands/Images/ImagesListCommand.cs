using System.ComponentModel;
using Pixault.Cli.Infrastructure;
using Pixault.Cli.Shared;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Pixault.Cli.Commands.Images;

public sealed class ImagesListSettings : ProjectSettings
{
    [CommandOption("-l|--limit")]
    [Description("Number of images to return")]
    [DefaultValue(20)]
    public int Limit { get; set; } = 20;

    [CommandOption("-s|--search")]
    [Description("Search query")]
    public string? Search { get; set; }

    [CommandOption("-c|--category")]
    [Description("Filter by category")]
    public string? Category { get; set; }

    [CommandOption("-k|--keyword")]
    [Description("Filter by keyword")]
    public string? Keyword { get; set; }

    [CommandOption("--author")]
    [Description("Filter by author")]
    public string? Author { get; set; }

    [CommandOption("--folder")]
    [Description("Filter by folder")]
    public string? Folder { get; set; }

    [CommandOption("--video")]
    [Description("Show only videos")]
    public bool? Video { get; set; }

    [CommandOption("--cursor")]
    [Description("Pagination cursor")]
    public string? Cursor { get; set; }
}

public sealed class ImagesListCommand(ClientFactory factory) : AsyncCommand<ImagesListSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ImagesListSettings settings)
    {
        OutputHelpers.WriteHeader("images list");

        var client = factory.CreateAdminClient(settings.Project);
        var result = await client.ListImagesAsync(
            limit: settings.Limit,
            cursor: settings.Cursor,
            project: settings.Project,
            search: settings.Search,
            category: settings.Category,
            keyword: settings.Keyword,
            author: settings.Author,
            isVideo: settings.Video,
            folder: settings.Folder);

        if (result.Images.Count == 0)
        {
            AnsiConsole.MarkupLine($"[{CliConstants.Muted}]No images found.[/]");
            return 0;
        }

        var table = new Table()
            .RoundedBorder()
            .BorderColor(Color.Grey)
            .AddColumn("ID")
            .AddColumn("Name")
            .AddColumn("Type")
            .AddColumn("Size")
            .AddColumn("Dimensions")
            .AddColumn("Uploaded");

        var alt = false;
        foreach (var img in result.Images)
        {
            var style = alt ? new Style(foreground: Color.White) : Style.Plain;
            table.AddRow(
                new Markup($"[{CliConstants.BrandAccent}]{img.ImageId.EscapeMarkup()}[/]"),
                new Text(img.Name ?? img.OriginalFileName, style),
                new Text(img.IsVideo ? "video" : img.IsEps ? "eps" : "image", style),
                new Text(img.FormattedSize, style),
                new Text($"{img.Width}x{img.Height}", style),
                new Markup(OutputHelpers.FormatDate(img.UploadedAt)));
            alt = !alt;
        }

        AnsiConsole.Write(table);

        AnsiConsole.MarkupLine($"[{CliConstants.Muted}]Showing {result.Images.Count} of {result.TotalCount} images[/]");

        if (result.NextCursor is not null)
            AnsiConsole.MarkupLine($"[{CliConstants.Muted}]Next page: pixault images list --cursor \"{result.NextCursor.EscapeMarkup()}\"[/]");

        return 0;
    }
}
