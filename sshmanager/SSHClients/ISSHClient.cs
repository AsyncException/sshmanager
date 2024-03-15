using Microsoft.Extensions.Configuration;
using Spectre.Console;
using sshmanager.Models;

namespace sshmanager.SSHClients;
internal interface ISSHClient
{
    public void StartSession(Server server, User user);

    public static ISSHClient FromConfig(IConfiguration configuration) {
        return (configuration["Terminal"] ?? string.Empty) switch {
            "WindowsTerminal" => new WindowsTerminalClient(configuration),
            "Putty" => new PuttyClient(configuration),
            _ => new InvalidClient(configuration),
        };
    }
}

internal class InvalidClient(IConfiguration configuration) : ISSHClient
{
    private readonly IConfiguration configuration = configuration;
    public void StartSession(Server server, User user) {
        AnsiConsole.MarkupLine($"[red]Selected terminal is not a valid option. Selection: {(configuration["Terminal"] ?? string.Empty)}[/]");
    }
}