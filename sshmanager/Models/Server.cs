namespace sshmanager.Models;

public class Server
{
    /// <summary>
    /// Database Id of the server
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The name or ip of the server
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public override string? ToString() {
        return Name;
    }
}