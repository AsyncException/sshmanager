using sshmanager.Models;
using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace sshmanager.Database;

public class UserContext(DbConnection connection)
{
    public async Task<IEnumerable<User>> Get(Server server)
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

    public async Task<IEnumerable<User>> GetLike(Server server, string user) {
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

    public async Task<bool> Remove(User user)
    {
        await connection.OpenAsync();
        
        using DbCommand command = connection.CreateCommand();
        command.CommandText = "DELETE from Users where Id=@Id";
        command.Parameters.Add(new SqliteParameter("@Id", user.Id));

        return await command.ExecuteNonQueryAsync() == 1;
    }
}