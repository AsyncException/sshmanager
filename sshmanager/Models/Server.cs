namespace sshmanager.Models;

public class Server
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;

    public override string? ToString() {
        return Name;
    }
}