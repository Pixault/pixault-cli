using Pixault.Cli.Infrastructure;
using Pixault.Cli.Shared;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Pixault.Cli.Commands.Folders;

public sealed class FoldersListCommand(ClientFactory factory) : AsyncCommand<ProjectSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ProjectSettings settings)
    {
        OutputHelpers.WriteHeader("folders list");

        var client = factory.CreateAdminClient(settings.Project);
        var folders = await client.ListFoldersAsync(settings.Project);

        if (folders.Count == 0)
        {
            AnsiConsole.MarkupLine($"[{CliConstants.Muted}]No folders found.[/]");
            return 0;
        }

        var tree = new Tree($"[{CliConstants.Brand}]{factory.ResolveProject(settings.Project).EscapeMarkup()}[/]");
        var nodes = new Dictionary<string, TreeNode>();

        foreach (var folder in folders.OrderBy(f => f))
        {
            var parts = folder.Split('/');
            TreeNode? parent = null;
            var path = "";

            foreach (var part in parts)
            {
                path = path == "" ? part : $"{path}/{part}";

                if (!nodes.TryGetValue(path, out var node))
                {
                    node = parent is null
                        ? tree.AddNode($"[bold]{part.EscapeMarkup()}[/]")
                        : parent.AddNode($"[bold]{part.EscapeMarkup()}[/]");
                    nodes[path] = node;
                }

                parent = node;
            }
        }

        AnsiConsole.Write(tree);
        AnsiConsole.MarkupLine($"\n[{CliConstants.Muted}]{folders.Count} folder(s)[/]");

        return 0;
    }
}
