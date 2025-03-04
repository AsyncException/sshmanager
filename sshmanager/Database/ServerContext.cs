using sshmanager.Models;
using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace sshmanager.Database;

public class ServerContext(DbConnection connection)
{
    /// <summary>
    /// Get all the servers in the database.
    /// </summary>
    /// <returns>The list of games</returns>
    public async Task<List<Server>> Get() {
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

    /// <summary>
    /// Searches the database for servers with a name like <paramref name="server"/>
    /// </summary>
    /// <param name="server">The name to search for</param>
    /// <returns>List of servers with a matching name</returns>
    public async Task<List<Server>> GetLike(string server) {
        await connection.OpenAsync();

        using DbCommand command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name FROM Servers WHERE (Name LIKE @searchquery)";
        command.Parameters.Add(new SqliteParameter("@searchquery", $"%{server}%"));

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

    /// <summary>
    /// Adds a server to the database
    /// </summary>
    /// <param name="server">The server to add</param>
    /// <returns>A <see cref="bool"/> true if the action succeeded</returns>
    public async Task<bool> Add(Server server)
    {
        await connection.OpenAsync();

        using DbCommand command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Servers VALUES (@Id, @Name)";
        command.Parameters.Add(new SqliteParameter("@Id", server.Id));
        command.Parameters.Add(new SqliteParameter("@Name", server.Name));

        return await command.ExecuteNonQueryAsync() == 1;
    }

    /// <summary>
    /// Removes a server from the database
    /// </summary>
    /// <param name="server">The server to remove</param>
    /// <returns>A <see cref="bool"/> true if the action succeeded</returns>
    public async Task<bool> Remove(Server server)
    {
        await connection.OpenAsync();

        using DbCommand command = connection.CreateCommand();
        command.CommandText = "DELETE from Servers where Id=@Id";
        command.Parameters.Add(new SqliteParameter("@Id", server.Id));

        return await command.ExecuteNonQueryAsync() == 1;
    }
}
