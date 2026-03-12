using System.ComponentModel;
using Pixault.Cli.Infrastructure;
using Pixault.Cli.Shared;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Pixault.Cli.Commands.Eps;

public sealed class EpsExtractSvgSettings : ProjectSettings
{
    [CommandArgument(0, "<id>")]
    [Description("EPS image ID to extract SVG from")]
    public string ImageId { get; set; } = "";
}

public sealed class EpsExtractSvgCommand(ClientFactory factory) : AsyncCommand<EpsExtractSvgSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, EpsExtractSvgSettings settings)
    {
        OutputHelpers.WriteHeader("eps extract-svg");

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Triggering SVG extraction...", async _ =>
            {
                var client = factory.CreateAdminClient(settings.Project);
                await client.ExtractEpsSvgAsync(settings.ImageId, settings.Project);
            });

        OutputHelpers.WriteSuccess($"SVG extraction triggered for {settings.ImageId}");
        AnsiConsole.MarkupLine($"[{CliConstants.Muted}]Use 'pixault eps status {settings.ImageId.EscapeMarkup()}' to check progress.[/]");

        return 0;
    }
}
