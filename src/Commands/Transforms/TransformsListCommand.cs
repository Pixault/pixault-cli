using Pixault.Cli.Infrastructure;
using Pixault.Cli.Shared;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Pixault.Cli.Commands.Transforms;

public sealed class TransformsListCommand(ClientFactory factory) : AsyncCommand<ProjectSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ProjectSettings settings)
    {
        OutputHelpers.WriteHeader("transforms list");

        var client = factory.CreateAdminClient(settings.Project);
        var transforms = await client.ListTransformsAsync(settings.Project);

        if (transforms.Count == 0)
        {
            AnsiConsole.MarkupLine($"[{CliConstants.Muted}]No transforms found.[/]");
            return 0;
        }

        var table = new Table()
            .RoundedBorder()
            .BorderColor(Color.Grey)
            .AddColumn("Name")
            .AddColumn("Width")
            .AddColumn("Height")
            .AddColumn("Quality")
            .AddColumn("Fit")
            .AddColumn("Blur")
            .AddColumn("Locked");

        foreach (var t in transforms)
        {
            var locked = t.LockedParameters.Count > 0
                ? $"[yellow]{string.Join(", ", t.LockedParameters)}[/]"
                : $"[{CliConstants.Muted}]none[/]";

            table.AddRow(
                new Markup($"[bold {CliConstants.BrandAccent}]{t.Name.EscapeMarkup()}[/]"),
                new Text(t.Width?.ToString() ?? "—"),
                new Text(t.Height?.ToString() ?? "—"),
                new Text(t.Quality?.ToString() ?? "—"),
                new Text(t.FitMode ?? "—"),
                new Text(t.Blur?.ToString() ?? "—"),
                new Markup(locked));
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"\n[{CliConstants.Muted}]{transforms.Count} transform(s)[/]");

        return 0;
    }
}
