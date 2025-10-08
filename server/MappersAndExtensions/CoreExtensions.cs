namespace server.MappersAndExtensions;

public static class CoreExtensions
{
    public static int IndexValue (this HashSet<string> hash, string value)
    {
        return hash.ToList().IndexOf(value);
    }
}
