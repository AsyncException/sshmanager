using Microsoft.Extensions.Configuration;
using sshmanager;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"), false)
    .Build();

DatabaseContext context = new();
context.Database.EnsureCreated();

MenuProvider provider = new(context, config);
provider.MainMenu.ShowMenu();