using System.ComponentModel;
using Pixault.Cli.Infrastructure;
using Pixault.Cli.Shared;
using Spectre.Console.Cli;

namespace Pixault.Cli.Commands.Plugins;

public sealed class PluginsActivateSettings : ProjectSettings
{
    [CommandArgument(0, "<name>")]
    [Description("Plugin name to activate")]
    public string Name { get; set; } = "";
}

public sealed class PluginsActivateCommand(ClientFactory factory) : AsyncCommand<PluginsActivateSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, PluginsActivateSettings settings)
    {
        OutputHelpers.WriteHeader("plugins activate");

        var project = factory.ResolveProject(settings.Project);
        var client = factory.CreateAdminClient(settings.Project);
        await client.ActivatePluginAsync(project, settings.Name);

        OutputHelpers.WriteSuccess($"Activated plugin: {settings.Name}");
        return 0;
    }
}
