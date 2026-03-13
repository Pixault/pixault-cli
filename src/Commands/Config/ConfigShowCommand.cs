using Pixault.Cli.Infrastructure;
using Pixault.Cli.Shared;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Pixault.Cli.Commands.Config;

public sealed class ConfigShowCommand(ConfigService configService) : Command
{
    public override int Execute(CommandContext context)
    {
        OutputHelpers.WriteHeader("config show");

        var config = configService.Config;

        var table = new Table()
            .RoundedBorder()
            .BorderColor(Color.DodgerBlue1)
            .AddColumn(new TableColumn("Setting").Width(14))
            .AddColumn(new TableColumn("Value"))
            .AddColumn(new TableColumn("Source").Width(10));

        void AddRow(string name, string? value, string? envVar)
        {
            var envValue = envVar is not null ? Environment.GetEnvironmentVariable(envVar) : null;
            var source = envValue is not null ? "env" : File.Exists(CliConfig.ConfigPath) ? "config" : "default";
            var display = envValue ?? value;

            var displayValue = name.Contains("Key") || name.Contains("Secret") || name.Contains("HMAC")
                ? OutputHelpers.MaskSecret(display)
                : display ?? "[dim]not set[/]";

            table.AddRow(
                new Markup($"[bold]{name.EscapeMarkup()}[/]"),
                new Markup(displayValue),
                new Markup($"[{CliConstants.Muted}]{source}[/]"));
        }

        AddRow("Base URL", config.BaseUrl, "PIXAULT_BASE_URL");
        AddRow("CDN URL", config.CdnUrl, "PIXAULT_CDN_URL");
        AddRow("Project", config.DefaultProject, "PIXAULT_PROJECT");
        AddRow("API Key", config.ApiKey, "PIXAULT_API_KEY");
        AddRow("HMAC Secret", config.HmacSecret, "PIXAULT_HMAC_SECRET");

        AnsiConsole.Write(table);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[{CliConstants.Muted}]Config file: {CliConfig.ConfigPath.EscapeMarkup()}[/]");

        return 0;
    }
}
