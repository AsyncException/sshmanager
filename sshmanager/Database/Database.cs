using System.Data.Common;

namespace sshmanager.Database;

public class Database(DbConnection connection) {
    public async Task BuildDatabase() {
        if(await TableExists("Servers")) {
            await BuildServerTable();
        }

        if(await TableExists("Users")) {
            await BuildUserTable();
        }
    }

    private async Task<bool> TableExists(string table_name) {
        await connection.OpenAsync();

        using DbCommand command = connection.CreateCommand();
        command.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{table_name}';";
        
        using DbDataReader reader = await command.ExecuteReaderAsync();
        return reader.HasRows;
    }

    public async Task BuildServerTable() {
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

    public async Task BuildUserTable() {
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