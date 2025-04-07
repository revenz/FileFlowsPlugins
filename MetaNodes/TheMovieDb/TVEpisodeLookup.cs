using DM.MovieApi;
using DM.MovieApi.MovieDb.TV;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using MetaNodes.Helpers;
using FileHelper = FileFlows.Plugin.Helpers.FileHelper;

namespace MetaNodes.TheMovieDb;

/// <summary>
/// TV Episode Lookup 
/// </summary>
public class TVEpisodeLookup : Node
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
    public override string HelpUrl => "https://fileflows.com/docs/plugins/meta-nodes/tv-episode-lookup";
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
    /// Gets or sets if the folder name should be used
    /// </summary>
    [Boolean(1)]
    public bool UseFolderName { get; set; }

    /// <summary>
    /// Constructs a new instance of this flow element
    /// </summary>
    public TVEpisodeLookup()
    {
        _Variables = new Dictionary<string, object>()
        {
            { "tvepisode.Title", "The Batman" },
            { "tvepisode.Subtitle", "The Man Who Laughs" },
            { "tvepisode.Year", 2004 },
            { "tvepisode.AirDate", new DateTime(2004, 06, 01) },
            { "tvepisode.Season", 2 },
            { "tvepisode.Episode", 4 },
            { "tvepisode.LastEpisode", 7 },
            { "tvepisode.Overview", "Joker makes Batman laugh" },
        };
    }
    
    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        string fullFilename = args.WorkingFile.StartsWith(args.TempPath) ? args.LibraryFileName : args.WorkingFile;
        string filename = FileHelper.GetShortFileNameWithoutExtension(fullFilename);
        
        args.Logger.ILog("Lookup filename: " + filename);
        
        var helper = new TVShowHelper(args);
        (string lookupName, string year) = helper.GetLookupName(args.LibraryFileName, UseFolderName);

        (string showName, int? season, int? episode, int? lastEpisode, string year2) = helper.GetTVShowInfo(filename);

        if (season == null)
        {
            args.Logger?.WLog("Season not found in string: " + filename);
            return 2;
        }
            
        if (episode == null)
        {
            args.Logger?.WLog("Episode not found in string: " + filename);
            return 2;
        }
        
        args.Logger?.ILog($"Found show info from filename '{lookupName}' season '{season}' episode '{(episode + (lastEpisode == null ? "" : "-" + lastEpisode))}'");
        
        string tvShowInfoCacheKey = $"TVShowInfo: {lookupName} ({year})";
        TVShowInfo result = args.Cache?.GetObject<TVShowInfo>(tvShowInfoCacheKey);

        // RegisterSettings only needs to be called one time when your application starts-up.
        MovieDbFactory.RegisterSettings(Globals.MovieDbBearerToken);
        
        if (result != null)
        {
            args.Logger?.ILog("Got TV show info from cache: " + result.Name);
        }
        else
        {
            var movieApi = MovieDbFactory.Create<IApiTVShowRequest>().Value;

            args.Logger?.ILog("Lookup TV Show: " + lookupName);

            var response = movieApi.SearchByNameAsync(lookupName).Result;

            // try find an exact match
            result = response.Results.OrderBy(x =>
                {
                    if (string.IsNullOrEmpty(year) == false)
                    {
                        return year == x.FirstAirDate.Year.ToString() ? 0 : 1;
                    }

                    if (string.IsNullOrEmpty(year2) == false)
                    {
                        return year2 == x.FirstAirDate.Year.ToString() ? 0 : 1;
                    }

                    return 0;
                })
                .ThenBy(x =>
                    x.Name.ToLower().Trim().Replace(" ", "") == lookupName.ToLower().Trim().Replace(" ", "") ? 0 : 1)
                .ThenBy(x =>
                    lookupName.ToLower().Trim().Replace(" ", "").StartsWith(x.Name.ToLower().Trim().Replace(" ", ""))
                        ? 0
                        : 1)
                .ThenBy(x => x.Name)
                .FirstOrDefault();
        }

        if (result == null)
        {
            args.Logger?.ILog("No result found for: " + lookupName);
            return 2; // no match
        }

        args.Logger?.ILog("Found TV Show: " + result.Name);
        
        // now we have the show, try and get the episode

        var episodeApi  = MovieDbFactory.Create<IApiTVShowRequest>().Value;
        
        var show = episodeApi.GetTvShowSeasonInfoAsync(result.Id, season.Value).Result;
        if (show.Item == null)
        {
            args.Logger?.WLog("Failed to load season info: " + (show?.Error?.Message?.EmptyAsNull() ?? "Unexpected error"));
            return 2;
        }

        if (show.Item.Episodes.Count < episode.Value)
        {
            args.Logger?.WLog("Season found but episode number is outside its bounds");
            return 2;
        }

        var epInfo = show.Item.Episodes.FirstOrDefault(x => x.EpisodeNumber == episode.Value);
        if (epInfo == null)
        {
            args.Logger?.WLog("Failed to locate episode number: " + episode.Value);
            return 2;
        }
        
        args.SetParameter(Globals.TV_SHOW_INFO, result);

        if (lastEpisode > episode)
        {
            for (int i = episode.Value + 1; i <= lastEpisode; i++)
            {
                
                var epInfoExtra = show.Item.Episodes.FirstOrDefault(x => x.EpisodeNumber == i);
                if (epInfoExtra == null)
                    continue;
                int diff = i - episode.Value;
                args.Variables[Globals.TV_EPISODE_INFO + "_" + diff] = epInfoExtra;
            }
        }

        Variables["tvepisode.Title"] = result.Name;
        args.Logger?.ILog("Detected Title: " + result.Name);
        Variables["tvepisode.Subtitle"] = epInfo.Name;
        Variables["tvepisode.Year"] = epInfo.AirDate.Year;
        Variables["tvepisode.AirDate"] = epInfo.AirDate;
        args.Logger?.ILog("Detected Air Date: " + epInfo.AirDate.ToShortDateString());
        Variables["tvepisode.Season"] = season.Value;
        args.Logger?.ILog("Detected Season: " + epInfo.SeasonNumber);
        Variables["tvepisode.Episode"] = episode.Value;
        args.Logger?.ILog("Detected Episode: " + episode.Value);
        if (lastEpisode != null)
        {
            Variables["tvepisode.LastEpisode"] = lastEpisode.Value;
            args.Logger?.ILog("Detected Last Episode: " + lastEpisode.Value);

            args.SetDisplayName(
                $"{result.Name} - {epInfo.SeasonNumber}x{episode.Value:D2}-{lastEpisode.Value:D2} - {epInfo.Name}");
        }
        else
        {
            args.SetDisplayName($"{result.Name} - {epInfo.SeasonNumber}x{episode.Value:D2} - {epInfo.Name}");
        }

        Variables["tvepisode.Overview"] = epInfo.Overview;
        //Variables["VideoMetadata"] = GetVideoMetadata(movieApi, result.Id, args.TempPath);
        Variables[Globals.TV_SHOW_INFO] = result;
        Variables[Globals.TV_EPISODE_INFO] = epInfo;

        if (string.IsNullOrWhiteSpace(result.OriginalLanguage) == false)
        {
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
}