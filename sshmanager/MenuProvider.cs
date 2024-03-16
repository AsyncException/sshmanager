using Microsoft.Extensions.Configuration;
using sshmanager.Menus;

namespace sshmanager;

public class MenuProvider(DatabaseContext context, IConfiguration configuration)
{
    private readonly DatabaseContext context = context;
    private readonly IConfiguration configuration = configuration;

    private MainMenu? main_menu;
    public MainMenu MainMenu => main_menu ??= new MainMenu(this, context, configuration);

    private ServerMenu? server_menu;
    public ServerMenu ServerMenu => server_menu ??= new ServerMenu(this, context, configuration);

    private UserMenu? user_menu;
    public UserMenu UserMenu => user_menu ??= new UserMenu(this, context, configuration);

}
