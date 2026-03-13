using Pixault.Cli.Infrastructure;
using Pixault.Cli.Shared;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Pixault.Cli.Commands.Plugins;

public sealed class PluginsInstalledCommand(ClientFactory factory) : AsyncCommand<ProjectSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ProjectSettings settings)
    {
        OutputHelpers.WriteHeader("plugins installed");

        var client = factory.CreateAdminClient(settings.Project);
        var plugins = await client.GetProjectPluginsAsync(settings.Project);

        if (plugins.Count == 0)
        {
            AnsiConsole.MarkupLine($"[{CliConstants.Muted}]No plugins installed.[/]");
            return 0;
        }

        var table = new Table()
            .RoundedBorder()
            .BorderColor(Color.Grey)
            .AddColumn("Name")
            .AddColumn("Display Name")
            .AddColumn("Category")
            .AddColumn("Status");

        foreach (var p in plugins)
        {
            var status = p.IsActivated
                ? $"[{CliConstants.Success}]active[/]"
                : $"[{CliConstants.Muted}]inactive[/]";

            table.AddRow(
                new Markup($"[bold {CliConstants.BrandAccent}]{p.Name.EscapeMarkup()}[/]"),
                new Text(p.DisplayName),
                new Text(p.Category),
                new Markup(status));
        }

        AnsiConsole.Write(table);

        return 0;
    }
}
