namespace FileFlows.DiscordNodes;

/// <summary>
/// Extension methods
/// </summary>
internal static class ExtensionMethods
{
    /// <summary>
    /// Treats an empty string as if it was null
    /// </summary>
    /// <param name="str">the input string</param>
    /// <returns>the string unless it was empty then null</returns>
    public static string? EmptyAsNull(this string str) => str == string.Empty ? null : str;
}
