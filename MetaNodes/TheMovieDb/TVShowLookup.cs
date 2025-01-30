using System.Text.RegularExpressions;
using DM.MovieApi;
using DM.MovieApi.ApiResponse;
using DM.MovieApi.MovieDb.Movies;
using DM.MovieApi.MovieDb.TV;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using FileFlows.Plugin.Helpers;
using MetaNodes.Helpers;
using Newtonsoft.Json;

namespace MetaNodes.TheMovieDb;

/// <summary>
/// TV Show Lookup 
/// </summary>
public class TVShowLookup : Node
{
    /// <summary>
    /// Gets the number of inputs
    /// </summary>
    public override int Inputs => 1;
    /// <summary>
    /// Gets the number of outputs
    /// </summary>
    public override int Outputs => 2;
    /// <summary>
    /// Gets the flow element type
    /// </summary>
    public override FlowElementType Type => FlowElementType.Logic;
    /// <summary>
    /// Gets the help URL
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/meta-nodes/tv-show-lookup";
    /// <summary>
    /// Gets the icon
    /// </summary>
    public override string Icon => "fas fa-tv";

    private Dictionary<string, object> _Variables;
    
    /// <summary>
    /// Gets the Variables this flow element provides
    /// </summary>
    public override Dictionary<string, object> Variables => _Variables;

    /// <summary>
    /// Constructs a new instance of this flow element
    /// </summary>
    public TVShowLookup()
    {
        _Variables = new Dictionary<string, object>()
        {
            { "tvshow.Title", "THe Batman" },
            { "tvshow.Year", 2004 }
        };
    }
    
    /// <summary>
    /// Gets or sets if the folder name should be used
    /// </summary>
    [Boolean(1)]
    public bool UseFolderName { get; set; }

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        var helper = new TVShowHelper(args);
        (string lookupName, string year) = helper.GetLookupName(args.LibraryFileName, UseFolderName);

        // RegisterSettings only needs to be called one time when your application starts-up.
        MovieDbFactory.RegisterSettings(Globals.MovieDbBearerToken);
        
        args.Logger?.ILog("Lookup TV Show: " + lookupName);

        string tvShowInfoCacheKey = $"TVShowInfo: {lookupName} ({year})";
        TVShowInfo result = args.Cache.GetObject<TVShowInfo>(tvShowInfoCacheKey);
        if (result != null)
        {
            args.Logger?.ILog("Got TV show info from cache: " + result.Name + "\n" + System.Text.Json.JsonSerializer.Serialize(result));
        }
        else
        {
            result = LookupShow(lookupName, year);

            if (result == null)
            {
                args.Logger?.ILog("No result found for: " + lookupName);
                return 2; // no match
            }
            string json = JsonConvert.SerializeObject(result, Formatting.Indented);
            args.Cache.SetJson(tvShowInfoCacheKey, json);
        }
        
        string tvShowCacheKey = $"TVShow: {result.Id}";
        TVShow? tv = args.Cache.GetObject<TVShow>(tvShowCacheKey);
        if (tv == null)
        {
            var tvApi = MovieDbFactory.Create<IApiTVShowRequest>().Value;
            tv = tvApi.FindByIdAsync(result.Id).Result?.Item;
            if (tv != null)
            {
                string json = JsonConvert.SerializeObject(result, Formatting.Indented);
                args.Cache.SetJson(tvShowCacheKey, json);
            }
        }
        else
        {
            args.Logger?.ILog("Got TV show from cache");
        }

        args.Logger?.ILog("Found TV Show: " + result.Name);
        args.SetParameter(Globals.TV_SHOW_INFO, result);

        Variables["tvshow.Title"] = result.Name;
        args.Logger?.ILog("Detected Title: " + result.Name);
        Variables["tvshow.Year"] = result.FirstAirDate.Year;
        args.Logger?.ILog("Detected Year: " + result.FirstAirDate.Year);
        Variables["VideoMetadata"] = GetVideoMetadata(tv, args.TempPath);
        Variables[Globals.TV_SHOW_INFO] = result;
        if (string.IsNullOrWhiteSpace(result.OriginalLanguage) == false)
        {
            Variables["OriginalLanguage"] = result.OriginalLanguage;
            args.Logger?.ILog("Detected Original Language: " + result.OriginalLanguage);
        }

        args.UpdateVariables(Variables);
        
        return 1;
    }
    //
    // internal static (string? LookupName, string? Year) GetLookupName(string filename, bool useFolderName)
    // {
    //     string lookupName;
    //     if (useFolderName)
    //     {
    //         lookupName = FileHelper.GetDirectoryName(filename);
    //         if (Regex.IsMatch(lookupName, "^(Season|Staffel|Saison|Specials)", RegexOptions.IgnoreCase))
    //             lookupName = FileHelper.GetDirectoryName(FileHelper.GetDirectory(filename));
    //     }
    //     else
    //     {
    //         lookupName = FileHelper.GetShortFileNameWithoutExtension(filename);
    //     }
    //     
    //     var result = GetTVShowInfo(lookupName);
    //     return (result.ShowName, result.Year);
    // }

    /// <summary>
    /// Looks up a show online
    /// </summary>
    /// <param name="lookupName">the lookup name</param>
    /// <param name="year">the year</param>
    /// <returns>the show if found</returns>
    private TVShowInfo LookupShow(string lookupName, string year)
    {
        
        var movieApi = MovieDbFactory.Create<IApiTVShowRequest>().Value;

        var response = movieApi.SearchByNameAsync(lookupName).Result;

        // try find an exact match
        var results = response.Results.OrderByDescending(x =>
            {
                if (string.IsNullOrEmpty(year) == false)
                {
                    if(year == x.FirstAirDate.Year.ToString())
                        return 2;
                    // sometimes the user may have hte date off by one, or the app may have 
                    if(year == (x.FirstAirDate.Year - 1).ToString())
                        return 1;
                    if(year == (x.FirstAirDate.Year + 1).ToString())
                        return 1;
                    return 0;
                }
                return 0;
            })
            .ThenBy(x => x.Name.ToLower().Trim().Replace(" ", "") == lookupName.ToLower().Trim().Replace(" ", "") ? 0 : 1)
            .ThenBy(x => lookupName.ToLower().Trim().Replace(" ", "").StartsWith(x.Name.ToLower().Trim().Replace(" ", "")) ? 0 : 1)
            // .ThenBy(x => x.Name)
            .ToList();
        
        return results.FirstOrDefault();
    }


    /// <summary>
    /// Gets the VideoMetadata
    /// </summary>
    /// <param name="tvApi">the tv API</param>
    /// <param name="id">the ID of the movie</param>
    /// <param name="tempPath">the temp path to save any images to</param>
    /// <returns>the VideoMetadata</returns>
    internal static VideoMetadata GetVideoMetadata(TVShow? tv, string tempPath)
    {
        if (tv == null)
            return null;

        VideoMetadata md = new();
        md.Title = tv.Name;
        md.Genres = tv.Genres?.Select(x => x.Name).ToList();
        md.Description = tv.Overview;
        md.Year = tv.FirstAirDate.Year;
        md.ReleaseDate = tv.FirstAirDate;
        md.OriginalLanguage = tv.OriginalLanguage;
        if (string.IsNullOrWhiteSpace(tv.PosterPath) == false)
        {
            try
            {
                using var httpClient = new HttpClient();
                using var stream = httpClient.GetStreamAsync("https://image.tmdb.org/t/p/w500" + tv.PosterPath).Result;
                string file = Path.Combine(tempPath, Guid.NewGuid() + ".jpg");
                using var fileStream = new FileStream(file, FileMode.CreateNew);
                stream.CopyTo(fileStream);
                md.ArtJpeg = file;
            }
            catch (Exception)
            {

            }
        }

        return md;
    }

    // /// <summary>
    // /// Gets the tv show name from the text
    // /// </summary>
    // /// <param name="text">the input text</param>
    // /// <returns>the tv show name</returns>
    // internal static (string ShowName, int? Season, int? Episode, int? LastEpisode, string? Year) GetTVShowInfo(string text)
    // {
    //     // Replace "1x02" format with "s1e02"
    //     text = Regex.Replace(text, @"(?<season>\d+)x(?<episode>\d+)", "s${season}e${episode}", RegexOptions.IgnoreCase);
    //     // Replace "s01.02" or "s01.e02" with "s01e02"
    //     text = Regex.Replace(text, @"(?<season>s\d+)[\.\s]?(e)?(?<episode>\d+)", "${season}e${episode}", RegexOptions.IgnoreCase);
    //
    //     // this removes any {tvdb-123456} etc
    //     string variableMatch = @"\{[a-zA-Z]+-[0-9]+\}";
    //     // Replace the matched pattern with an empty string while ensuring that spaces around it are collapsed
    //     text = Regex.Replace(text, variableMatch, m =>
    //     {
    //         // If there are spaces before and after, replace with a single space
    //         if (Regex.IsMatch(m.Value, @"^\s*\{\w+-\d+\}\s*$"))
    //             return " ";
    //         // Otherwise, collapse completely without adding spaces
    //         return "";
    //     });
    //     // Trim any leading or trailing spaces
    //     text = text.Trim();
    //     
    //     
    //     // string year = null;
    //     // var reYear = Regex.Match(text, @"\((19|20)[\d]{2}\)", RegexOptions.CultureInvariant);
    //     // if (reYear.Success)
    //     // {
    //     //     year = reYear.Value;
    //     //     text = text.Replace(year, string.Empty);
    //     //     year = year[1..^1]; // remove the ()
    //     // }
    //     (text, var year) = ExtractYearAndCleanText(text);
    //     
    //     
    //     string pattern = @"^(?<showName>[\w\s',&$.-]+)[. _-]?(?:(s|S)(?<season>\d+)(e|E)(?<episode>\d+)(?:-(?<lastEpisode>\d+))?)";
    //
    //     Match match = Regex.Match(text, pattern);
    //
    //     if (match.Success == false)
    //     {
    //         Match seasonMatch = Regex.Match(text, @"[\s\.][s][\d]{1,2}[\.ex]", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    //         if (seasonMatch.Success)
    //         {
    //             text = text[..seasonMatch.Index];  // Get the text before the match
    //         }
    //         
    //         text = Regex.Replace(text, @"\[[^\]]+\]", string.Empty);
    //         text = text.TrimExtra("-");
    //         return (text, null, null, null, year);
    //     }
    //
    //     string show = match.Groups["showName"].Value.Replace(".", " ").TrimEnd();
    //     show = Regex.Replace(show, @"\[[^\]]+", string.Empty);
    //     show = show.TrimExtra("-");
    //     int season = int.Parse(match.Groups["season"].Value);
    //     int episode = int.Parse(match.Groups["episode"].Value);
    //     string lastEpisodeStr = match.Groups["lastEpisode"].Value;
    //     int? lastEpisode = string.IsNullOrEmpty(lastEpisodeStr) ? (int?)null : int.Parse(lastEpisodeStr);
    //
    //     return (show, season, episode, lastEpisode, year);
    // }
    //
    //
    // /// <summary>
    // /// Extracts the year from the given text if it matches specific patterns and removes it from the text.
    // /// </summary>
    // /// <param name="text">The input text containing a potential year.</param>
    // /// <returns>
    // /// A tuple containing the cleaned text and the extracted year.
    // /// The year is extracted only if it falls between 1950 and 5 years from the current year.
    // /// </returns>
    // static (string cleanedText, string? year) ExtractYearAndCleanText(string text)
    // {
    //     string year = null;
    //     int currentYear = DateTime.Now.Year;
    //     int upperYearLimit = currentYear + 5;
    //     var cleanedText = text;
    //
    //     // Match year in parentheses (e.g., (2024))
    //     var reYear = Regex.Match(text, $@"(19[5-9]\d|20[0-{upperYearLimit % 100 / 10}]\d|{upperYearLimit})(?=[^\d]|$)", RegexOptions.CultureInvariant);
    //     if (reYear.Success)
    //     {
    //         year = reYear.Value;
    //         cleanedText = text.Replace(year, string.Empty).Replace("()", "");
    //     }
    //     else
    //     {
    //         // Match dot-separated year (e.g., .2024.)
    //         var reYearAlt = Regex.Match(text, $@"\.(19[5-9]\d|20[0-{upperYearLimit % 100 / 10}]\d|{upperYearLimit})\.", RegexOptions.CultureInvariant);
    //         if (reYearAlt.Success)
    //         {
    //             year = reYearAlt.Value.Trim('.');
    //             cleanedText = text.Replace(reYearAlt.Value, string.Empty).Replace("..", ".");
    //         }
    //     }
    //
    //     // Validate the extracted year
    //     if (year != null)
    //     {
    //         int yearInt = int.Parse(year);
    //         if (yearInt < 1950 || yearInt > upperYearLimit)
    //         {
    //             year = null;
    //             cleanedText = text; // restore it
    //         }
    //         else
    //         {
    //             cleanedText = cleanedText.Replace("  ", " ").Replace("..", ".");
    //         }
    //     }
    //
    //     return (cleanedText, year);
    // }
}