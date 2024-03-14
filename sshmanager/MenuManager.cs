using Microsoft.Extensions.Configuration;
using Spectre.Console;
using sshmanager.Models;
using System.Diagnostics;
using System.Text;
using TextCopy;

namespace sshmanager;
public static class MenuManager
{
    public static IConfiguration Configuration { get; set; } = default!;
    public static void PresentServerList(DatabaseContext context) {
        while (true) {
            AnsiConsole.Clear();
            List<Server> servers = context.Servers.ToList();
            string response = AnsiConsole.Prompt(new SelectionPrompt<string>().AddChoices([.. servers.Select(e => e.Name), "----------", "Add Server", "Exit"]));

            if(response == "----------") {
                //Ignore spacer
                continue;
            }

            if(response == "Add Server") {
                string name = AnsiConsole.Ask<string>("Enter server ip or hostname:\n");
                context.Servers.Add(new() { Name = name });
                context.SaveChanges();
                continue;
            }

            if(response == "Exit") {
                Environment.Exit(0);
            }

            PresentServer(context, servers.First(e => e.Name == response));
        }

    }

    private static void PresentServer(DatabaseContext context, Server server) {
        while (true) {
            AnsiConsole.Clear();
            string response = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title(server.Name)
                .AddChoices([.. context.Users.Where(e => e.Server == server).Select(e => e.Username), "----------", "Add user", "Delete Server", "Return"]));

            switch (response) {
                case "----------":
                    break;
                case "Add user":
                    string name = AnsiConsole.Ask<string>("Enter username");
                    string password = AnsiConsole.Prompt(new TextPrompt<string>("Enter password for this user").Secret());
                    context.Users.Add(new() { Username = name, Password = password, Server = server });
                    context.SaveChanges();
                    break;
                case "Delete Server":
                    context.Servers.Remove(server);
                    context.SaveChanges();
                    return;
                case "Return":
                    return;
                default:
                    PresentUser(context, server, context.Users.First(e => e.Username == response));
                    break;
            }
        }
    }

    private static void PresentUser(DatabaseContext context, Server server, User user) {
        while (true) {
            AnsiConsole.Clear();
            string response = AnsiConsole.Prompt(new SelectionPrompt<string>().Title($"{user.Username}@{server.Name}").AddChoices(["Connect", "Copy password", "Delete user", "Return"]));

            switch (response) {
                case "Connect":
                    StartSSHSession(server.Name, user.Username);
                    break;
                case "Copy password":
                    ClipboardService.SetText(user.Password);
                    break;
                case "Delete user":
                    context.Users.Remove(user);
                    context.SaveChanges();
                    return;
                case "Return":
                    return;
            }
        }
    }

    private static void StartSSHSession(string server, string user) {
        string start_process = $"Powershell Start-Process -NoNewWindow -FilePath ssh -ArgumentList {user}@{server}";
        //$"--window 0 split-pane --size 0.8 --title {server} --suppressApplicationTitle -p \"Powershell\" -d . {start_process}",
        Process ssh_process = new() {
            StartInfo = new ProcessStartInfo() {
                FileName = "wt",
                Arguments = $"{GenerateArugments(server, user)}{start_process}",
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
            Console.WriteLine($"Error starting SSH process: {ex.Message}");
        }
    }

    private static string GenerateArugments(string server, string user) {
        IConfigurationSection section = Configuration.GetRequiredSection("TerminalSettings");

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

        builder.Append(" --title ").Append(title.InjectVariables(server, user));

        if(suppress_title) {
            builder.Append(" --suppressApplicationTitle");
        }

        builder.Append(" -p \"").Append(profile).Append('"');
        builder.Append(" -d . ");

        return builder.ToString();
    }

    private static string InjectVariables(this string input, string server, string user) {
        Dictionary<string, string> replace_pairs = new() {
            {"{server}", server},
            {"{user}", user}
        };

        foreach(KeyValuePair<string, string> replace in replace_pairs) {
            input = input.Replace(replace.Key, replace.Value);
        }

        return input;
    }
}