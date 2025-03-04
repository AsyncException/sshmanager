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
    private static readonly Promptable<Server>[] options = [new(Constants.SEPERATOR), new(Constants.ADD_SERVER), new(Constants.EXIT)];

    /// <summary>
    /// Writes the main menu to the console
    /// </summary>
    /// <returns>Type of action to perform to the caller</returns>
    public async ValueTask<ReturnType> ShowMenu()
    {
        while (true)
        {
            AnsiConsole.Clear();
            if (await SwitchResponse(await ShowPopup()) == ReturnType.Return) {
                return ReturnType.Break;
            }
        }
    }

    private async Task<Promptable<Server>> ShowPopup() => AnsiConsole.Prompt(new SelectionPrompt<Promptable<Server>>()
                .AddChoices(await GetServers())
                .AddChoices(options));

    private async Task<IEnumerable<Promptable<Server>>> GetServers() => (await Context.Servers.Get()).OrderBy(e => e.Name).Select(e => new Promptable<Server>(e));

    private async ValueTask<ReturnType> SwitchResponse(Promptable<Server> response) => response switch {
        { IsOptions: true, OptionValue: Constants.SEPERATOR } => ReturnType.Break,
        { IsOptions: true, OptionValue: Constants.ADD_SERVER } => await CreateServer(),
        { IsOptions: true, OptionValue: Constants.EXIT } => Exit(),
        { IsOptions: false } => await MenuProvider.ServerMenu.ShowMenu(response.Value),
        _ => InvalidOption()
    };

    private async Task<ReturnType> CreateServer() {
        string name = AnsiConsole.Ask<string>("Enter server ip or hostname:\n");
        await Context.Servers.Add(new() { Name = name });
        return ReturnType.Break;
    }

    private static ReturnType InvalidOption() {
        AnsiConsole.WriteException(new Exception("Invalid option: report this issue on github"), ExceptionFormats.ShortenEverything);
        Environment.Exit(0);
        return ReturnType.Return;
    }

    private static ReturnType Exit() {
        Environment.Exit(0);
        return ReturnType.Return;
    }
}
