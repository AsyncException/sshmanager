using Microsoft.Extensions.Configuration;
using Spectre.Console;
using sshmanager.Database;
using sshmanager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace sshmanager.Menus;

public class PointerMenu(MenuProvider menu_provider, DatabaseContext context, IConfiguration configuration) : BaseMenu(menu_provider, context, configuration)
{
    public async Task<ReturnType> ShowMenu(DestinationPointer pointer) {
        if(pointer.User is null) {
            return await ServerMenu(pointer);
        }
        else {
            return await ServerUserMenu(pointer);
        }
    }

    public async Task<ReturnType> ServerMenu(DestinationPointer pointer) {
        Server server = await pointer.GetClosestServer(Context);

        await MenuProvider.ServerMenu.ShowMenu(server);

        return await MenuProvider.MainMenu.ShowMenu();
    }

    public async Task<ReturnType> ServerUserMenu(DestinationPointer pointer) {
        Server server = await pointer.GetClosestServer(Context);
        User user = await pointer.GetClosestUser(Context, server);

        await MenuProvider.UserMenu.ShowMenu(server, user);
        await MenuProvider.ServerMenu.ShowMenu(server);

        return await MenuProvider.MainMenu.ShowMenu();
    }
}
