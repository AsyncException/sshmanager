using Spectre.Console;
using sshmanager.Database;
using sshmanager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sshmanager;
public class ArgumentHandler(DatabaseContext context)
{
    private readonly DatabaseContext context = context;
    private readonly string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "sshmanager");

    public async Task<ReturnType> Handle(string[] args) => args switch {
        { Length: 0 } => ReturnType.Break,
        ["--initialize"] => await Initialize(),
        { Length: > 0 } => ReturnType.Other,
    };

    private async Task<ReturnType> Initialize() {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        await context.Database.BuildDatabase();
        return ReturnType.Return;
    }

    public DestinationPointer GeneratePointer(string[] args) => args switch {
        { Length: 0 or > 2 } => throw new Exception(),
        { Length: 1 } => new DestinationPointer(args[0], null),
        { Length: 2 } => new DestinationPointer(args[0], args[1])
    };
}

public record struct DestinationPointer(string Server, string? User)
{
    public async readonly Task<Server> GetClosestServer(DatabaseContext context) {
        return (await context.Servers.GetLike(Server)).FirstOrDefault() ?? throw new Exception("Invalid server option");
    }

    public async readonly Task<User> GetClosestUser(DatabaseContext context, Server server) {
        return (await context.Users.GetLike(server, User!)).FirstOrDefault() ?? throw new Exception("Invalid user option");
    }
}