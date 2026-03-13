using System.ComponentModel;
using Pixault.Cli.Infrastructure;
using Pixault.Cli.Shared;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Pixault.Cli.Commands.Images;

public sealed class ImagesDeleteSettings : ProjectSettings
{
    [CommandArgument(0, "<id>")]
    [Description("Image ID")]
    public string ImageId { get; set; } = "";

    [CommandOption("--force")]
    [Description("Skip confirmation prompt")]
    public bool Force { get; set; }
}

public sealed class ImagesDeleteCommand(ClientFactory factory) : AsyncCommand<ImagesDeleteSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ImagesDeleteSettings settings)
    {
        OutputHelpers.WriteHeader("images delete");

        if (!settings.Force)
        {
            AnsiConsole.Write(new Panel(
                new Markup($"[bold red]This will permanently delete image {settings.ImageId.EscapeMarkup()}[/]"))
                .BorderColor(Color.Red)
                .RoundedBorder()
                .Header("[red]Warning[/]"));

            if (!AnsiConsole.Confirm("Are you sure?", defaultValue: false))
            {
                AnsiConsole.MarkupLine($"[{CliConstants.Muted}]Cancelled.[/]");
                return 0;
            }
        }

        var client = factory.CreateAdminClient(settings.Project);
        await client.DeleteImageAsync(settings.ImageId, settings.Project);

        OutputHelpers.WriteSuccess($"Deleted {settings.ImageId}");
        return 0;
    }
}
