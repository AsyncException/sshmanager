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


            Promptable<Server> response = AnsiConsole.Prompt(new SelectionPrompt<Promptable<Server>>()
                //Add servers to the list of options
                .AddChoices(servers.OrderBy(e => e.Name).Select(e => new Promptable<Server>(e)))
                //Add user choices to the end of the list
                .AddChoices([ new(Constants.SEPERATOR), new(Constants.ADD_SERVER), new(Constants.EXIT)]));

            switch (response) {
                case { IsOptions: true, OptionValue: Constants.SEPERATOR }:
                    continue;
                case { IsOptions: true, OptionValue: Constants.ADD_SERVER }:
                    string name = AnsiConsole.Ask<string>("Enter server ip or hostname:\n");
                    context.Servers.Add(new() { Name = name });
                    context.SaveChanges();
                    continue;
                case { IsOptions: true, OptionValue: Constants.EXIT }:
                    Environment.Exit(0);
                    break;
                case { IsOptions: false }:
                    PresentServer(context, response.Value);
                    break;
                default:
                    AnsiConsole.WriteException(new Exception("Invalid option: report this issue on github"));
                    Environment.Exit(0);
                    break;
            }
        }
    }

    private static void PresentServer(DatabaseContext context, Server server) {
        while (true) {
            AnsiConsole.Clear();
            Promptable<User> response = AnsiConsole.Prompt(new SelectionPrompt<Promptable<User>>()
                .Title(server.Name)
                //Add the list of users to the list
                .AddChoices(context.Users.Where(e => e.Server == server).OrderBy(e => e.Username).Select(e => new Promptable<User>(e)))
                //Add user choices to the end of the list
                .AddChoices([ new(Constants.SEPERATOR), new(Constants.ADD_SERVER), new(Constants.DELETE_SERVER), new(Constants.RETURN) ]));

            switch (response) {
                case { IsOptions: true, OptionValue: Constants.SEPERATOR }:
                    break;
                case { IsOptions: true, OptionValue: Constants.ADD_SERVER }:
                    string name = AnsiConsole.Ask<string>("Enter username");
                    string password = AnsiConsole.Prompt(new TextPrompt<string>("Enter password for this user").Secret());
                    context.Users.Add(new() { Username = name, Password = password, Server = server });
                    context.SaveChanges();
                    break;
                case { IsOptions: true, OptionValue: Constants.DELETE_SERVER }:
                    context.Servers.Remove(server);
                    context.SaveChanges();
                    return;
                case { IsOptions: true, OptionValue: Constants.RETURN }:
                    return;
                case { IsOptions: false }:
                    PresentUser(context, server, response.Value);
                    break;
                default:
                    AnsiConsole.WriteException(new Exception("Invalid option: report this issue on github"));
                    Environment.Exit(0);
                    break;
            }
        }
    }

    private static void PresentUser(DatabaseContext context, Server server, User user) {
        while (true) {
            AnsiConsole.Clear();
            string response = AnsiConsole.Prompt(new SelectionPrompt<string>().Title($"{user.Username}@{server.Name}")
                .AddChoices([Constants.CONNECT, Constants.COPY_PASSWORD, Constants.DELETE_USER, Constants.RETURN]));

            switch (response) {
                case Constants.CONNECT:
                    StartSSHSession(server.Name, user.Username);
                    break;
                case Constants.COPY_PASSWORD:
                    ClipboardService.SetText(user.Password);
                    break;
                case Constants.DELETE_USER:
                    context.Users.Remove(user);
                    context.SaveChanges();
                    return;
                case Constants.RETURN:
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