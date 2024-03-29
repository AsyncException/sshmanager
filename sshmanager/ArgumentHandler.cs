﻿using Spectre.Console;
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
        ["--Initialize"] => await Initialize(),
        [..] => InvalidOption(),
    };

    private async Task<ReturnType> Initialize() {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        await context.Database.BuildDatabase();
        return ReturnType.Return;
    }

    private static ReturnType InvalidOption() {
        AnsiConsole.WriteException(new Exception("Invalid argument: report this issue on github"), ExceptionFormats.ShortenEverything);
        Environment.Exit(0);

        return ReturnType.Return;
    }
}
