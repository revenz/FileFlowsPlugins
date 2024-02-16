namespace FileFlows.VideoNodes.Helpers;

/// <summary>
/// General helper
/// </summary>
public class GeneralHelper
{
    /// <summary>
    /// Checks if the input string represents a regular expression.
    /// </summary>
    /// <param name="input">The input string to check.</param>
    /// <returns>True if the input is a regular expression, otherwise false.</returns>
    public static bool IsRegex(string input)
    {
        return new[] { "?", "|", "^", "$", "*" }.Any(ch => input.Contains(ch));
    }
}