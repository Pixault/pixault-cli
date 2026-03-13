using System.ComponentModel;
using Pixault.Cli.Infrastructure;
using Pixault.Cli.Shared;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Pixault.Cli.Commands.Transforms;

public sealed class TransformsGetSettings : ProjectSettings
{
    [CommandArgument(0, "<name>")]
    [Description("Transform name")]
    public string Name { get; set; } = "";
}

public sealed class TransformsGetCommand(ClientFactory factory) : AsyncCommand<TransformsGetSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, TransformsGetSettings settings)
    {
        OutputHelpers.WriteHeader("transforms get");

        var client = factory.CreateAdminClient(settings.Project);
        var t = await client.GetTransformAsync(settings.Name, settings.Project);

        if (t is null)
        {
            OutputHelpers.WriteError($"Transform not found: {settings.Name}");
            return 1;
        }

        var grid = new Grid().AddColumn().AddColumn();

        void Row(string label, string? value) =>
            grid.AddRow(new Markup($"[bold]{label.EscapeMarkup()}[/]"), new Markup(value ?? $"[{CliConstants.Muted}]—[/]"));

        Row("Name", $"[{CliConstants.BrandAccent}]{t.Name.EscapeMarkup()}[/]");
        Row("Project", t.ProjectId);
        Row("Width", t.Width?.ToString());
        Row("Height", t.Height?.ToString());
        Row("Quality", t.Quality?.ToString());
        Row("Fit Mode", t.FitMode);
        Row("Blur", t.Blur?.ToString());
        Row("Watermark", t.WatermarkId);
        Row("Watermark Position", t.WatermarkPosition);
        Row("Watermark Opacity", t.WatermarkOpacity?.ToString());

        if (t.LockedParameters.Count > 0)
            Row("Locked", $"[yellow]{string.Join(", ", t.LockedParameters).EscapeMarkup()}[/]");
        else
            Row("Locked", $"[{CliConstants.Muted}]none[/]");

        AnsiConsole.Write(new Panel(grid)
            .Header($"[{CliConstants.Brand}]{t.Name.EscapeMarkup()}[/]")
            .RoundedBorder()
            .BorderColor(Color.DodgerBlue1));

        return 0;
    }
}
