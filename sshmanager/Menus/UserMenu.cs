using Microsoft.Extensions.Configuration;
using Spectre.Console;
using sshmanager.Models;
using sshmanager.SSHClients;
using TextCopy;

namespace sshmanager.Menus;

public class UserMenu(MenuProvider menu_provider, DatabaseContext context, IConfiguration configuration) : BaseMenu(menu_provider, context, configuration)
{
    private static readonly string[] options = [Constants.CONNECT, Constants.COPY_PASSWORD, Constants.DELETE_USER, Constants.RETURN];
    public ReturnType ShowMenu(Server server, User user)
    {
        while (true)
        {
            AnsiConsole.Clear();
            if(SwitchResponse(ShowPopup(server, user), server, user) == ReturnType.Return) {
                return ReturnType.Break;
            }
        }
    }

    private static string ShowPopup(Server server, User user) => AnsiConsole.Prompt(new SelectionPrompt<string>()
        .Title($"{user.Username}@{server.Name}")
        .AddChoices(options));

    private ReturnType SwitchResponse(string response, Server server, User user) => response switch {
        Constants.CONNECT => ReturnType.Break.FromVoid(() => ISSHClient.FromConfig(Configuration).StartSession(server, user)),
        Constants.COPY_PASSWORD => ReturnType.Break.FromVoid(() => ClipboardService.SetText(user.Password)),
        Constants.DELETE_USER => DeleteUser(user),
        Constants.RETURN => ReturnType.Return,
        _ => InvalidOption()
    };

    private ReturnType DeleteUser(User user) {
        Context.Users.Remove(user);
        Context.SaveChanges();

        return ReturnType.Return;
    }

    private static ReturnType InvalidOption() {
        AnsiConsole.WriteException(new Exception("Invalid option: report this issue on github"), ExceptionFormats.ShortenEverything);
        Environment.Exit(0);

        return ReturnType.Return;
    }
}
