using System.ComponentModel;
using Pixault.Cli.Infrastructure;
using Pixault.Cli.Shared;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Pixault.Cli.Commands.Transforms;

public sealed class TransformsDeleteSettings : ProjectSettings
{
    [CommandArgument(0, "<name>")]
    [Description("Transform name")]
    public string Name { get; set; } = "";

    [CommandOption("--force")]
    [Description("Skip confirmation prompt")]
    public bool Force { get; set; }
}

public sealed class TransformsDeleteCommand(ClientFactory factory) : AsyncCommand<TransformsDeleteSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, TransformsDeleteSettings settings)
    {
        OutputHelpers.WriteHeader("transforms delete");

        if (!settings.Force)
        {
            OutputHelpers.WriteWarning($"This will delete transform: {settings.Name}");
            if (!AnsiConsole.Confirm("Are you sure?", defaultValue: false))
            {
                AnsiConsole.MarkupLine($"[{CliConstants.Muted}]Cancelled.[/]");
                return 0;
            }
        }

        var client = factory.CreateAdminClient(settings.Project);
        await client.DeleteTransformAsync(settings.Name, settings.Project);

        OutputHelpers.WriteSuccess($"Deleted transform: {settings.Name}");
        return 0;
    }
}
