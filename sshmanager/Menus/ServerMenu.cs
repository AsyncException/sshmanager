using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Spectre.Console;
using sshmanager.Models;

namespace sshmanager.Menus;

public class ServerMenu(MenuProvider menu_provider, DatabaseContext context, IConfiguration configuration) : BaseMenu(menu_provider, context, configuration)
{
    private static readonly Promptable<User>[] options = [new(Constants.SEPERATOR), new(Constants.ADD_USER), new(Constants.DELETE_SERVER), new(Constants.RETURN)];
    
    public ReturnType ShowMenu(Server server)
    {
        while (true)
        {
            AnsiConsole.Clear();
            if(SwitchResponse(ShowPopup(server), server) == ReturnType.Return) {
                return ReturnType.Break;
            }
        }
    }

    private Promptable<User> ShowPopup(Server server) => AnsiConsole.Prompt(new SelectionPrompt<Promptable<User>>()
        .Title(server.Name)
        .AddChoices(GetUsers(server))
        .AddChoices(options));

    private IEnumerable<Promptable<User>> GetUsers(Server server) => Context.Users.Where(e => e.Server == server).OrderBy(e => e.Username).Select(e => new Promptable<User>(e));

    private ReturnType SwitchResponse(Promptable<User> response, Server server) => response switch {
        { IsOptions: true, OptionValue: Constants.SEPERATOR } => ReturnType.Break,
        { IsOptions: true, OptionValue: Constants.ADD_USER } => CreateUser(server),
        { IsOptions: true, OptionValue: Constants.DELETE_SERVER } => DeleteServer(server),
        { IsOptions: true, OptionValue: Constants.RETURN } => ReturnType.Return,
        { IsOptions: false } => MenuProvider.UserMenu.ShowMenu(server, response.Value),
        _ => InvalidOption()
    };

    private ReturnType DeleteServer(Server server) {
        Context.Servers.Remove(server);
        Context.SaveChanges();

        return ReturnType.Return;
    }

    private ReturnType CreateUser(Server server) {
        string name = AnsiConsole.Ask<string>("Enter username");
        string password = AnsiConsole.Prompt(new TextPrompt<string>("Enter password for this user").Secret());
        Context.Users.Add(new() { Username = name, Password = password, Server = server });
        Context.SaveChanges();

        return ReturnType.Break;
    }

    private static ReturnType InvalidOption() {
        AnsiConsole.WriteException(new Exception("Invalid option: report this issue on github"), ExceptionFormats.ShortenEverything);
        Environment.Exit(0);

        return ReturnType.Return;
    }
}
