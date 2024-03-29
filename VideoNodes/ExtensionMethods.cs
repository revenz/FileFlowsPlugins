﻿namespace FileFlows.VideoNodes
{
    using System.Linq;
    using System.Text.RegularExpressions;

    internal static class ExtensionMethods
    {
        public static void AddOrUpdate(this Dictionary<string, object> dict, string key, object value) {
            if (dict.ContainsKey(key))
                dict[key] = value;
            else
                dict.Add(key, value);
        }
        public static string? EmptyAsNull(this string str)
        {
            return str == string.Empty ? null : str;
        }

        public static bool TryMatch(this Regex regex, string input, out Match match)
        {
            match = regex.Match(input);
            return match.Success;
        }

        public static IEnumerable<string> SplitCommandLine(this string commandLine)
        {
            bool inQuotes = false;

            return commandLine.Split(c =>
            {
                if (c == '\"')
                    inQuotes = !inQuotes;

                return !inQuotes && c == ' ';
            })
            .Select(arg => arg.Trim().TrimMatchingQuotes('\"'))
            .Where(arg => !string.IsNullOrEmpty(arg));
        }
        public static IEnumerable<string> Split(this string str,
                                          Func<char, bool> controller)
        {
            int nextPiece = 0;

            for (int c = 0; c < str.Length; c++)
            {
                if (controller(str[c]))
                {
                    yield return str.Substring(nextPiece, c - nextPiece);
                    nextPiece = c + 1;
                }
            }

            yield return str.Substring(nextPiece);
        }
        public static string TrimMatchingQuotes(this string input, char quote)
        {
            if ((input.Length >= 2) &&
                (input[0] == quote) && (input[input.Length - 1] == quote))
                return input.Substring(1, input.Length - 2);

            return input;
        }
    }
}
