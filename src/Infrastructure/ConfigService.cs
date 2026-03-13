namespace Pixault.Cli.Infrastructure;

public sealed class ConfigService
{
    private CliConfig? _config;

    public CliConfig Config => _config ??= CliConfig.Load();

    public string ResolveBaseUrl(string? flag = null) =>
        flag
        ?? Environment.GetEnvironmentVariable("PIXAULT_BASE_URL")
        ?? Config.BaseUrl;

    public string? ResolveCdnUrl(string? flag = null) =>
        flag
        ?? Environment.GetEnvironmentVariable("PIXAULT_CDN_URL")
        ?? Config.CdnUrl;

    public string? ResolveProject(string? flag = null) =>
        flag
        ?? Environment.GetEnvironmentVariable("PIXAULT_PROJECT")
        ?? Config.DefaultProject;

    public string? ResolveApiKey(string? flag = null) =>
        flag
        ?? Environment.GetEnvironmentVariable("PIXAULT_API_KEY")
        ?? Config.ApiKey;

    public string? ResolveHmacSecret(string? flag = null) =>
        flag
        ?? Environment.GetEnvironmentVariable("PIXAULT_HMAC_SECRET")
        ?? Config.HmacSecret;

    public bool IsConfigured =>
        ResolveProject() is not null && ResolveApiKey() is not null;

    public void Save(CliConfig config)
    {
        config.Save();
        _config = config;
    }
}
