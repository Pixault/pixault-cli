using System.ComponentModel;
using Pixault.Cli.Infrastructure;
using Pixault.Cli.Shared;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Pixault.Cli.Commands.Eps;

public sealed class EpsStatusSettings : ProjectSettings
{
    [CommandArgument(0, "<id>")]
    [Description("Image ID to check processing status")]
    public string ImageId { get; set; } = "";
}

public sealed class EpsStatusCommand(ClientFactory factory) : AsyncCommand<EpsStatusSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, EpsStatusSettings settings)
    {
        OutputHelpers.WriteHeader("eps status");

        var client = factory.CreateAdminClient(settings.Project);
        var status = await client.GetEpsProcessingStatusAsync(settings.ImageId, settings.Project);

        if (status is null)
        {
            OutputHelpers.WriteError($"No processing status found for: {settings.ImageId}");
            return 1;
        }

        var statusColor = status.Status.ToLowerInvariant() switch
        {
            "completed" or "succeeded" => CliConstants.Success,
            "processing" or "pending" => CliConstants.Warning,
            _ => CliConstants.Error
        };

        var grid = new Grid().AddColumn().AddColumn();

        grid.AddRow(new Markup("[bold]Job ID[/]"), new Text(status.Id.ToString()));
        grid.AddRow(new Markup("[bold]Source[/]"), new Text(status.Source));
        grid.AddRow(new Markup("[bold]Status[/]"), new Markup($"[{statusColor}]{status.Status.EscapeMarkup()}[/]"));
        grid.AddRow(new Markup("[bold]Total[/]"), new Text(status.TotalAssets.ToString()));
        grid.AddRow(new Markup("[bold]Processed[/]"), new Text(status.ProcessedAssets.ToString()));
        grid.AddRow(new Markup("[bold]Succeeded[/]"), new Markup($"[{CliConstants.Success}]{status.SucceededAssets}[/]"));
        grid.AddRow(new Markup("[bold]Failed[/]"), new Markup(status.FailedAssets > 0
            ? $"[{CliConstants.Error}]{status.FailedAssets}[/]"
            : "0"));
        grid.AddRow(new Markup("[bold]Created[/]"), new Text(status.CreatedAt.ToString("yyyy-MM-dd HH:mm")));

        if (status.StartedAt.HasValue)
            grid.AddRow(new Markup("[bold]Started[/]"), new Text(status.StartedAt.Value.ToString("yyyy-MM-dd HH:mm")));
        if (status.CompletedAt.HasValue)
            grid.AddRow(new Markup("[bold]Completed[/]"), new Text(status.CompletedAt.Value.ToString("yyyy-MM-dd HH:mm")));

        AnsiConsole.Write(new Panel(grid)
            .Header($"[{CliConstants.Brand}]EPS Processing[/]")
            .RoundedBorder()
            .BorderColor(Color.DodgerBlue1));

        // Progress bar
        if (status.TotalAssets > 0)
        {
            AnsiConsole.WriteLine();
            var pct = (double)status.ProcessedAssets / status.TotalAssets;
            var bar = new BreakdownChart()
                .Width(60)
                .AddItem("Succeeded", status.SucceededAssets, Color.Green)
                .AddItem("Failed", status.FailedAssets, Color.Red)
                .AddItem("Remaining", status.TotalAssets - status.ProcessedAssets, Color.Grey);
            AnsiConsole.Write(bar);
        }

        return 0;
    }
}
