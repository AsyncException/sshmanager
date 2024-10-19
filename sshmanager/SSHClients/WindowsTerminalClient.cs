using Microsoft.Extensions.Configuration;
using Spectre.Console;
using sshmanager.Models;
using System.Diagnostics;
using System.Text;

namespace sshmanager.SSHClients;

internal class WindowsTerminalClient(IConfiguration configuration) : ISSHClient
{
    private readonly IConfiguration configuration = configuration;

    public void StartSession(Server server, User user) {
        ConnectionDetails connection = ParseServer(server.Name);
        string start_process = $"Powershell Start-Process -NoNewWindow -FilePath ssh -ArgumentList '{user.Username}@{connection.Server} -p {connection.Port}'";
        Process ssh_process = new() {
            StartInfo = new ProcessStartInfo() {
                FileName = "wt",
                Arguments = $"{GenerateArugments(server.Name, user.Username)}{start_process}",
                RedirectStandardInput = false,
                RedirectStandardOutput = false,
                UseShellExecute = true,
                CreateNoWindow = false
            }
        };

        try {
            ssh_process.Start();
        }
        catch (Exception ex) {
            AnsiConsole.MarkupLine("[red]Error starting SSH process[/]");
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
        }
    }

    private ConnectionDetails ParseServer(ReadOnlySpan<char> server) {
        Span<Range> ranges = stackalloc Range[2];
        server.Split(ranges, ':');

        if (server[ranges[1]].Length == 0) {
            return new(server.ToString(), 22);
        }

        if (!int.TryParse(server[ranges[1]], out int port)) {
            return new(server.ToString(), 22);
        }

        return new(server[ranges[0]].ToString(), port);
    }

    private string GenerateArugments(string server, string user) {
        IConfigurationSection section = configuration.GetRequiredSection("WindowsTerminalSettings");

        (string size, string orientation) = (string.Empty, string.Empty);
        string window = section["Window"] ?? "0";
        string title = section["Title"] ?? "{server}";
        string profile = section["Profile"] ?? "Powershell";
        bool suppress_title = bool.Parse(section["SupressApplicationTitle"] ?? "False");
        bool split = bool.Parse(section["Split:Enable"] ?? "False");
        if (split) {
            size = section["Split:Size"] ?? "0.5";
            orientation = section["Split:Orientation"] ?? "V";
        }

        StringBuilder builder = new();
        builder.Append("--window ").Append(window);
        if (split) {
            builder.Append(" split-pane");
            builder.Append(" --size ").Append(size);
            builder.Append(" -").Append(orientation);
        }

        builder.Append(" --title ").Append(InjectVariables(title, server, user));

        if (suppress_title) {
            builder.Append(" --suppressApplicationTitle");
        }

        builder.Append(" -p \"").Append(profile).Append('"');
        builder.Append(" -d . ");

        return builder.ToString();
    }

    private static string InjectVariables(string input, string server, string user) {
        Dictionary<string, string> replace_pairs = new() {
            {"{server}", server},
            {"{user}", user}
        };

        foreach (KeyValuePair<string, string> replace in replace_pairs) {
            input = input.Replace(replace.Key, replace.Value);
        }

        return input;
    }
}