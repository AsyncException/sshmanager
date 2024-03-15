using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sshmanager.Models;
public class Promptable<T> where T : new() {

    public Promptable(T value)
    {
        IsOptions = false;
        OptionValue = string.Empty;
        Value = value;
    }
    public Promptable(string value) {
        IsOptions = true;
        OptionValue = value;
        Value = new();
    }

    public bool IsOptions { get; set; }
    public string OptionValue { get; set; }
    public T Value { get; set; } = new();

    public override string? ToString() {
        if(IsOptions) {
            return OptionValue;
        }

        return Value?.ToString();
    }
}
