using sshmanager.Models;
using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace sshmanager.Database;

public class ServerContext(DbConnection connection)
{
    public async Task<IEnumerable<Server>> Get() {
        await connection.OpenAsync();

        using DbCommand command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name FROM Servers";

        using DbDataReader reader = await command.ExecuteReaderAsync();
        if (!reader.HasRows) {
            return [];
        }

        List<Server> servers = [];
        while (await reader.ReadAsync()) {
            servers.Add(new Server() {
                Id = reader.GetGuid(0),
                Name = reader.GetString(1),
            });
        }

        return servers;
    }

    public async Task<bool> Add(Server server)
    {
        await connection.OpenAsync();

        using DbCommand command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Servers VALUES (@Id, @Name)";
        command.Parameters.Add(new SqliteParameter("@Id", server.Id));
        command.Parameters.Add(new SqliteParameter("@Name", server.Name));

        return await command.ExecuteNonQueryAsync() == 1;
    }

    public async Task<bool> Remove(Server server)
    {
        await connection.OpenAsync();

        using DbCommand command = connection.CreateCommand();
        command.CommandText = "DELETE from Servers where Id=@Id";
        command.Parameters.Add(new SqliteParameter("@Id", server.Id));

        return await command.ExecuteNonQueryAsync() == 1;
    }
}
