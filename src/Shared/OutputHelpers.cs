using Spectre.Console;

namespace Pixault.Cli.Shared;

public static class OutputHelpers
{
    public static void WriteHeader(string command)
    {
        AnsiConsole.Markup($"[bold {CliConstants.Brand}]pixault[/] [{CliConstants.Muted}]>[/] {command}");
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();
    }

    public static void WriteSuccess(string message)
    {
        AnsiConsole.Write(new Panel(new Markup($"[{CliConstants.Success}]{message.EscapeMarkup()}[/]"))
            .BorderColor(Color.Green)
            .RoundedBorder());
    }

    public static void WriteError(string message)
    {
        AnsiConsole.Write(new Panel(new Markup($"[{CliConstants.Error}]{message.EscapeMarkup()}[/]"))
            .BorderColor(Color.Red)
            .RoundedBorder()
            .Header("[red]Error[/]"));
    }

    public static void WriteWarning(string message)
    {
        AnsiConsole.Write(new Panel(new Markup($"[{CliConstants.Warning}]{message.EscapeMarkup()}[/]"))
            .BorderColor(Color.Yellow)
            .RoundedBorder()
            .Header("[yellow]Warning[/]"));
    }

    public static string FormatBytes(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024.0):F1} MB",
        _ => $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB"
    };

    public static string FormatDate(DateTimeOffset? date) =>
        date?.LocalDateTime.ToString("yyyy-MM-dd HH:mm") ?? "[dim]—[/]";

    public static string MaskSecret(string? value) =>
        value is null or { Length: 0 } ? "[dim]not set[/]" : $"{value[..4]}{"".PadRight(value.Length - 4, '*')}";
}
