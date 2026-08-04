using System.ComponentModel;
using Pixault.Cli.Infrastructure;
using Pixault.Cli.Shared;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Pixault.Cli.Commands.Images;

public sealed class ImagesDeleteSettings : ProjectSettings
{
    [CommandArgument(0, "[id]")]
    [Description("Image ID to delete (omit when selecting by --ids/--files or a filter)")]
    public string? ImageId { get; set; }

    // ── Bulk selection (combine as needed) ──
    [CommandOption("--ids")]
    [Description("Comma-separated image IDs to delete")]
    public string? Ids { get; set; }

    [CommandOption("--files")]
    [Description("Comma-separated original filenames to delete")]
    public string? Files { get; set; }

    [CommandOption("--all")]
    [Description("Delete ALL images in the project (across every folder; ignores other selectors)")]
    public bool All { get; set; }

    [CommandOption("-s|--search")]
    [Description("Delete images matching this text search (name/filename/id)")]
    public string? Search { get; set; }

    [CommandOption("-c|--category")]
    [Description("Delete images in this category")]
    public string? Category { get; set; }

    [CommandOption("-k|--keyword")]
    [Description("Delete images with this keyword")]
    public string? Keyword { get; set; }

    [CommandOption("--folder")]
    [Description("Delete every image in this folder")]
    public string? Folder { get; set; }

    [CommandOption("--video")]
    [Description("Limit to videos (true) or images (false)")]
    public bool? Video { get; set; }

    [CommandOption("--max")]
    [Description("Safety cap: refuse if more than this many images match (default 500)")]
    public int Max { get; set; } = 500;

    [CommandOption("--dry-run")]
    [Description("Preview what would be deleted without deleting anything")]
    public bool DryRun { get; set; }

    [CommandOption("-y|--yes|--force")]
    [Description("Skip the confirmation prompt")]
    public bool Force { get; set; }
}

public sealed class ImagesDeleteCommand(ClientFactory factory) : AsyncCommand<ImagesDeleteSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, ImagesDeleteSettings settings, CancellationToken cancellationToken)
    {
        OutputHelpers.WriteHeader("images delete");
        var client = factory.CreateAdminClient(settings.Project);

        // 1. Resolve target images from explicit IDs, filenames, and/or a filter scan.
        var targets = new Dictionary<string, string>(StringComparer.Ordinal); // imageId -> filename

        if (!string.IsNullOrWhiteSpace(settings.ImageId))
            targets[settings.ImageId] = "";
        if (!string.IsNullOrWhiteSpace(settings.Ids))
            foreach (var id in Split(settings.Ids)) targets[id] = "";

        var wantFiles = string.IsNullOrWhiteSpace(settings.Files)
            ? null
            : Split(settings.Files).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var useScan = settings.All
                   || wantFiles is not null
                   || !string.IsNullOrWhiteSpace(settings.Folder)
                   || !string.IsNullOrWhiteSpace(settings.Search)
                   || !string.IsNullOrWhiteSpace(settings.Category)
                   || !string.IsNullOrWhiteSpace(settings.Keyword)
                   || settings.Video.HasValue;

        var truncated = false;
        if (useScan)
        {
            // --all wipes the whole project: no filters (folder=null spans every folder) and no
            // filename narrowing. Otherwise apply the given selectors.
            var scanSearch = settings.All ? null : settings.Search;
            var scanCategory = settings.All ? null : settings.Category;
            var scanKeyword = settings.All ? null : settings.Keyword;
            var scanFolder = settings.All ? null : settings.Folder;
            var scanVideo = settings.All ? (bool?)null : settings.Video;
            var filterFiles = settings.All ? null : wantFiles;

            string? cursor = null;
            var pages = 0;
            do
            {
                var page = await client.ListImagesAsync(50, cursor, project: settings.Project,
                    search: scanSearch, category: scanCategory, keyword: scanKeyword,
                    folder: scanFolder, isVideo: scanVideo);
                foreach (var i in page.Images)
                {
                    if (filterFiles is not null && !filterFiles.Contains(i.OriginalFileName)) continue;
                    targets[i.ImageId] = i.OriginalFileName;
                }
                cursor = page.NextCursor;
            } while (cursor is not null && ++pages < 60);
            truncated = cursor is not null; // hit the ~3000-image scan cap with more remaining
        }

        // 2. Guards.
        if (targets.Count == 0)
        {
            AnsiConsole.MarkupLine($"[{CliConstants.Muted}]No matching images. Provide an image ID, or --ids/--files, or a filter (--search/--folder/--category/…).[/]");
            return 0;
        }
        if (targets.Count > settings.Max)
        {
            AnsiConsole.MarkupLine($"[red]Refusing to delete {targets.Count} images — exceeds the safety cap of {settings.Max}. Narrow the selection or raise --max.[/]");
            return 1;
        }

        // 3. Show the selection.
        AnsiConsole.MarkupLine($"[bold]{targets.Count}[/] image(s) selected:");
        foreach (var t in targets.Take(20))
            AnsiConsole.MarkupLine($"  [{CliConstants.Muted}]{t.Key.EscapeMarkup()}{(t.Value.Length > 0 ? $" ({t.Value.EscapeMarkup()})" : "")}[/]");
        if (targets.Count > 20)
            AnsiConsole.MarkupLine($"  [{CliConstants.Muted}]… and {targets.Count - 20} more[/]");
        if (truncated)
            AnsiConsole.MarkupLine($"[yellow]Note: scan hit ~3000 images; more may match.[/]");

        if (settings.DryRun)
        {
            AnsiConsole.MarkupLine("[yellow]Dry run — nothing deleted.[/]");
            return 0;
        }

        // 4. Confirm (unless --yes/--force).
        if (!settings.Force &&
            !AnsiConsole.Confirm($"[red]Permanently delete {targets.Count} image(s)?[/]", defaultValue: false))
        {
            AnsiConsole.MarkupLine($"[{CliConstants.Muted}]Cancelled.[/]");
            return 0;
        }

        // 5. Delete.
        var ok = 0;
        var errors = new List<string>();
        foreach (var (id, _) in targets)
        {
            try { await client.DeleteImageAsync(id, settings.Project); ok++; }
            catch (Exception ex) { errors.Add($"{id}: {ex.Message}"); }
        }

        OutputHelpers.WriteSuccess($"Deleted {ok}/{targets.Count} image(s).");
        if (errors.Count > 0)
        {
            AnsiConsole.MarkupLine($"[red]{errors.Count} error(s):[/]");
            foreach (var e in errors.Take(10))
                AnsiConsole.MarkupLine($"  [red]{e.EscapeMarkup()}[/]");
        }
        return errors.Count > 0 ? 1 : 0;
    }

    private static IEnumerable<string> Split(string s) =>
        s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
