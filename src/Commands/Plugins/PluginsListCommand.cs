using Pixault.Cli.Infrastructure;
using Pixault.Cli.Shared;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Pixault.Cli.Commands.Plugins;

public sealed class PluginsListCommand(ClientFactory factory) : AsyncCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        OutputHelpers.WriteHeader("plugins list");

        var client = factory.CreateAdminClient();
        var plugins = await client.GetAllPluginsAsync();

        if (plugins.Count == 0)
        {
            AnsiConsole.MarkupLine($"[{CliConstants.Muted}]No plugins available.[/]");
            return 0;
        }

        var table = new Table()
            .RoundedBorder()
            .BorderColor(Color.Grey)
            .AddColumn("Name")
            .AddColumn("Display Name")
            .AddColumn("Category")
            .AddColumn("Vendor")
            .AddColumn("Stage")
            .AddColumn("Price");

        foreach (var p in plugins)
        {
            var price = p.PriceCentsPerInvocation > 0
                ? $"${p.PriceCentsPerInvocation / 100.0:F2}/call"
                : $"[{CliConstants.Success}]free[/]";

            table.AddRow(
                new Markup($"[bold {CliConstants.BrandAccent}]{p.Name.EscapeMarkup()}[/]"),
                new Text(p.DisplayName),
                new Text(p.Category),
                new Text(p.Vendor),
                new Text(p.Stage),
                new Markup(price));
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"\n[{CliConstants.Muted}]{plugins.Count} plugin(s) available[/]");

        return 0;
    }
}
