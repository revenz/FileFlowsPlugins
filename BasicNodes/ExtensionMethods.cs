namespace FileFlows.BasicNodes;

internal static class ExtensionMethods
{
    public static string? EmptyAsNull(this string str)
    {
        return str == string.Empty ? null : str;
    }
    public static void AddOrUpdate(this Dictionary<string, object> dict, string key, object value)
    {
        if (dict.ContainsKey(key))
            dict[key] = value;
        else
            dict.Add(key, value);
    }
}
