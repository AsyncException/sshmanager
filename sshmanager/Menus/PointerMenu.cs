using Microsoft.Extensions.Configuration;
using sshmanager.Database;
using sshmanager.Models;
using sshmanager.Utilities;

namespace sshmanager.Menus;

public class PointerMenu(MenuProvider menu_provider, DatabaseContext context, IConfiguration configuration) : BaseMenu(menu_provider, context, configuration)
{
    public async Task<ReturnType> ShowMenu(DestinationPointer pointer) {
        (Option<Server> server, Option<User> user) closestTargets = await pointer.GetClosest(Context);

        return closestTargets switch {
            { server.HasValue: true, user.HasValue: true } => await MenuProvider.UserMenu.ShowMenu(closestTargets.server, closestTargets.user),
            { server.HasValue: true, user.HasValue: false } => await MenuProvider.ServerMenu.ShowMenu(closestTargets.server),
            _ => await MenuProvider.MainMenu.ShowMenu(),
        };
    }
}
