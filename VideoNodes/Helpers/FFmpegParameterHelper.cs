using System.Text;

namespace FileFlows.VideoNodes.Helpers;

/// <summary>
/// Helper class for processing FFmpeg parameter replacements.
/// </summary>
public static class FFmpegParameterHelper
{
    /// <summary>
    /// Applies parameter replacements to a list of FFmpeg arguments, optionally logging changes.
    /// </summary>
    /// <param name="ffArgs">The original list of FFmpeg arguments.</param>
    /// <param name="parameterReplacements">
    /// A list of key-value pairs where the key is the FFmpeg parameter to replace, and the value is:
    /// <list type="bullet">
    /// <item><description>An empty or null string to remove the parameter and its value.</description></item>
    /// <item><description>A string (possibly with spaces) to replace the value(s) of the parameter.</description></item>
    /// </list>
    /// </param>
    /// <param name="logger">An optional logger used to record each replacement operation.</param>
    /// <returns>A new list of FFmpeg arguments with replacements applied.</returns>
    public static List<string> ApplyParameterReplacements(
        List<string> ffArgs,
        List<KeyValuePair<string, string>> parameterReplacements,
        ILogger logger = null)
    {
        if (ffArgs == null || parameterReplacements == null || parameterReplacements.Count == 0)
            return ffArgs;

        var updatedArgs = new List<string>(ffArgs);

        foreach (var replacement in parameterReplacements)
        {
            var keyTokens = TokenizeWithQuotes(replacement.Key);
            if (keyTokens.Count == 0)
                continue;

            int index = FindSequenceIndex(updatedArgs, keyTokens);
            while (index != -1)
            {
                if (string.IsNullOrWhiteSpace(replacement.Value))
                {
                    updatedArgs.RemoveRange(index, keyTokens.Count);
                    logger?.ILog($"Removed FFmpeg parameter '{replacement.Key}'");
                    // Continue search at same index (list shrunk)
                    index = FindSequenceIndex(updatedArgs, keyTokens, index);
                }
                else
                {
                    var newTokens = TokenizeWithQuotes(replacement.Value);
                    updatedArgs.RemoveRange(index, keyTokens.Count);
                    updatedArgs.InsertRange(index, newTokens);
                    logger?.ILog($"Replaced FFmpeg parameter '{replacement.Key}' with '{replacement.Value}'");
                    // Continue search after inserted tokens to avoid infinite loop
                    index = FindSequenceIndex(updatedArgs, keyTokens, index + newTokens.Count);
                }
            }
        }

        return updatedArgs;
    }


    /// <summary>
    /// Finds the index of the first occurrence of a token sequence in the list, starting at optional startIndex.
    /// Returns -1 if not found.
    /// </summary>
    private static int FindSequenceIndex(List<string> list, List<string> sequence, int startIndex = 0)
    {
        for (int i = startIndex; i <= list.Count - sequence.Count; i++)
        {
            bool match = true;
            for (int j = 0; j < sequence.Count; j++)
            {
                if (list[i + j] != sequence[j])
                {
                    match = false;
                    break;
                }
            }

            if (match)
                return i;
        }

        return -1;
    }


    /// <summary>
    /// Splits a string into tokens separated by spaces, but respects double quotes as grouping tokens.
    /// For example, input: a b "this is one with spaces" c
    /// returns: [a, b, this is one with spaces, c]
    /// </summary>
    /// <param name="input">The input string to tokenize</param>
    /// <returns>List of tokens</returns>
    private static List<string> TokenizeWithQuotes(string input)
    {
        var tokens = new List<string>();
        if (string.IsNullOrWhiteSpace(input))
            return tokens;

        var currentToken = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue; // Skip the quote character
            }

            if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (currentToken.Length > 0)
                {
                    tokens.Add(currentToken.ToString());
                    currentToken.Clear();
                }
            }
            else
            {
                currentToken.Append(c);
            }
        }

        if (currentToken.Length > 0)
            tokens.Add(currentToken.ToString());

        return tokens;
    }
}