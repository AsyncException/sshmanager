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
    static async Task Main() {
        Console.OutputEncoding = Encoding.UTF8;
        ShowPreChecks();

        string url = string.Empty;
        try {
            Release release = await github_api.GetLatestRelease();
            url = GithubAPI.GetUrl(release);
            AnsiConsole.MarkupLine($"Downloading version {release.TagName}");
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
            UnpackAndClean();
        }
        catch (Exception ex) {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            Console.ReadLine();
            Environment.Exit(0);
        }

        AnsiConsole.MarkupLine(":check_mark_button: [green]Installation succeeded.[/]");
        Console.ReadLine();
        Environment.Exit(0);
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
    private static void UnpackAndClean() {
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots2)
            .SpinnerStyle(Style.Plain)
            .Start("Starting extraction...", ctx => {
                if (!Path.Exists(INSTALLATION_DIRECTORY)) {
                    try {
                        Directory.CreateDirectory(INSTALLATION_DIRECTORY);
                        AnsiConsole.MarkupLine(":check_mark_button: [green]Installation folder created.[/]");
                    }
                    catch (Exception ex) {
                        AnsiConsole.MarkupLine(":cross_mark: [red]Could not create installation directory.[/]");
                        AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
                        Console.ReadLine();
                        Environment.Exit(0);
                    }
                }
                else {
                    try {
                        DirectoryInfo directory = new(INSTALLATION_DIRECTORY);

                        foreach (FileInfo file in directory.GetFiles()) {
                            file.Delete();
                        }
                        foreach (DirectoryInfo dir in directory.GetDirectories()) {
                            dir.Delete(true);
                        }

                        AnsiConsole.MarkupLine(":check_mark_button: [green]Cleaned up old files.[/]");
                    }
                    catch (Exception ex) {
                        AnsiConsole.MarkupLine(":cross_mark: [red]Could not remove old files.[/]");
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
                        string full_path = Path.Combine(INSTALLATION_DIRECTORY, entry.FullName);
                        string directory = Path.GetDirectoryName(full_path) ?? INSTALLATION_DIRECTORY;
                        if (!Directory.Exists(directory)) {
                            Directory.CreateDirectory(directory);
                        }

                        entry.ExtractToFile(full_path, true);
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

                try {
                    ctx.Status("Setting Environment Variable");
                    string? value = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User);
                    if (!string.IsNullOrEmpty(value) && !value.Contains(INSTALLATION_DIRECTORY)) {
                        Environment.SetEnvironmentVariable("Path", string.Concat(value, $";{INSTALLATION_DIRECTORY}"), EnvironmentVariableTarget.User);
                        AnsiConsole.MarkupLine(":check_mark_button: [green]Environment Variables set.[/]");
                    }
                }
                catch (Exception ex) {
                    AnsiConsole.MarkupLine(":cross_mark: [red]Could not set the environment variables.[/]");
                    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
                }

                try {
                    ctx.Status("Cleaning up");
                    File.Delete(temp_file_path);
                    AnsiConsole.MarkupLine(":check_mark_button: [green]Temp files removed.[/]");
                }
                catch (Exception ex) {
                    AnsiConsole.MarkupLine(":cross_mark: [red]Could remove temp files.[/]");
                    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
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
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}
