using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using sshmanager;
using sshmanager.Models;
using sshmanager.Database;

string connection_string = $"Data Source={Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "sshmanager", "Database.db")}";
using (DatabaseContext context = new(new SqliteConnection(connection_string))) {

    ReturnType result = await new ArgumentHandler(context).Handle(args);
    if (result == ReturnType.Return)
        return;

    IConfiguration config = new ConfigurationBuilder()
        .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"), false)
        .Build();

    if (result == ReturnType.Other) {
        await new MenuProvider(context, config).PointerMenu.ShowMenu(new ArgumentHandler(context).GeneratePointer(args));
    }
    else {
        await new MenuProvider(context, config).MainMenu.ShowMenu();
    }

}