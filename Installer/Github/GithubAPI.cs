using Installer.Github.Models;
using Spectre.Console;
using System.Net.Http.Json;

namespace Installer.Github;

public class GithubAPI : IDisposable
{
    private readonly HttpClient http_client = new();
    public GithubAPI() { http_client.DefaultRequestHeaders.Add("User-Agent", "sshmanager_installer"); }

    public async Task<Release> GetLatestRelease()
    {
        const string url = "https://api.github.com/repos/AsyncException/sshmanager/releases";
        Release[]? releases = await http_client.GetFromJsonAsync<Release[]>(url);

        if (releases is null) {
            AnsiConsole.WriteException(new Exception("Unable to get the latest release from github"), ExceptionFormats.ShortenEverything);
            Environment.Exit(0);
        }

        return releases[0];
    }

    public static string GetUrl(Release release) {
        Asset asset = release.Assets.First(e => e.Name.Contains(".zip"));
        return asset.BrowserDownloadUrl;
    }

    public void Dispose()
    {
        http_client.Dispose();
        GC.SuppressFinalize(this);
    }
}
