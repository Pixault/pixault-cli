using Microsoft.Extensions.DependencyInjection;
using Pixault.Cli.Commands.Config;
using Pixault.Cli.Commands.Eps;
using Pixault.Cli.Commands.Folders;
using Pixault.Cli.Commands.Images;
using Pixault.Cli.Commands.Plugins;
using Pixault.Cli.Commands.Transforms;
using Pixault.Cli.Commands.Upload;
using Pixault.Cli.Infrastructure;
using Pixault.Cli.Shared;
using Spectre.Console.Cli;

var services = new ServiceCollection();
services.AddSingleton<ConfigService>();
services.AddSingleton<ClientFactory>();

var app = new CommandApp(new TypeRegistrar(services));

app.Configure(config =>
{
    config.SetApplicationName("pixault");
    config.SetApplicationVersion("0.1.0");

    config.SetExceptionHandler((ex, _) =>
    {
        OutputHelpers.WriteError(ex.Message);
        return -1;
    });

    config.AddBranch("config", config =>
    {
        config.SetDescription("Manage CLI configuration");
        config.AddCommand<ConfigInitCommand>("init")
            .WithDescription("Interactive setup wizard");
        config.AddCommand<ConfigShowCommand>("show")
            .WithDescription("Display current configuration");
    });

    config.AddCommand<UploadCommand>("upload")
        .WithDescription("Upload file(s) to Pixault");

    config.AddBranch("images", images =>
    {
        images.SetDescription("Manage images");
        images.AddCommand<ImagesListCommand>("list")
            .WithDescription("List images with pagination");
        images.AddCommand<ImagesGetCommand>("get")
            .WithDescription("Get image details");
        images.AddCommand<ImagesUpdateCommand>("update")
            .WithDescription("Update image metadata");
        images.AddCommand<ImagesDeleteCommand>("delete")
            .WithDescription("Delete an image");
        images.AddCommand<ImagesUrlCommand>("url")
            .WithDescription("Generate CDN URL with transforms");
        images.AddCommand<ImagesEmbedCommand>("embed")
            .WithDescription("Generate SEO-optimized HTML embed tag");
        images.AddCommand<ImagesDerivedCommand>("derived")
            .WithDescription("List derived assets");
    });

    config.AddBranch("folders", folders =>
    {
        folders.SetDescription("Manage folders");
        folders.AddCommand<FoldersListCommand>("list")
            .WithDescription("List folder hierarchy");
        folders.AddCommand<FoldersCreateCommand>("create")
            .WithDescription("Create a folder");
        folders.AddCommand<FoldersDeleteCommand>("delete")
            .WithDescription("Delete a folder");
    });

    config.AddBranch("transforms", transforms =>
    {
        transforms.SetDescription("Manage named transforms");
        transforms.AddCommand<TransformsListCommand>("list")
            .WithDescription("List named transforms");
        transforms.AddCommand<TransformsGetCommand>("get")
            .WithDescription("Get transform details");
        transforms.AddCommand<TransformsCreateCommand>("create")
            .WithDescription("Create a named transform");
        transforms.AddCommand<TransformsDeleteCommand>("delete")
            .WithDescription("Delete a named transform");
    });

    config.AddBranch("plugins", plugins =>
    {
        plugins.SetDescription("Manage plugins");
        plugins.AddCommand<PluginsListCommand>("list")
            .WithDescription("List available plugins");
        plugins.AddCommand<PluginsInstalledCommand>("installed")
            .WithDescription("List installed plugins");
        plugins.AddCommand<PluginsActivateCommand>("activate")
            .WithDescription("Activate a plugin");
        plugins.AddCommand<PluginsDeactivateCommand>("deactivate")
            .WithDescription("Deactivate a plugin");
    });

    config.AddBranch("eps", eps =>
    {
        eps.SetDescription("EPS processing operations");
        eps.AddCommand<EpsStatusCommand>("status")
            .WithDescription("Check EPS processing status");
        eps.AddCommand<EpsSplitCommand>("split")
            .WithDescription("Trigger EPS design splitting");
        eps.AddCommand<EpsExtractSvgCommand>("extract-svg")
            .WithDescription("Extract vector SVG from an EPS file");
    });
});

return app.Run(args);
