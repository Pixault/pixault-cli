using System.ComponentModel;
using Pixault.Client;
using Pixault.Cli.Infrastructure;
using Pixault.Cli.Shared;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Pixault.Cli.Commands.Images;

public sealed class ImagesUpdateSettings : ProjectSettings
{
    [CommandArgument(0, "<id>")]
    [Description("Image ID")]
    public string ImageId { get; set; } = "";

    [CommandOption("--name")]
    [Description("Image name")]
    public string? Name { get; set; }

    [CommandOption("--description")]
    [Description("Image description")]
    public string? Description { get; set; }

    [CommandOption("--caption")]
    [Description("Image caption")]
    public string? Caption { get; set; }

    [CommandOption("--category")]
    [Description("Image category")]
    public string? Category { get; set; }

    [CommandOption("--folder")]
    [Description("Image folder")]
    public string? Folder { get; set; }

    [CommandOption("--keywords")]
    [Description("Comma-separated keywords")]
    public string? Keywords { get; set; }

    [CommandOption("--author")]
    [Description("Image author")]
    public string? Author { get; set; }
}

public sealed class ImagesUpdateCommand(ClientFactory factory) : AsyncCommand<ImagesUpdateSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ImagesUpdateSettings settings)
    {
        OutputHelpers.WriteHeader("images update");

        var update = new MetadataUpdate
        {
            Name = settings.Name,
            Description = settings.Description,
            Caption = settings.Caption,
            Category = settings.Category,
            Folder = settings.Folder,
            Keywords = settings.Keywords?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
            Author = settings.Author
        };

        var client = factory.CreateAdminClient(settings.Project);
        var result = await client.UpdateMetadataAsync(settings.ImageId, update, settings.Project);

        if (result is null)
        {
            OutputHelpers.WriteError($"Image not found: {settings.ImageId}");
            return 1;
        }

        OutputHelpers.WriteSuccess($"Updated {settings.ImageId}");
        return 0;
    }
}
