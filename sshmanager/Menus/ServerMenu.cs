using Microsoft.Extensions.Configuration;
using Spectre.Console;
using sshmanager.Database;
using sshmanager.Models;

namespace sshmanager.Menus;

/// <summary>
/// The server menu that shows the content of the server
/// </summary>
/// <param name="menu_provider">The <see cref="sshmanager.MenuProvider"/> to use</param>
/// <param name="context">The <see cref="DatabaseContext"/> to use</param>
/// <param name="configuration">The app configuration</param>
public class ServerMenu(MenuProvider menu_provider, DatabaseContext context, IConfiguration configuration) : BaseMenu(menu_provider, context, configuration)
{
    /// <summary>
    /// The default choices appended to the bottom of the menu.
    /// </summary>
    private static readonly Promptable<User>[] default_choices = [new(Constants.SEPERATOR), new(Constants.ADD_USER), new(Constants.DELETE_SERVER), new(Constants.RETURN)];

    /// <summary>
    /// Writes the server menu to the console
    /// </summary>
    /// <returns>Type of action to perform to the caller</returns>
    public async ValueTask<ReturnType> ShowMenu(Server server)
    {
        while (true)
        {
            AnsiConsole.Clear();
            Promptable<User> userSelection = await ShowPopup(server);
            ReturnType returnType = await GetResponseAction(userSelection, server);

            if (returnType is ReturnType.Return) {
                return ReturnType.Break;
            }
        }
    }

    /// <summary>
    /// Show the prompt to the user
    /// </summary>
    /// <returns>The prompt the user has selected</returns>
    private async Task<Promptable<User>> ShowPopup(Server server) {
        List<Promptable<User>> users = await GetUsers(server);
        IPrompt<Promptable<User>> selectionPrompt = new SelectionPrompt<Promptable<User>>()
            .Title(server.Name)
            .AddChoices([.. users, .. default_choices]);

        return AnsiConsole.Prompt(selectionPrompt);
    }

    /// <summary>
    /// Get the users from the database and put them in a promptable structure
    /// </summary>
    /// <param name="server">What server to get the users for</param>
    /// <returns>The list of promptable users</returns>
    private async Task<List<Promptable<User>>> GetUsers(Server server) {
        IEnumerable<User> users = await Context.Users.Get(server);
        IOrderedEnumerable<User> ordered = users.OrderBy(user => user.Username);
        IEnumerable<Promptable<User>> promptables = ordered.Select(user => new Promptable<User>(user));

        return [.. promptables];
    }

    /// <summary>
    /// Gets the action that should be performed or enter deeper into a submenu
    /// </summary>
    /// <param name="response">The selection to user made</param>
    /// <param name="server">The server to convey down the sub menus</param>
    /// <returns>Follow up action to continue or leave the loop</returns>
    private async ValueTask<ReturnType> GetResponseAction(Promptable<User> response, Server server) => response switch {
        { IsOptions: true, OptionValue: Constants.SEPERATOR } => ReturnType.Break,
        { IsOptions: true, OptionValue: Constants.ADD_USER } => await CreateUser(server),
        { IsOptions: true, OptionValue: Constants.DELETE_SERVER } => await DeleteServer(server),
        { IsOptions: true, OptionValue: Constants.RETURN } => ReturnType.Return,
        { IsOptions: false } => await MenuProvider.UserMenu.ShowMenu(server, response.Value),
        _ => InvalidOption()
    };

    /// <summary>
    /// Deletes a server
    /// </summary>
    /// <param name="server">The server to delete</param>
    /// <returns><see cref="ReturnType.Return"/></returns>
    private async Task<ReturnType> DeleteServer(Server server) {
        await Context.Servers.Remove(server);
        return ReturnType.Return;
    }

    /// <summary>
    /// Creates a new user for the selected server
    /// </summary>
    /// <param name="server">The server that is selected</param>
    /// <returns><see cref="ReturnType.Break"/></returns>
    private async Task<ReturnType> CreateUser(Server server) {
        string name = AnsiConsole.Ask<string>("Enter username");
        string password = AnsiConsole.Prompt(new TextPrompt<string>("Enter password for this user").Secret());
        await Context.Users.Add(new() { Username = name, Password = password, Server = server });

        return ReturnType.Break;
    }

    /// <summary>
    /// Creates an exception if somehow the user selects an option that does not exist. This should never happen.
    /// </summary>
    /// <returns><see cref="ReturnType.Return"/></returns>
    private static ReturnType InvalidOption() {
        AnsiConsole.WriteException(new Exception("Invalid option: report this issue on github"), ExceptionFormats.ShortenEverything);
        Environment.Exit(0);

        return ReturnType.Return;
    }
}
