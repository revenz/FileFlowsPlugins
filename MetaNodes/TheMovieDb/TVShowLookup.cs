using System.Text.Json;
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
    /// Gets or sets an optional language to use for the lookup.
    /// </summary>
    [Text(2)]
    public string Language { get; set; } = "";

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        var helper = new TVShowHelper(args);
        
        string fullFilename = args.WorkingFile.StartsWith(args.TempPath) ? args.LibraryFileName : args.WorkingFile;
        args.Logger.ILog("Full File Name: " + fullFilename);
        Language = LanguageHelper.GetIso1Code(Language?.Trim()?.EmptyAsNull() ?? "en");
        args.Logger?.ILog("Lookup Language: " + Language);
        
        (string lookupName, string year) = helper.GetLookupName(fullFilename, UseFolderName);

        // RegisterSettings only needs to be called one time when your application starts-up.
        MovieDbFactory.RegisterSettings(Globals.MovieDbBearerToken);
        
        args.Logger?.ILog("Lookup TV Show: " + lookupName);

        string tvShowInfoCacheKey = $"TVShowInfo: {lookupName} ({year})";
        TVShowInfo result = args.Cache?.GetObject<TVShowInfo>(tvShowInfoCacheKey);
        if (result != null)
        {
            args.Logger?.ILog("Got TV show info from cache: " + result.Name);
        }
        else
        {
            result = LookupShow(lookupName, year);

            if (result == null)
            {
                args.Logger?.ILog("No result found for: " + lookupName);
                return 2; // no match
            }
            args.Cache?.SetObject(tvShowInfoCacheKey, result);
        }
        
        string tvShowCacheKey = $"TVShow: {result.Id}";
        TVShow? tv = args.Cache?.GetObject<TVShow>(tvShowCacheKey);
        if (tv == null)
        {
            var tvApi = MovieDbFactory.Create<IApiTVShowRequest>().Value;
            tv = tvApi.FindByIdAsync(result.Id, language: Language).Result?.Item;
            if (tv != null)
                args.Cache?.SetObject(tvShowCacheKey, tv);
        }
        else
        {
            args.Logger?.ILog($"Got TV show from cache: {tv.Name} ({tv.FirstAirDate.Year})");
        }

        args.Logger?.ILog("Found TV Show: " + result.Name);
        args.SetParameter(Globals.TV_SHOW_INFO, result);

        Variables["tvshow.Title"] = result.Name;
        args.Logger?.ILog("Detected Title: " + result.Name);
        Variables["tvshow.Year"] = result.FirstAirDate.Year;
        args.Logger?.ILog("Detected Year: " + result.FirstAirDate.Year);
        var metadata = GetVideoMetadata(tv, args.TempPath);
        if (metadata != null)
        {
            Variables["VideoMetadata"] = metadata;
            args.Logger?.ILog("Title: " + metadata.Title);
            args.Logger?.ILog("Description: " + metadata.Description);
            args.Logger?.ILog("Year: " + metadata.Year);
            args.Logger?.ILog("ReleaseDate: " + metadata.ReleaseDate.ToShortDateString());
            args.Logger?.ILog("OriginalLanguage: " + metadata.OriginalLanguage);
        }

        Variables[Globals.TV_SHOW_INFO] = result;
        if (string.IsNullOrWhiteSpace(result.OriginalLanguage) == false)
        {
            result.OriginalLanguage = result.OriginalLanguage.ToLowerInvariant() switch
            {
                "ch" => "zho",
                "chi" => "zho",
                _ => tv.OriginalLanguage
            };
            Variables["OriginalLanguage"] = result.OriginalLanguage;
            args.Logger?.ILog("Detected Original Language: " + result.OriginalLanguage);
        }

        DownloadThumbnail(args, result.PosterPath);

        args.UpdateVariables(Variables);
        
        return 1;
    }


    /// <summary>
    /// Downloads the poster path
    /// </summary>
    /// <param name="args">the node parameteres</param>
    /// <param name="posterPath">the poster path</param>
    private void DownloadThumbnail(NodeParameters args, string posterPath)
    {
        if (string.IsNullOrWhiteSpace(posterPath) == false)
        {
            try
            {
                string url = "https://image.tmdb.org/t/p/w500" + posterPath;
                args.Logger?.ILog("Downloading poster: " + url);
                using var httpClient = new HttpClient();
                using var stream = httpClient.GetStreamAsync(url).Result;
                string file = Path.Combine(args.TempPath, Guid.NewGuid() + ".jpg");
                using var fileStream = new FileStream(file, FileMode.CreateNew);
                stream.CopyTo(fileStream);
                args.SetThumbnail(file);
                args.Logger?.ILog("Set thumbnail: " + file);
                //md.ArtJpeg = file;
            }
            catch (Exception)
            {
                // Ignored
            }
        }
    }
    /// <summary>
    /// Looks up a show online
    /// </summary>
    /// <param name="lookupName">the lookup name</param>
    /// <param name="year">the year</param>
    /// <returns>the show if found</returns>
    private TVShowInfo LookupShow(string lookupName, string year)
    {
        var movieApi = MovieDbFactory.Create<IApiTVShowRequest>().Value;

        var response = movieApi.SearchByNameAsync(lookupName, language: Language).Result;

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
        md.OriginalLanguage = tv.OriginalLanguage?.ToLowerInvariant() switch
        {
            "ch" => "zho",
            "chi" => "zho",
            _ => tv.OriginalLanguage
        };
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
}