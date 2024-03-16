using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using sshmanager.CompiledModels;
using sshmanager.Models;

namespace sshmanager;
public class DatabaseContext : DbContext
{
    public DbSet<Server> Servers { get; set; } = default!;
    public DbSet<User> Users { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder options_builder) {
        base.OnConfiguring(options_builder);
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "sshmanager");
        if (!Path.Exists(path)) {
            Directory.CreateDirectory(path);
        }

        options_builder
            .EnableSensitiveDataLogging()
            .UseModel(DatabaseContextModel.Instance)
            .UseSqlite($"Data Source={Path.Combine(path, "Database.db")}");
    }
}

//To update the Compiled state run: dotnet ef dbcontext optimize