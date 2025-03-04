using Microsoft.Extensions.Configuration;
using Spectre.Console;
using sshmanager.Database;
using sshmanager.Models;
using sshmanager.SSHClients;
using TextCopy;

namespace sshmanager.Menus;

/// <summary>
/// The main menu which shows the options after selecting a user.
/// </summary>
/// <param name="menu_provider">The <see cref="sshmanager.MenuProvider"/> to use</param>
/// <param name="context">The <see cref="DatabaseContext"/> to use</param>
/// <param name="configuration">The app configuration</param>
public class UserMenu(MenuProvider menu_provider, DatabaseContext context, IConfiguration configuration) : BaseMenu(menu_provider, context, configuration)
{
    /// <summary>
    /// The default choices appended to the bottom of the menu.
    /// </summary>
    private static readonly string[] default_choices = [Constants.CONNECT, Constants.COPY_PASSWORD, Constants.DELETE_USER, Constants.RETURN];

    /// <summary>
    /// Writes the user menu to the console
    /// </summary>
    /// <returns>Type of action to perform to the caller</returns>
    public async ValueTask<ReturnType> ShowMenu(Server server, User user)
    {
        while (true)
        {
            AnsiConsole.Clear();
            string userSelection = ShowPopup(server, user);
            ReturnType returnType = await GetResponseAction(userSelection, server, user);

            if (returnType is ReturnType.Return) {
                return ReturnType.Break;
            }
        }
    }

    /// <summary>
    /// Show the prompt to the user
    /// </summary>
    /// <returns>The prompt the user has selected</returns>
    private static string ShowPopup(Server server, User user) {
        IPrompt<string> selectionPrompt = new SelectionPrompt<string>()
            .Title($"{user.Username}@{server.Name}")
            .AddChoices(default_choices);

        return AnsiConsole.Prompt(selectionPrompt);
    }

    /// <summary>
    /// Gets the action that should be performed or enter deeper into a submenu
    /// </summary>
    /// <param name="response">The selection to user made</param>
    /// <param name="server">The server to convey down to sub menus</param>
    /// <param name="user">The user to convey down to sub menus</param>
    /// <returns>Follow up action to continue or leave the loop</returns>
    private async ValueTask<ReturnType> GetResponseAction(string response, Server server, User user) => response switch {
        Constants.CONNECT => ReturnType.Break.FromVoid(() => ISSHClient.FromConfig(Configuration).StartSession(server, user)),
        Constants.COPY_PASSWORD => ReturnType.Break.FromVoid(() => ClipboardService.SetText(user.Password)),
        Constants.DELETE_USER => await DeleteUser(user),
        Constants.RETURN => ReturnType.Return,
        _ => InvalidOption()
    };

    /// <summary>
    /// Deletes a user from a server
    /// </summary>
    /// <param name="user">The user to delete</param>
    /// <returns><see cref="ReturnType.Return"/></returns>
    private async ValueTask<ReturnType> DeleteUser(User user) {
        await Context.Users.Remove(user);
        return ReturnType.Return;
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
