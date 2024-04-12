namespace MetaNodes;

/// <summary>
/// Extension methods
/// </summary>
internal static class ExtensionMethods
{
    /// <summary>
    /// Returns an empty string as null, otherwise returns the original string
    /// </summary>
    /// <param name="str">the input string</param>
    /// <returns>the string or null if empty</returns>
    public static string? EmptyAsNull(this string str)
    {
        return str == string.Empty ? null : str;
    }
    
    /// <summary>
    /// Trims the specified characters from the beginning and end of the string.
    /// </summary>
    /// <param name="input">The input string to trim.</param>
    /// <param name="charsToTrim">The characters to trim from the string.</param>
    /// <returns>A new string that has the specified characters removed from the beginning and end.</returns>
    /// <exception cref="ArgumentNullException">Thrown when input is null.</exception>
    public static string TrimExtra(this string input, string charsToTrim)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));
        
        if (string.IsNullOrEmpty(charsToTrim))
            return input.Trim();

        int startIndex = 0;
        int endIndex = input.Length - 1;

        while (startIndex <= endIndex && (input[startIndex] == ' ' || charsToTrim.Contains(input[startIndex])))
        {
            startIndex++;
        }

        while (endIndex >= startIndex && (input[endIndex] == ' ' || charsToTrim.Contains(input[endIndex])))
        {
            endIndex--;
        }

        return input.Substring(startIndex, endIndex - startIndex + 1);
    }
}
