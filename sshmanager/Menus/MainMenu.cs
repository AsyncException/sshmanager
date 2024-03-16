using Microsoft.Extensions.Configuration;
using Spectre.Console;
using sshmanager;
using sshmanager.Models;

namespace sshmanager.Menus;

public class MainMenu(MenuProvider menu_provider, DatabaseContext context, IConfiguration configuration) : BaseMenu(menu_provider, context, configuration)
{
    private static readonly Promptable<Server>[] options = [new(Constants.SEPERATOR), new(Constants.ADD_SERVER), new(Constants.EXIT)];

    public ReturnType ShowMenu()
    {
        while (true)
        {
            AnsiConsole.Clear();
            if (SwitchResponse(ShowPopup()) == ReturnType.Return) {
                return ReturnType.Break;
            }
        }
    }

    private Promptable<Server> ShowPopup() => AnsiConsole.Prompt(new SelectionPrompt<Promptable<Server>>()
                .AddChoices(GetServers())
                .AddChoices(options));

    private IEnumerable<Promptable<Server>> GetServers() => Context.Servers.OrderBy(e => e.Name).Select(e => new Promptable<Server>(e));

    private ReturnType SwitchResponse(Promptable<Server> response) => response switch {
        { IsOptions: true, OptionValue: Constants.SEPERATOR } => ReturnType.Break,
        { IsOptions: true, OptionValue: Constants.ADD_SERVER } => CreateServer(),
        { IsOptions: true, OptionValue: Constants.EXIT } => Exit(),
        { IsOptions: false } => MenuProvider.ServerMenu.ShowMenu(response.Value),
        _ => InvalidOption()
    };

    private ReturnType CreateServer() {
        string name = AnsiConsole.Ask<string>("Enter server ip or hostname:\n");
        Context.Servers.Add(new() { Name = name });
        Context.SaveChanges();
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
