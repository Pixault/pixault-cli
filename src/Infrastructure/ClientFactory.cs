using Microsoft.Extensions.Options;
using Pixault.Client;

namespace Pixault.Cli.Infrastructure;

public sealed class ClientFactory(ConfigService configService)
{
    private HttpClient? _httpClient;

    private HttpClient GetOrCreateHttpClient(string? projectFlag = null)
    {
        if (_httpClient is not null)
            return _httpClient;

        var baseUrl = configService.ResolveBaseUrl();
        var apiKey = configService.ResolveApiKey();

        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };

        if (apiKey is not null)
            _httpClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        return _httpClient;
    }

    private IOptions<PixaultOptions> CreateOptions(string? projectFlag = null)
    {
        var opts = new PixaultOptions
        {
            BaseUrl = configService.ResolveBaseUrl(),
            CdnUrl = configService.ResolveCdnUrl(),
            DefaultProject = configService.ResolveProject(projectFlag),
            ApiKey = configService.ResolveApiKey(),
            HmacSecret = configService.ResolveHmacSecret()
        };
        return Options.Create(opts);
    }

    public PixaultAdminClient CreateAdminClient(string? projectFlag = null) =>
        new(GetOrCreateHttpClient(projectFlag), CreateOptions(projectFlag));

    public PixaultUploadClient CreateUploadClient(string? projectFlag = null) =>
        new(GetOrCreateHttpClient(projectFlag), CreateOptions(projectFlag));

    public PixaultUrlBuilder CreateUrlBuilder(string imageId, string? projectFlag = null)
    {
        var cdnUrl = configService.ResolveCdnUrl() ?? configService.ResolveBaseUrl();
        var project = configService.ResolveProject(projectFlag)
            ?? throw new InvalidOperationException("No project configured. Run 'pixault config init' first.");
        return new PixaultUrlBuilder(cdnUrl, project, imageId);
    }

    public string ResolveProject(string? projectFlag = null) =>
        configService.ResolveProject(projectFlag)
        ?? throw new InvalidOperationException("No project configured. Run 'pixault config init' or use --project.");
}
