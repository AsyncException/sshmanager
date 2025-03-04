using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sshmanager.Models;
public class Promptable<T> where T : new() {

    /// <summary>
    /// The construction for a value
    /// </summary>
    /// <param name="value"></param>
    public Promptable(T value)
    {
        IsOptions = false;
        OptionValue = string.Empty;
        Value = value;
    }

    /// <summary>
    /// Constructor for an option.
    /// </summary>
    /// <param name="value"></param>
    public Promptable(string value) {
        IsOptions = true;
        OptionValue = value;
        Value = new();
    }

    /// <summary>
    /// If the prompt is an option or if it contains a value.
    /// </summary>
    public bool IsOptions { get; set; }

    /// <summary>
    /// The option name if it is an option.
    /// </summary>
    public string OptionValue { get; set; }
    
    /// <summary>
    /// The value of the promptable if its not an option.
    /// </summary>
    public T Value { get; set; } = new();

    public override string? ToString() {
        if(IsOptions) {
            return OptionValue;
        }

        return Value?.ToString();
    }
}
