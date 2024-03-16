using System.Data.Common;

namespace sshmanager.Database;
public class DatabaseContext(DbConnection connection) : IAsyncDisposable, IDisposable
{
    private readonly DbConnection connection = connection;

    public ServerContext Servers => new(connection);
    public UserContext Users => new(connection);
    public Database Database => new(connection);

    public void Dispose() {
        connection.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync() {
        await connection.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}