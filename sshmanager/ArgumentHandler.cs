using Spectre.Console;
using sshmanager.Database;
using sshmanager.Models;
using sshmanager.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sshmanager;
public class ArgumentHandler
{
    public static ReturnType Handle(string[] args) => args switch {
        { Length: 0 } => ReturnType.Break,
        { Length: > 0 } => ReturnType.Other,
    };

    public static DestinationPointer GeneratePointer(string[] args) => args switch {
        { Length: 0 or > 2 } => throw new Exception(),
        { Length: 1 } => new DestinationPointer(args[0], null),
        { Length: 2 } => new DestinationPointer(args[0], args[1])
    };
}

public readonly struct DestinationPointer(string server, string? user)
{
    private readonly string _server = server ?? string.Empty;
    private readonly string _user = user ?? string.Empty;

    public async Task<(Option<Server> server, Option<User> user)> GetClosest(DatabaseContext context) {
        if (string.IsNullOrEmpty(_server)) {
            return (Option.None, Option.None);
        }

        Option<Server> server = await GetClosestServer(context, _server);

        if(!server.HasValue || string.IsNullOrEmpty(_user)) {
            return (server, Option.None);
        }

        Option<User> user = await GetClosestUser(context, server.Value, _user);

        return (server, user);
    }

    private static async Task<Server> GetClosestServer(DatabaseContext context, string serverName) {
        IEnumerable<Server> servers = await context.Servers.GetLike(serverName);
        Server? server = servers.FirstOrDefault();
        return Option.FromNull(server);
    }

    private static async Task<User> GetClosestUser(DatabaseContext context, Server server, string userName) {
        IEnumerable<User> users = await context.Users.GetLike(server, userName);
        User? user = users.FirstOrDefault();
        return Option.FromNull(user);
    }
}