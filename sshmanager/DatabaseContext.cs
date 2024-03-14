using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using sshmanager.Models;

namespace sshmanager;
public class DatabaseContext : DbContext
{
    public DbSet<Server> Servers { get; set; } = default!;
    public DbSet<User> Users { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder options_builder) {
        base.OnConfiguring(options_builder);
        string path = AppDomain.CurrentDomain.BaseDirectory;

        options_builder.UseSqlite($"Data Source={Path.Combine(path, "Database.db")}");
    }
}
