using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sshmanager.Models;
public class Server
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public List<User> Usernames { get; set; } = [];
}

public class User {
    public Guid Id { get; set; } = Guid.NewGuid();
    public Server Server { get; set; } = new(); 
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}