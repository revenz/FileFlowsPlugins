using DM.MovieApi;
using DM.MovieApi.MovieDb.TV;
using FileFlows.Plugin;

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

    internal const string MovieDbBearerToken = "eyJhbGciOiJIUzI1NiJ9.eyJhdWQiOiIxZjVlNTAyNmJkMDM4YmZjZmU2MjI2MWU2ZGEwNjM0ZiIsInN1YiI6IjRiYzg4OTJjMDE3YTNjMGY5MjAwMDIyZCIsInNjb3BlcyI6WyJhcGlfcmVhZCJdLCJ2ZXJzaW9uIjoxfQ.yMwyT8DEK1rF1gQMKJ-ZSy-dUGxFs5T345XwBLrvrWE";

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        string filename = args.FileName.Replace("\\", "/");
        filename = filename.Substring(filename.LastIndexOf("/") + 1);
        filename = filename.Substring(0, filename.LastIndexOf("."));
        
        (string lookupName, string year) = TVShowLookup.GetLookupName(filename, false);

        (string showName, int? season, int? episode, int? lastEpisode) = TVShowLookup.GetTVShowInfo(filename);

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
        Variables["tvepisode.Subtitle"] = epInfo.Name;
        Variables["tvepisode.Year"] = epInfo.AirDate.Year;
        Variables["tvepisode.AirDate"] = epInfo.AirDate;
        Variables["tvepisode.Season"] = season.Value;
        Variables["tvepisode.Episode"] = episode.Value;
        if(lastEpisode != null)
            Variables["tvepisode.LastEpisode"] = lastEpisode.Value;
        Variables["tvepisode.Overview"] = epInfo.Overview;
        //Variables["VideoMetadata"] = GetVideoMetadata(movieApi, result.Id, args.TempPath);
        Variables[Globals.TV_SHOW_INFO] = result;
        Variables[Globals.TV_EPISODE_INFO] = epInfo;
        
        if (string.IsNullOrWhiteSpace(result.OriginalLanguage) == false)
            Variables["OriginalLanguage"] = result.OriginalLanguage;
        
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
}