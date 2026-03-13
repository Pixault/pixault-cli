using Pixault.Cli.Infrastructure;
using Pixault.Cli.Shared;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Pixault.Cli.Commands.Config;

public sealed class ConfigInitCommand(ConfigService configService) : AsyncCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        AnsiConsole.Write(new FigletText("pixault").Color(Color.DodgerBlue1));
        AnsiConsole.WriteLine();

        var existing = configService.Config;

        var baseUrl = AnsiConsole.Prompt(
            new TextPrompt<string>("API base URL:")
                .DefaultValue(existing.BaseUrl));

        var cdnUrl = AnsiConsole.Prompt(
            new TextPrompt<string>("CDN URL (leave blank to use base URL):")
                .AllowEmpty()
                .DefaultValue(existing.CdnUrl ?? ""));

        var project = AnsiConsole.Prompt(
            new TextPrompt<string>("Default project ID:")
                .DefaultValue(existing.DefaultProject ?? ""));

        var apiKey = AnsiConsole.Prompt(
            new TextPrompt<string>("API key:")
                .Secret()
                .DefaultValue(existing.ApiKey ?? ""));

        var hmacSecret = AnsiConsole.Prompt(
            new TextPrompt<string>("HMAC secret (optional):")
                .Secret()
                .AllowEmpty()
                .DefaultValue(existing.HmacSecret ?? ""));

        var config = new CliConfig
        {
            BaseUrl = baseUrl,
            CdnUrl = string.IsNullOrWhiteSpace(cdnUrl) ? null : cdnUrl,
            DefaultProject = string.IsNullOrWhiteSpace(project) ? null : project,
            ApiKey = string.IsNullOrWhiteSpace(apiKey) ? null : apiKey,
            HmacSecret = string.IsNullOrWhiteSpace(hmacSecret) ? null : hmacSecret
        };

        // Test connection
        if (config.ApiKey is not null && config.DefaultProject is not null)
        {
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync("Testing connection...", async _ =>
                {
                    try
                    {
                        var factory = new ClientFactory(new ConfigService());
                        // Save first so the factory picks it up
                        configService.Save(config);
                        var client = factory.CreateAdminClient(config.DefaultProject);
                        await client.ListImagesAsync(limit: 1, project: config.DefaultProject);
                        AnsiConsole.MarkupLine($"[{CliConstants.Success}]Connection successful![/]");
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[{CliConstants.Warning}]Connection test failed: {ex.Message.EscapeMarkup()}[/]");
                        AnsiConsole.MarkupLine($"[{CliConstants.Muted}]Config saved anyway — check your settings.[/]");
                    }
                });
        }
        else
        {
            configService.Save(config);
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Panel(
            new Rows(
                new Markup($"[bold]Base URL:[/]  {config.BaseUrl.EscapeMarkup()}"),
                new Markup($"[bold]CDN URL:[/]   {(config.CdnUrl ?? "[dim]same as base[/]")}"),
                new Markup($"[bold]Project:[/]   {config.DefaultProject ?? "[dim]not set[/]"}"),
                new Markup($"[bold]API Key:[/]   {OutputHelpers.MaskSecret(config.ApiKey)}"),
                new Markup($"[bold]HMAC:[/]      {OutputHelpers.MaskSecret(config.HmacSecret)}")
            ))
            .Header($"[{CliConstants.Success}]Config saved to {CliConfig.ConfigPath.EscapeMarkup()}[/]")
            .BorderColor(Color.Green)
            .RoundedBorder());

        return 0;
    }
}
