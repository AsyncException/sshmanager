using sshmanager.Models;
using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace sshmanager.Database;

public class UserContext(DbConnection connection)
{
    /// <summary>
    /// Gets the users for a specific server
    /// </summary>
    /// <param name="server">The server where the users are from</param>
    /// <returns>The list of users</returns>
    public async Task<List<User>> Get(Server server)
    {
        await connection.OpenAsync();
        
        using DbCommand command = connection.CreateCommand();
        command.CommandText = "select * from Users where ServerId=@Id";
        command.Parameters.Add(new SqliteParameter("@Id", server.Id));

        using DbDataReader reader = await command.ExecuteReaderAsync();

        if (!reader.HasRows)
            return [];

        List<User> users = [];
        while (await reader.ReadAsync())
        {
            users.Add(new User()
            {
                Id = reader.GetGuid(0),
                Server = new() { Id = reader.GetGuid(1) },
                Username = reader.GetString(2),
                Password = reader.GetString(3)
            });
        }

        return users;
    }

    /// <summary>
    /// Gets the users where the name is alike <paramref name="user"/>
    /// </summary>
    /// <param name="server">The server to get the users from</param>
    /// <param name="user">The name to search for</param>
    /// <returns>The list of users</returns>
    public async Task<List<User>> GetLike(Server server, string user) {
        await connection.OpenAsync();

        using DbCommand command = connection.CreateCommand();
        command.CommandText = "select * from Users WHERE ServerId=@Id AND Username LIKE @searchquery";
        command.Parameters.Add(new SqliteParameter("@Id", server.Id));
        command.Parameters.Add(new SqliteParameter("@searchquery", $"%{user}%"));

        using DbDataReader reader = await command.ExecuteReaderAsync();

        if (!reader.HasRows)
            return [];

        List<User> users = [];
        while (await reader.ReadAsync()) {
            users.Add(new User() {
                Id = reader.GetGuid(0),
                Server = new() { Id = reader.GetGuid(1) },
                Username = reader.GetString(2),
                Password = reader.GetString(3)
            });
        }

        return users;
    }

    /// <summary>
    /// Add a user to the database
    /// </summary>
    /// <param name="user">The user to add</param>
    /// <returns>A <see cref="bool"/> true if the action succeeded</returns>
    public async Task<bool> Add(User user)
    {
        await connection.OpenAsync();

        using DbCommand command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Users VALUES (@Id, @ServerId, @Username, @Password)";
        command.Parameters.Add(new SqliteParameter("@Id", user.Id));
        command.Parameters.Add(new SqliteParameter("@ServerId", user.Server.Id));
        command.Parameters.Add(new SqliteParameter("@Username", user.Username));
        command.Parameters.Add(new SqliteParameter("@Password", user.Password));

        return await command.ExecuteNonQueryAsync() == 1;
    }

    /// <summary>
    /// Removes a user from the database
    /// </summary>
    /// <param name="user">The user to remove</param>
    /// <returns>A <see cref="bool"/> true if the action succeeded</returns>
    public async Task<bool> Remove(User user)
    {
        await connection.OpenAsync();
        
        using DbCommand command = connection.CreateCommand();
        command.CommandText = "DELETE from Users where Id=@Id";
        command.Parameters.Add(new SqliteParameter("@Id", user.Id));

        return await command.ExecuteNonQueryAsync() == 1;
    }
}