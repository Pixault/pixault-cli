using System.ComponentModel;
using Spectre.Console.Cli;

namespace Pixault.Cli.Shared;

public class ProjectSettings : CommandSettings
{
    [CommandOption("-p|--project")]
    [Description("Project ID (overrides config default)")]
    public string? Project { get; set; }
}
