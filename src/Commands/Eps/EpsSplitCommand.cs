using System.ComponentModel;
using Pixault.Cli.Infrastructure;
using Pixault.Cli.Shared;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Pixault.Cli.Commands.Eps;

public sealed class EpsSplitSettings : ProjectSettings
{
    [CommandArgument(0, "<id>")]
    [Description("EPS image ID to split")]
    public string ImageId { get; set; } = "";
}

public sealed class EpsSplitCommand(ClientFactory factory) : AsyncCommand<EpsSplitSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, EpsSplitSettings settings)
    {
        OutputHelpers.WriteHeader("eps split");

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Triggering design split...", async _ =>
            {
                var client = factory.CreateAdminClient(settings.Project);
                await client.SplitEpsDesignsAsync(settings.ImageId, settings.Project);
            });

        OutputHelpers.WriteSuccess($"Design split triggered for {settings.ImageId}");
        AnsiConsole.MarkupLine($"[{CliConstants.Muted}]Use 'pixault eps status {settings.ImageId.EscapeMarkup()}' to check progress.[/]");

        return 0;
    }
}
