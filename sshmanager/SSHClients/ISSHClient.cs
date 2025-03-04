using Microsoft.Extensions.Configuration;
using Spectre.Console;
using sshmanager.Models;

namespace sshmanager.SSHClients;
internal interface ISSHClient
{
    /// <summary>
    /// Start an ssh session with the specific client
    /// </summary>
    /// <param name="server">The server to connect to</param>
    /// <param name="user">The user to connect with</param>
    public void StartSession(Server server, User user);

    /// <summary>
    /// Generate an client from the configuration file
    /// </summary>
    /// <param name="configuration">The app configuration</param>
    /// <returns>The constructed SSHClient</returns>
    public static ISSHClient FromConfig(IConfiguration configuration) {
        return (configuration["Terminal"] ?? string.Empty) switch {
            "WindowsTerminal" => new WindowsTerminalClient(configuration),
            "Putty" => new PuttyClient(configuration),
            _ => new InvalidClient(configuration),
        };
    }
}

/// <summary>
/// An invalid client type if the configuration is incorrect.
/// </summary>
/// <param name="configuration"></param>
internal class InvalidClient(IConfiguration configuration) : ISSHClient
{
    private readonly IConfiguration configuration = configuration;
    public void StartSession(Server server, User user) {
        AnsiConsole.MarkupLine($"[red]Selected terminal is not a valid option. Selection: {(configuration["Terminal"] ?? string.Empty)}[/]");
    }
}