using System.Text.RegularExpressions;
using DM.MovieApi;
using DM.MovieApi.ApiResponse;
using DM.MovieApi.MovieDb.Movies;
using DM.MovieApi.MovieDb.TV;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

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

    internal const string MovieDbBearerToken = "eyJhbGciOiJIUzI1NiJ9.eyJhdWQiOiIxZjVlNTAyNmJkMDM4YmZjZmU2MjI2MWU2ZGEwNjM0ZiIsInN1YiI6IjRiYzg4OTJjMDE3YTNjMGY5MjAwMDIyZCIsInNjb3BlcyI6WyJhcGlfcmVhZCJdLCJ2ZXJzaW9uIjoxfQ.yMwyT8DEK1rF1gQMKJ-ZSy-dUGxFs5T345XwBLrvrWE";

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
        var fileInfo = new FileInfo(args.FileName);
        string lookupName;
        if (UseFolderName)
        {
            lookupName = fileInfo.Directory.Name;
            if (Regex.IsMatch(lookupName, "^(Season|Staffel|Saison)", RegexOptions.IgnoreCase))
                lookupName = fileInfo.Directory.Parent.Name;
        }
        else
        {
            lookupName = GetTVShowInfo(fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf(fileInfo.Extension))).ShowName;
        }
        lookupName = lookupName.Replace(".", " ").Replace("_", " ");

        // look for year
        string year = string.Empty;
        var match = Regex.Matches(lookupName, @"((19[2-9][0-9])|(20[0-9]{2}))(?=([\.\s_\-\)\]]|$))").LastOrDefault();
        if (match != null)
        {
            year = match.Value;
            lookupName = lookupName.Substring(0, lookupName.IndexOf(year)).Trim();
        }

        // remove double spaces in case they were added when removing the year
        while (lookupName.IndexOf("  ") > 0)
            lookupName = lookupName.Replace("  ", " ");

        // RegisterSettings only needs to be called one time when your application starts-up.
        MovieDbFactory.RegisterSettings(MovieDbBearerToken);

        var movieApi = MovieDbFactory.Create<IApiTVShowRequest>().Value;
        
        args.Logger?.ILog("Lookup TV Show: " + lookupName);

        var response = movieApi.SearchByNameAsync(lookupName).Result;
        

        // try find an exact match
        var result = response.Results.OrderBy(x =>
            {
                if (string.IsNullOrEmpty(year) == false)
                {
                    return year == x.FirstAirDate.Year.ToString() ? 0 : 1;
                }
                return 0;
            })
            .ThenBy(x => x.Name.ToLower().Trim().Replace(" ", "") == lookupName.ToLower().Trim().Replace(" ", "") ? 0 : 1)
            .ThenBy(x => lookupName.ToLower().Trim().Replace(" ", "").StartsWith(x.Name.ToLower().Trim().Replace(" ", "")) ? 0 : 1)
            .ThenBy(x => x.Name)
            .FirstOrDefault();

        if (result == null)
            return 2; // no match

        args.SetParameter(Globals.TV_SHOW_INFO, result);

        Variables["tvshow.Title"] = result.Name;
        Variables["tvshow.Year"] = result.FirstAirDate.Year;
        Variables["VideoMetadata"] = GetVideoMetadata(movieApi, result.Id, args.TempPath);
        Variables[Globals.TV_SHOW_INFO] = result;
        args.UpdateVariables(Variables);
        
        return 1;

    }


    /// <summary>
    /// Gets the VideoMetadata
    /// </summary>
    /// <param name="tvApi">the tv API</param>
    /// <param name="id">the ID of the movie</param>
    /// <param name="tempPath">the temp path to save any images to</param>
    /// <returns>the VideoMetadata</returns>
    internal static VideoMetadata GetVideoMetadata(IApiTVShowRequest tvApi, int id, string tempPath)
    {
        var tv = tvApi.FindByIdAsync(id).Result?.Item;
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

    /// <summary>
    /// Gets the tv show name from the text
    /// </summary>
    /// <param name="text">the input text</param>
    /// <returns>the tv show name</returns>
    internal static (string ShowName, int? Season, int? Episode, int? LastEpisode) GetTVShowInfo(string text)
    {
        // Replace "1x02" format with "s1e02"
        text = Regex.Replace(text, @"(?<season>\d+)x(?<episode>\d+)", "s${season}e${episode}", RegexOptions.IgnoreCase);

        string pattern = @"^(?<showName>[\w\s.-]+)[. _-]?(?:(s|S)(?<season>\d+)(e|E)(?<episode>\d+)(?:-(?<lastEpisode>\d+))?)";

        Match match = Regex.Match(text, pattern);

        if (match.Success == false)
            return (text, null, null, null);
        
        string show = match.Groups["showName"].Value.Replace(".", " ").TrimEnd();
        if (show.EndsWith(" -"))
            show = show[..^2];
        int season = int.Parse(match.Groups["season"].Value);
        int episode = int.Parse(match.Groups["episode"].Value);
        string lastEpisodeStr = match.Groups["lastEpisode"].Value;
        int? lastEpisode = string.IsNullOrEmpty(lastEpisodeStr) ? (int?)null : int.Parse(lastEpisodeStr);

        return (show, season, episode, lastEpisode);
    }
}