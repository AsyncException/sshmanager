using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using sshmanager;
using sshmanager.Models;
using sshmanager.Database;

string connection_string = $"Data Source={Path.Combine(Constants.DATABASE_DIRECTORY, "Database.db")}";
using (SqliteConnection connection = new(connection_string))
using (DatabaseContext context = await DatabaseContext.CreateContext(connection)) {

    ReturnType result = ArgumentHandler.Handle(args);

    IConfiguration config = new ConfigurationBuilder()
        .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"), false)
        .Build();

    if (result == ReturnType.Other) {
        await new MenuProvider(context, config).PointerMenu.ShowMenu(ArgumentHandler.GeneratePointer(args));
    }
    else {
        await new MenuProvider(context, config).MainMenu.ShowMenu();
    }

}