namespace sshmanager.Models;

//A followup action to convey upwards a method chain
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
