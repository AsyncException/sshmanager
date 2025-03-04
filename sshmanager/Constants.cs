namespace sshmanager;
internal static class Constants
{
    public const string SEPERATOR = "----------";
    public const string ADD_SERVER = "Add server";
    public const string DELETE_SERVER = "Delete Server";
    public const string ADD_USER = "Add user";
    public const string DELETE_USER = "Delete user";
    public const string CONNECT = "Connect";
    public const string COPY_PASSWORD = "Copy password";
    public const string RETURN = "Return";
    public const string EXIT = "Exit";
    public static string DATABASE_DIRECTORY = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "sshmanager");
}
