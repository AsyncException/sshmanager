using Microsoft.Extensions.Configuration;
using sshmanager.Database;

namespace sshmanager.Menus;

/// <summary>
/// Base class for all menus
/// </summary>
/// <param name="menu_provider">The <see cref="sshmanager.MenuProvider"/> to use</param>
/// <param name="context">The <see cref="DatabaseContext"/> to use</param>
/// <param name="configuration">The app configuration</param>
public abstract class BaseMenu(MenuProvider menu_provider, DatabaseContext context, IConfiguration configuration)
{
    private protected MenuProvider MenuProvider { get; } = menu_provider;
    private protected DatabaseContext Context { get; } = context;
    private protected IConfiguration Configuration { get; } = configuration;
}
