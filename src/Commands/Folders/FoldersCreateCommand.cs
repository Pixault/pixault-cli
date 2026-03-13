using System.ComponentModel;
using Pixault.Cli.Infrastructure;
using Pixault.Cli.Shared;
using Spectre.Console.Cli;

namespace Pixault.Cli.Commands.Folders;

public sealed class FoldersCreateSettings : ProjectSettings
{
    [CommandArgument(0, "<path>")]
    [Description("Folder path to create")]
    public string Path { get; set; } = "";
}

public sealed class FoldersCreateCommand(ClientFactory factory) : AsyncCommand<FoldersCreateSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, FoldersCreateSettings settings)
    {
        OutputHelpers.WriteHeader("folders create");

        var client = factory.CreateAdminClient(settings.Project);
        await client.CreateFolderAsync(settings.Path, settings.Project);

        OutputHelpers.WriteSuccess($"Created folder: {settings.Path}");
        return 0;
    }
}
