using Microsoft.Extensions.Configuration;
using sshmanager;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"), false)
    .Build();

DatabaseContext context = new();
context.Database.EnsureCreated();
MenuManager.Configuration = config;
MenuManager.PresentServerList(context);