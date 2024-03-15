using Microsoft.Extensions.Configuration;
using Spectre.Console;
using sshmanager.Models;
using System.Diagnostics;
using System.Text;

namespace sshmanager.SSHClients;
internal class PuttyClient(IConfiguration configuration) : ISSHClient
{
    private readonly IConfiguration configuration = configuration;
    public void StartSession(Server server, User user) {
        string file_path = configuration["PuttySettings:Path"] ?? "C:\\Program Files\\PuTTY\\putty.exe";
        Process ssh_process = new() {
            StartInfo = new ProcessStartInfo() {
                FileName = file_path,
                Arguments = CreateArguments(configuration, server, user),
                RedirectStandardInput = false,
                RedirectStandardOutput = false,
                UseShellExecute = true,
                CreateNoWindow = false
            }
        };

        try {
            ssh_process.Start();
        }
        catch (Exception ex) {
            AnsiConsole.MarkupLine("[red]Error starting SSH process[/]");
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            Console.Read();
        }
    }

    private static string CreateArguments(IConfiguration configuration, Server server, User user) {
        StringBuilder argument_builder = new();
        argument_builder.Append($"{user.Username}@{server.Name}");

        if(!bool.TryParse(configuration["PuttySettings:IncludePassword"], out bool include_password))
            include_password = true;

        if (include_password && !string.IsNullOrEmpty(user.Password)) {
            argument_builder.Append($" -pw {user.Password}");
        }

        return argument_builder.ToString();
    }
}
