using Installer.Github.Models;
using Spectre.Console;
using System.Net.Http.Json;

namespace Installer.Github;

public class GithubAPI : IDisposable
{
    private readonly HttpClient http_client = new() { BaseAddress = new Uri("https://api.github.com") };

    public async Task<Release> GetLatestRelease()
    {
        const string url = "/repos/AsyncException/Steam-Shortcut-Generator/releases/latest";
        Release? release = await http_client.GetFromJsonAsync<Release>(url);

        if(release is null) {
            AnsiConsole.WriteException(new Exception("Unable to get the latest release from github"), ExceptionFormats.ShortenEverything);
            Environment.Exit(0);
        }

        return release;
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
