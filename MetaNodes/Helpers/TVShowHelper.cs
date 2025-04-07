using System.Text.RegularExpressions;
using FileFlows.Plugin;
using FileFlows.Plugin.Helpers;

namespace MetaNodes.Helpers;

public class TVShowHelper(NodeParameters args)
{
    internal (string? LookupName, string? Year) GetLookupName(string filename, bool useFolderName)
    {
        string lookupName;
        if (useFolderName)
        {
            lookupName = FileHelper.GetDirectoryName(filename);
            if (Regex.IsMatch(lookupName, "^(Season|Staffel|Saison|Specials|S[0-9]+)", RegexOptions.IgnoreCase))
                lookupName = FileHelper.GetDirectoryName(FileHelper.GetDirectory(filename));
        }
        else
        {
            lookupName = FileHelper.GetShortFileNameWithoutExtension(filename);
        }

       // lookupName = lookupName.Replace("!", "");
        
        var result = GetTVShowInfo(lookupName);
        return (result.ShowName, result.Year);
    }
    
    /// <summary>
    /// Gets the tv show name from the text
    /// </summary>
    /// <param name="text">the input text</param>
    /// <returns>the tv show name</returns>
    public (string ShowName, int? Season, int? Episode, int? LastEpisode, string? Year) GetTVShowInfo(string text)
    {
        // Replace "1x02" format with "s1e02"
        text = Regex.Replace(text, @"(?<season>\d+)x(?<episode>\d+)", "s${season}e${episode}", RegexOptions.IgnoreCase);
        // Replace "s01.02" or "s01.e02" with "s01e02"
        text = Regex.Replace(text, @"(?<season>s\d+)[\.\s]?(e)?(?<episode>\d+)", "${season}e${episode}", RegexOptions.IgnoreCase);

        // this removes any {tvdb-123456} etc
        string variableMatch = @"\{[a-zA-Z]+-[0-9]+\}";
        // Replace the matched pattern with an empty string while ensuring that spaces around it are collapsed
        text = Regex.Replace(text, variableMatch, m =>
        {
            // If there are spaces before and after, replace with a single space
            if (Regex.IsMatch(m.Value, @"^\s*\{\w+-\d+\}\s*$"))
                return " ";
            // Otherwise, collapse completely without adding spaces
            return "";
        });
        // Trim any leading or trailing spaces
        text = text.Trim();
        
        
        // string year = null;
        // var reYear = Regex.Match(text, @"\((19|20)[\d]{2}\)", RegexOptions.CultureInvariant);
        // if (reYear.Success)
        // {
        //     year = reYear.Value;
        //     text = text.Replace(year, string.Empty);
        //     year = year[1..^1]; // remove the ()
        // }
        (text, var year) = ExtractYearAndCleanText(text);
        
        
        string pattern = @"^(?<showName>[\w\s',&$!.-]+)[. _-]?(?:(s|S)(?<season>\d+)(e|E)(?<episode>\d+)(?:-(?<lastEpisode>\d+))?)";

        Match match = Regex.Match(text, pattern);

        if (match.Success == false)
        {
            Match seasonMatch = Regex.Match(text, @"[\s\.][s][\d]{1,2}[\.ex]", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            if (seasonMatch.Success)
            {
                text = text[..seasonMatch.Index];  // Get the text before the match
            }
            
            text = Regex.Replace(text, @"\[[^\]]+\]", string.Empty);
            text = text.TrimExtra("-");
            return (text, null, null, null, year);
        }

        string show = match.Groups["showName"].Value.Replace(".", " ").TrimEnd();
        show = Regex.Replace(show, @"\[[^\]]+", string.Empty);
        show = show.TrimExtra("-");
        int season = int.Parse(match.Groups["season"].Value);
        int episode = int.Parse(match.Groups["episode"].Value);
        string lastEpisodeStr = match.Groups["lastEpisode"].Value;
        int? lastEpisode = string.IsNullOrEmpty(lastEpisodeStr) ? (int?)null : int.Parse(lastEpisodeStr);

        return (show, season, episode, lastEpisode, year);
    }
    
    
    /// <summary>
    /// Extracts the year from the given text if it matches specific patterns and removes it from the text.
    /// </summary>
    /// <param name="text">The input text containing a potential year.</param>
    /// <returns>
    /// A tuple containing the cleaned text and the extracted year.
    /// The year is extracted only if it falls between 1950 and 5 years from the current year.
    /// </returns>
    (string cleanedText, string? year) ExtractYearAndCleanText(string text)
    {
        string year = null;
        int currentYear = DateTime.Now.Year;
        int upperYearLimit = currentYear + 5;
        var cleanedText = text;

        // Match year in parentheses (e.g., (2024))
        var reYear = Regex.Match(text, $@"(19[5-9]\d|20[0-{upperYearLimit % 100 / 10}]\d|{upperYearLimit})(?=[^\d]|$)", RegexOptions.CultureInvariant);
        if (reYear.Success)
        {
            year = reYear.Value;
            cleanedText = text.Replace(year, string.Empty).Replace("()", "");
        }
        else
        {
            // Match dot-separated year (e.g., .2024.)
            var reYearAlt = Regex.Match(text, $@"\.(19[5-9]\d|20[0-{upperYearLimit % 100 / 10}]\d|{upperYearLimit})\.", RegexOptions.CultureInvariant);
            if (reYearAlt.Success)
            {
                year = reYearAlt.Value.Trim('.');
                cleanedText = text.Replace(reYearAlt.Value, string.Empty).Replace("..", ".");
            }
        }

        // Validate the extracted year
        if (year != null)
        {
            int yearInt = int.Parse(year);
            if (yearInt < 1950 || yearInt > upperYearLimit)
            {
                year = null;
                cleanedText = text; // restore it
            }
            else
            {
                cleanedText = cleanedText.Replace("  ", " ").Replace("..", ".");
            }
        }

        return (cleanedText, year);
    }
}
