namespace sshmanager.Models;

public class User {
    public Guid Id { get; set; } = Guid.NewGuid();
    public Server Server { get; set; } = new(); 
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public override string? ToString() {
        return Username;
    }
}