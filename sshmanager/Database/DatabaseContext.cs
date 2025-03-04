using System.Data.Common;

namespace sshmanager.Database;
public class DatabaseContext : IAsyncDisposable, IDisposable
{
    private readonly DbConnection connection;

    public ServerContext Servers => new(connection);
    public UserContext Users => new(connection);

    private DatabaseContext(DbConnection connection) => this.connection = connection;

    public static async Task<DatabaseContext> CreateContext(DbConnection connection) {
        DatabaseContext context = new(connection);
        await DatabaseBuilder.BuildDatabase(connection);

        return context;
    }

    public void Dispose() {
        connection.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync() {
        await connection.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}

public static class DatabaseBuilder
{
    public static async Task BuildDatabase(DbConnection connection) {

        if (!Directory.Exists(Constants.DATABASE_DIRECTORY))
            Directory.CreateDirectory(Constants.DATABASE_DIRECTORY);

        if (!await TableExists(connection, "Servers")) {
            await BuildServerTable(connection);
        }

        if (!await TableExists(connection, "Users")) {
            await BuildUserTable(connection);
        }
    }

    private static async Task<bool> TableExists(DbConnection connection, string table_name) {
        await connection.OpenAsync();

        using DbCommand command = connection.CreateCommand();
        command.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{table_name}';";

        using DbDataReader reader = await command.ExecuteReaderAsync();
        return reader.HasRows;
    }

    public static async Task BuildServerTable(DbConnection connection) {
        await connection.OpenAsync();

        using DbCommand command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE Servers (
                Id TEXT PRIMARY KEY,
                Name TEXT
            );
            """;

        await command.ExecuteNonQueryAsync();
    }

    public static async Task BuildUserTable(DbConnection connection) {
        await connection.OpenAsync();

        using DbCommand command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE Users (
                Id TEXT PRIMARY KEY,
                ServerId TEXT,
                Username TEXT,
                Password TEXT,
                FOREIGN KEY (ServerId) REFERENCES Servers(Id) ON DELETE CASCADE
            );
            """;

        await command.ExecuteNonQueryAsync();
    }
}