using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Spectre.Console;
using sshmanager.Database;
using sshmanager.Models;

namespace sshmanager.Menus;

public class ServerMenu(MenuProvider menu_provider, DatabaseContext context, IConfiguration configuration) : BaseMenu(menu_provider, context, configuration)
{
    private static readonly Promptable<User>[] options = [new(Constants.SEPERATOR), new(Constants.ADD_USER), new(Constants.DELETE_SERVER), new(Constants.RETURN)];
    
    public async ValueTask<ReturnType> ShowMenu(Server server)
    {
        while (true)
        {
            AnsiConsole.Clear();
            if(await SwitchResponse(await ShowPopup(server), server) == ReturnType.Return) {
                return ReturnType.Break;
            }
        }
    }

    private async Task<Promptable<User>> ShowPopup(Server server) => AnsiConsole.Prompt(new SelectionPrompt<Promptable<User>>()
        .Title(server.Name)
        .AddChoices(await GetUsers(server))
        .AddChoices(options));

    private async Task<IEnumerable<Promptable<User>>> GetUsers(Server server) => (await Context.Users.Get(server)).OrderBy(e => e.Username).Select(e => new Promptable<User>(e));

    private async ValueTask<ReturnType> SwitchResponse(Promptable<User> response, Server server) => response switch {
        { IsOptions: true, OptionValue: Constants.SEPERATOR } => ReturnType.Break,
        { IsOptions: true, OptionValue: Constants.ADD_USER } => await CreateUser(server),
        { IsOptions: true, OptionValue: Constants.DELETE_SERVER } => await DeleteServer(server),
        { IsOptions: true, OptionValue: Constants.RETURN } => ReturnType.Return,
        { IsOptions: false } => await MenuProvider.UserMenu.ShowMenu(server, response.Value),
        _ => InvalidOption()
    };

    private async Task<ReturnType> DeleteServer(Server server) {
        await Context.Servers.Remove(server);
        return ReturnType.Return;
    }

    private async Task<ReturnType> CreateUser(Server server) {
        string name = AnsiConsole.Ask<string>("Enter username");
        string password = AnsiConsole.Prompt(new TextPrompt<string>("Enter password for this user").Secret());
        await Context.Users.Add(new() { Username = name, Password = password, Server = server });

        return ReturnType.Break;
    }

    private static ReturnType InvalidOption() {
        AnsiConsole.WriteException(new Exception("Invalid option: report this issue on github"), ExceptionFormats.ShortenEverything);
        Environment.Exit(0);

        return ReturnType.Return;
    }
}
