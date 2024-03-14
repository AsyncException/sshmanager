using Installer.Github;
using Installer.Github.Models;
using Spectre.Console;
using System.IO.Compression;
using System.Security.Principal;
using System.Text;

namespace Installer;

internal class Program {
    private const string INSTALLATION_DIRECTORY = "C:\\Program Files\\sshmanager";
    private const string TEMP_FILE_NAME = "sshmanager.zip";
    private static readonly string temp_file_path = Path.Combine(Path.GetTempPath(), TEMP_FILE_NAME);
    private static readonly GithubAPI github_api = new();
    static async Task Main(string[] args) {
        Console.OutputEncoding = Encoding.UTF8;
        ShowPreChecks();

        string url = string.Empty;
        try {
            Release release = await github_api.GetLatestRelease();
            url = GithubAPI.GetUrl(release);
        }
        catch (Exception ex) {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            Console.ReadLine();
            Environment.Exit(0);
        }

        try {
            await DownloadFiles(url);
        }
        catch (Exception ex) {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            Console.ReadLine();
            Environment.Exit(0);
        }

        try {
            Unpack();
        }
        catch (Exception ex) {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            Console.ReadLine();
            Environment.Exit(0);
        }
    }

    private static void ShowPreChecks() => AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots2)
        .SpinnerStyle(Style.Plain)
        .Start("Starting installer", ctx => {
            Utility.ShowWelcome();

            ctx.Status("Checking permissions");
            if (Utility.IsAdministrator()) {
                AnsiConsole.MarkupLine(":check_mark_button: [green]Permissions.[/]");
            }
            else {
                AnsiConsole.MarkupLine(":cross_mark: [red]Permissions are invalid. Please run as Administrator.[/]");
                Console.ReadLine();
                Environment.Exit(0);
            }

            // Update the status and spinner
            ctx.Status("Checking network");
            if (Utility.HasNetworkConnectivity()) {
                AnsiConsole.MarkupLine(":check_mark_button: [green]Network[/]");
            }
            else {
                AnsiConsole.MarkupLine(":cross_mark: [red]Network is required to use this installer.[/]");
                Console.ReadLine();
                Environment.Exit(0);
            }
        });
    private static async Task DownloadFiles(string url) => await AnsiConsole.Progress()
        .Columns([
            new TaskDescriptionColumn() { Alignment = Justify.Left },
            new ProgressBarColumn() { RemainingStyle = new Style(foreground: Color.Green) },
            new PercentageColumn(),
            new RemainingTimeColumn(),
            new SpinnerColumn() { Spinner = Spinner.Known.Dots2 }
        ])
        .StartAsync(async ctx => {
            ProgressTask download_task = ctx.AddTask("Downloading files");

            using HttpClient client = new();

            Progress<float> progress = new();
            progress.ProgressChanged += (sender, total) => {
                download_task.Value = total;
            };

            using FileStream file = new(temp_file_path, FileMode.Create, FileAccess.Write, FileShare.None);
            await client.DownloadDataAsync(url, file, progress);
        });
    private static void Unpack() {
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots2)
            .SpinnerStyle(Style.Plain)
            .Start("Starting extraction...", ctx => {
                if (!Path.Exists(INSTALLATION_DIRECTORY)) {
                    try {
                        Directory.CreateDirectory(INSTALLATION_DIRECTORY);
                        AnsiConsole.MarkupLine(":check_mark_button: [green]Installation folder created.[/]");
                    }
                    catch (Exception ex){
                        AnsiConsole.MarkupLine(":cross_mark: [red]Could not create installation directory.[/]");
                        AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
                        Console.ReadLine();
                        Environment.Exit(0);
                    }
                    
                }

                try {
                    using FileStream stream = File.OpenRead(temp_file_path);
                    using ZipArchive archive = new(stream);

                    int i = 1;
                    foreach (ZipArchiveEntry entry in archive.Entries) {
                        ctx.Status($"Extracting {entry.Name} {i}/{archive.Entries.Count}");
                        entry.ExtractToFile(Path.Combine(INSTALLATION_DIRECTORY, entry.Name));
                        i++;
                    }

                    AnsiConsole.MarkupLine(":check_mark_button: [green]Extracted files.[/]");
                }
                catch (Exception ex) {
                    AnsiConsole.MarkupLine(":cross_mark: [red]Could not extract files.[/]");
                    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
                    Console.ReadLine();
                    Environment.Exit(0);
                }
            });
    }
}

internal static class Utility {
    public static void ShowWelcome() {
        AnsiConsole.Write(new FigletText("SSH Manager")
                .Centered()
                .Color(Color.White));
    }
    public static bool HasNetworkConnectivity() => System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
    public static bool IsAdministrator() {
        using (WindowsIdentity identity = WindowsIdentity.GetCurrent()) {
            WindowsPrincipal principal = new(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
