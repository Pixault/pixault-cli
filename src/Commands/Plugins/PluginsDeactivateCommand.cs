using System.ComponentModel;
using Pixault.Cli.Infrastructure;
using Pixault.Cli.Shared;
using Spectre.Console.Cli;

namespace Pixault.Cli.Commands.Plugins;

public sealed class PluginsDeactivateSettings : ProjectSettings
{
    [CommandArgument(0, "<name>")]
    [Description("Plugin name to deactivate")]
    public string Name { get; set; } = "";
}

public sealed class PluginsDeactivateCommand(ClientFactory factory) : AsyncCommand<PluginsDeactivateSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, PluginsDeactivateSettings settings)
    {
        OutputHelpers.WriteHeader("plugins deactivate");

        var project = factory.ResolveProject(settings.Project);
        var client = factory.CreateAdminClient(settings.Project);
        await client.DeactivatePluginAsync(project, settings.Name);

        OutputHelpers.WriteSuccess($"Deactivated plugin: {settings.Name}");
        return 0;
    }
}
