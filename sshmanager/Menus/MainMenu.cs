using Microsoft.Extensions.Configuration;
using Spectre.Console;
using sshmanager.Database;
using sshmanager.Models;

namespace sshmanager.Menus;

/// <summary>
/// The main menu which shows the servers
/// </summary>
/// <param name="menu_provider">The <see cref="sshmanager.MenuProvider"/> to use</param>
/// <param name="context">The <see cref="DatabaseContext"/> to use</param>
/// <param name="configuration">The app configuration</param>
public class MainMenu(MenuProvider menu_provider, DatabaseContext context, IConfiguration configuration) : BaseMenu(menu_provider, context, configuration)
{
    /// <summary>
    /// The default choices appended to the bottom of the menu.
    /// </summary>
    private static readonly Promptable<Server>[] default_choices = [new(Constants.SEPERATOR), new(Constants.ADD_SERVER), new(Constants.EXIT)];

    /// <summary>
    /// Writes the main menu to the console
    /// </summary>
    /// <returns>Type of action to perform to the caller</returns>
    public async ValueTask<ReturnType> ShowMenu() {
        while (true) {
            AnsiConsole.Clear();
            Promptable<Server> userSelection = await ShowPopup();
            ReturnType returnType = await GetResponseAction(userSelection);

            if (returnType is ReturnType.Return) {
                return ReturnType.Break;
            }
        }
    }

    /// <summary>
    /// Show the prompt to the user
    /// </summary>
    /// <returns>The prompt the user has selected</returns>
    private async Task<Promptable<Server>> ShowPopup() {
        List<Promptable<Server>> servers = await GetServers();
        IPrompt<Promptable<Server>> selectionPrompt = new SelectionPrompt<Promptable<Server>>().AddChoices([.. servers, .. default_choices]);
        return AnsiConsole.Prompt(selectionPrompt);
    }

    /// <summary>
    /// Get the servers from the database and put them in a promptable structure
    /// </summary>
    /// <returns>The list of promptable servers</returns>
    private async Task<List<Promptable<Server>>> GetServers() {
        IEnumerable<Server> servers = await Context.Servers.Get();
        IOrderedEnumerable<Server> ordered = servers.OrderBy(e => e.Name);
        IEnumerable<Promptable<Server>> promptables = ordered.Select(e => new Promptable<Server>(e));

        return [.. promptables];
    }

    /// <summary>
    /// Gets the action that should be performed or enter deeper into a submenu
    /// </summary>
    /// <param name="response">The selection to user made</param>
    /// <returns>Follow up action to continue or leave the loop</returns>
    private async ValueTask<ReturnType> GetResponseAction(Promptable<Server> response) => response switch {
        { IsOptions: true, OptionValue: Constants.SEPERATOR } => ReturnType.Break,
        { IsOptions: true, OptionValue: Constants.ADD_SERVER } => await CreateServer(),
        { IsOptions: true, OptionValue: Constants.EXIT } => Exit(),
        { IsOptions: false } => await MenuProvider.ServerMenu.ShowMenu(response.Value),
        _ => InvalidOption()
    };

    /// <summary>
    /// Creates a new server and adds it to the server list.
    /// </summary>
    /// <returns><see cref="ReturnType.Break"/></returns>
    private async Task<ReturnType> CreateServer() {
        string name = AnsiConsole.Ask<string>("Enter server ip or hostname:\n");
        await Context.Servers.Add(new() { Name = name });
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

    /// <summary>
    /// Exists the application
    /// </summary>
    /// <returns>Returns <see cref="ReturnType.Return"/>. This method exits the application so the <see cref="ReturnType"/> is useless</returns>
    private static ReturnType Exit() {
        Environment.Exit(0);
        return ReturnType.Return;
    }
}
