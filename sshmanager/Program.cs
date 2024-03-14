using Microsoft.Extensions.Configuration;
using sshmanager;
using System.IO;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"), false)
.Build();

if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database.db"))) {
    throw new Exception("Datbase not found");
}

DatabaseContext context = new();
MenuManager.Configuration = config;
MenuManager.PresentServerList(context);