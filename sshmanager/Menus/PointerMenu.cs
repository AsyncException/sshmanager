using Microsoft.Extensions.Configuration;
using sshmanager.Database;
using sshmanager.Models;
using sshmanager.Utilities;

namespace sshmanager.Menus;

/// <summary>
/// Directly points towards a server or user if they are found otherwise shows the main menu
/// </summary>
/// <param name="menu_provider">The <see cref="sshmanager.MenuProvider"/> to use</param>
/// <param name="context">The <see cref="DatabaseContext"/> to use</param>
/// <param name="configuration">The app configuration</param>
public class PointerMenu(MenuProvider menu_provider, DatabaseContext context, IConfiguration configuration) : BaseMenu(menu_provider, context, configuration)
{
    /// <summary>
    /// Writes the pointer to the console
    /// </summary>
    /// <returns>Type of action to perform to the caller</returns>
    public async Task<ReturnType> ShowMenu(DestinationPointer pointer) {
        (Option<Server> server, Option<User> user) closestTargets = await pointer.GetClosest(Context);

        return closestTargets switch {
            { server.HasValue: true, user.HasValue: true } => await MenuProvider.UserMenu.ShowMenu(closestTargets.server, closestTargets.user),
            { server.HasValue: true, user.HasValue: false } => await MenuProvider.ServerMenu.ShowMenu(closestTargets.server),
            _ => await MenuProvider.MainMenu.ShowMenu(),
        };
    }
}
