using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sshmanager.Models;
public enum ReturnType
{
    Break,
    Return,
    Other
}

public static class ReturnTypeExtensions {
    public static ReturnType FromVoid(this ReturnType type, Action action) {
        action();
        return type;
    }
}
