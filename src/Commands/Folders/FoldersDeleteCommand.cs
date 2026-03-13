using System.ComponentModel;
using Pixault.Cli.Infrastructure;
using Pixault.Cli.Shared;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Pixault.Cli.Commands.Folders;

public sealed class FoldersDeleteSettings : ProjectSettings
{
    [CommandArgument(0, "<path>")]
    [Description("Folder path to delete")]
    public string Path { get; set; } = "";

    [CommandOption("--force")]
    [Description("Skip confirmation prompt")]
    public bool Force { get; set; }
}

public sealed class FoldersDeleteCommand(ClientFactory factory) : AsyncCommand<FoldersDeleteSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, FoldersDeleteSettings settings)
    {
        OutputHelpers.WriteHeader("folders delete");

        if (!settings.Force)
        {
            OutputHelpers.WriteWarning($"This will delete folder: {settings.Path}");
            if (!AnsiConsole.Confirm("Are you sure?", defaultValue: false))
            {
                AnsiConsole.MarkupLine($"[{CliConstants.Muted}]Cancelled.[/]");
                return 0;
            }
        }

        var client = factory.CreateAdminClient(settings.Project);
        await client.DeleteFolderAsync(settings.Path, settings.Project);

        OutputHelpers.WriteSuccess($"Deleted folder: {settings.Path}");
        return 0;
    }
}
