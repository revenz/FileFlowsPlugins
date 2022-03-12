namespace BasicNodes
{
    internal static class ExtensionMethods
    {
        public static string? EmptyAsNull(this string str)
        {
            return str == string.Empty ? null : str;
        }
    }
}
