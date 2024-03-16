using Microsoft.Extensions.Configuration;

namespace sshmanager.Menus;

public abstract class BaseMenu(MenuProvider menu_provider, DatabaseContext context, IConfiguration configuration)
{
    private protected MenuProvider MenuProvider { get; } = menu_provider;
    private protected DatabaseContext Context { get; } = context;
    private protected IConfiguration Configuration { get; } = configuration;
}
