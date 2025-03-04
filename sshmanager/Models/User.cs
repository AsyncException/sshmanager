namespace sshmanager.Models;

public class User {
    /// <summary>
    /// The database Id of the server
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The server this user belongs to
    /// </summary>
    public Server Server { get; set; } = new(); 

    /// <summary>
    /// The users username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The users password
    /// </summary>
    public string Password { get; set; } = string.Empty;

    public override string? ToString() {
        return Username;
    }
}