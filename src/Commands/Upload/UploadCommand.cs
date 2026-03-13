using System.ComponentModel;
using Pixault.Cli.Infrastructure;
using Pixault.Cli.Shared;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Pixault.Cli.Commands.Upload;

public sealed class UploadSettings : ProjectSettings
{
    [CommandArgument(0, "<path>")]
    [Description("File path or glob pattern to upload")]
    public string Path { get; set; } = "";

    [CommandOption("--folder")]
    [Description("Destination folder")]
    public string? Folder { get; set; }

    [CommandOption("--alt")]
    [Description("Alt text / description")]
    public string? Alt { get; set; }

    [CommandOption("--tags")]
    [Description("Comma-separated tags")]
    public string? Tags { get; set; }
}

public sealed class UploadCommand(ClientFactory factory) : AsyncCommand<UploadSettings>
{
    private static readonly Dictionary<string, string> MimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg", [".jpeg"] = "image/jpeg",
        [".png"] = "image/png", [".gif"] = "image/gif",
        [".webp"] = "image/webp", [".svg"] = "image/svg+xml",
        [".avif"] = "image/avif", [".bmp"] = "image/bmp",
        [".tiff"] = "image/tiff", [".tif"] = "image/tiff",
        [".eps"] = "application/postscript",
        [".mp4"] = "video/mp4", [".webm"] = "video/webm",
        [".mov"] = "video/quicktime"
    };

    public override async Task<int> ExecuteAsync(CommandContext context, UploadSettings settings)
    {
        OutputHelpers.WriteHeader("upload");

        var project = factory.ResolveProject(settings.Project);
        var client = factory.CreateUploadClient(settings.Project);

        // Resolve files from path (supports glob)
        var files = ResolveFiles(settings.Path);

        if (files.Count == 0)
        {
            OutputHelpers.WriteError($"No files found matching: {settings.Path}");
            return 1;
        }

        AnsiConsole.MarkupLine($"[{CliConstants.Muted}]Uploading {files.Count} file(s) to project [bold]{project.EscapeMarkup()}[/][/]");
        AnsiConsole.WriteLine();

        var results = new List<(string File, string? ImageId, string? Url, string? Error)>();

        await AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new TransferSpeedColumn(),
                new RemainingTimeColumn())
            .StartAsync(async ctx =>
            {
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    var task = ctx.AddTask(fileName, maxValue: new FileInfo(file).Length);

                    try
                    {
                        var ext = Path.GetExtension(file);
                        var contentType = MimeTypes.GetValueOrDefault(ext, "application/octet-stream");

                        await using var stream = File.OpenRead(file);
                        var trackingStream = new ProgressStream(stream, bytes => task.Increment(bytes));

                        var result = await client.UploadAsync(
                            project, fileName, trackingStream, contentType,
                            folder: settings.Folder);

                        task.Value = task.MaxValue;
                        results.Add((fileName, result.ImageId, result.Url, null));
                    }
                    catch (Exception ex)
                    {
                        task.Value = task.MaxValue;
                        results.Add((fileName, null, null, ex.Message));
                    }
                }
            });

        AnsiConsole.WriteLine();

        // Show results
        var succeeded = results.Where(r => r.Error is null).ToList();
        var failed = results.Where(r => r.Error is not null).ToList();

        if (succeeded.Count > 0)
        {
            var rows = succeeded.Select(r =>
                new Markup($"[bold]{r.File.EscapeMarkup()}[/]  [{CliConstants.Muted}]{r.ImageId}[/]  [link={r.Url}]{r.Url}[/]"));

            AnsiConsole.Write(new Panel(new Rows(rows))
                .Header($"[{CliConstants.Success}]{succeeded.Count} uploaded[/]")
                .BorderColor(Color.Green)
                .RoundedBorder());
        }

        if (failed.Count > 0)
        {
            var rows = failed.Select(r =>
                new Markup($"[bold]{r.File.EscapeMarkup()}[/]  [{CliConstants.Error}]{r.Error!.EscapeMarkup()}[/]"));

            AnsiConsole.Write(new Panel(new Rows(rows))
                .Header($"[{CliConstants.Error}]{failed.Count} failed[/]")
                .BorderColor(Color.Red)
                .RoundedBorder());
        }

        // If alt/tags provided, update metadata for succeeded uploads
        if (succeeded.Count > 0 && (settings.Alt is not null || settings.Tags is not null))
        {
            var admin = factory.CreateAdminClient(settings.Project);
            var update = new Pixault.Client.MetadataUpdate
            {
                Description = settings.Alt,
                Keywords = settings.Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            };

            foreach (var s in succeeded)
            {
                if (s.ImageId is not null)
                    await admin.UpdateMetadataAsync(s.ImageId, update, project);
            }

            AnsiConsole.MarkupLine($"[{CliConstants.Muted}]Metadata updated for {succeeded.Count} image(s)[/]");
        }

        return failed.Count > 0 ? 1 : 0;
    }

    private static List<string> ResolveFiles(string path)
    {
        // If it contains wildcards, treat as glob
        if (path.Contains('*') || path.Contains('?'))
        {
            var dir = Path.GetDirectoryName(path) ?? ".";
            var pattern = Path.GetFileName(path);
            return Directory.Exists(dir)
                ? [.. Directory.GetFiles(dir, pattern)]
                : [];
        }

        // Single file
        if (File.Exists(path))
            return [path];

        // Directory — upload all supported files
        if (Directory.Exists(path))
            return [.. Directory.GetFiles(path)
                .Where(f => MimeTypes.ContainsKey(Path.GetExtension(f)))];

        return [];
    }
}

internal sealed class ProgressStream(Stream inner, Action<long> onRead) : Stream
{
    public override bool CanRead => inner.CanRead;
    public override bool CanSeek => inner.CanSeek;
    public override bool CanWrite => false;
    public override long Length => inner.Length;
    public override long Position { get => inner.Position; set => inner.Position = value; }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var bytesRead = inner.Read(buffer, offset, count);
        if (bytesRead > 0) onRead(bytesRead);
        return bytesRead;
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct)
    {
        var bytesRead = await inner.ReadAsync(buffer, offset, count, ct);
        if (bytesRead > 0) onRead(bytesRead);
        return bytesRead;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default)
    {
        var bytesRead = await inner.ReadAsync(buffer, ct);
        if (bytesRead > 0) onRead(bytesRead);
        return bytesRead;
    }

    public override void Flush() => inner.Flush();
    public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);
    public override void SetLength(long value) => inner.SetLength(value);
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
