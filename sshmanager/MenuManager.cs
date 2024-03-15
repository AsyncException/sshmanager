using Microsoft.Extensions.Configuration;
using Spectre.Console;
using sshmanager.Models;
using sshmanager.SSHClients;
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
                    AnsiConsole.WriteException(new Exception("Invalid option: report this issue on github"), ExceptionFormats.ShortenEverything);
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
                    AnsiConsole.WriteException(new Exception("Invalid option: report this issue on github"), ExceptionFormats.ShortenEverything);
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
                    ISSHClient.FromConfig(Configuration).StartSession(server, user);
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

    
}