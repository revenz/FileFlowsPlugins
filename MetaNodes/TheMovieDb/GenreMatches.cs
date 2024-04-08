using System.ComponentModel.DataAnnotations;
using DM.MovieApi.MovieDb.Movies;
using DM.MovieApi.MovieDb.TV;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace MetaNodes.TheMovieDb;

/// <summary>
/// Tests a genres matches 
/// </summary>
public class GenreMatches: Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override string Icon => "fas fa-theater-masks";
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/meta-nodes/genre-matches";


    /// <summary>
    /// Gets or sets if all selected genres should must be present
    /// </summary>
    [Boolean(1)]
    public bool MatchAll { get; set; }
    
    /// <summary>
    /// The genres to match
    /// </summary>
    [Checklist(nameof(Options), 2)]
    [Required]
    public List<string> Genres { get; set; }

    private static List<ListOption> _Options;
    /// <summary>
    /// The options available
    /// </summary>
    public static List<ListOption> Options
    {
        get
        {
            if (_Options == null)
            {
                _Options = new List<ListOption>
                {
                    new () { Value = "Action", Label = "Action"},
                    new () { Value = "Adventure", Label = "Adventure"},
                    new () { Value = "Action & Adventure", Label = "Action & Adventure" },
                    new () { Value = "Animation", Label = "Animation"},
                    new () { Value = "Comedy", Label = "Comedy"},
                    new () { Value = "Crime", Label = "Crime"},
                    new () { Value = "Documentary", Label = "Documentary"},
                    new () { Value = "Drama", Label = "Drama"},
                    new () { Value = "Family", Label = "Family"},
                    new () { Value = "Fantasy", Label = "Fantasy"},
                    new () { Value = "History", Label = "History"},
                    new () { Value = "Horror", Label = "Horror"},
                    new () { Value = "Kids", Label = "Kids" },
                    new () { Value = "Music", Label = "Music"},
                    new () { Value = "Mystery", Label = "Mystery"},
                    new () { Value = "News", Label = "News" },
                    new () { Value = "Reality", Label = "Reality" },
                    new () { Value = "Romance", Label = "Romance"},
                    new () { Value = "Science Fiction", Label = "Science Fiction"},
                    new () { Value = "Sci-Fi & Fantasy", Label = "Sci-Fi & Fantasy" },
                    new () { Value = "Soap", Label = "Soap" },
                    new () { Value = "Talk", Label = "Talk" },
                    new () { Value = "Thriller", Label = "Thriller"},
                    new () { Value = "TV Movie", Label = "TV Movie"},
                    new () { Value = "War", Label = "War"},
                    new () { Value = "War & Politics", Label = "War & Politics" },   
                    new () { Value = "Western", Label = "Western"},              
                };
            }
            return _Options;
        }
    }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var expected = Genres?.Select(x => x.ToLowerInvariant()).ToList() ??  new List<string>();
        if (expected.Any() == false)
        {
            args.FailureReason = "No genres selected";
            args.Logger.ELog(args.FailureReason);
            return -1;
        }

        args.Logger?.ILog($"Expecting {(MatchAll ? "all" : "any")}: {string.Join(", ", Genres)}");
        
        List<string> videoGenres = new();
        if (args.Variables.TryGetValue(Globals.MOVIE_INFO, out object oMovieInfo) && oMovieInfo is MovieInfo mi)
        {
            args.Logger?.ILog("Found movie info");
            videoGenres.AddRange(mi.Genres?.Select(x => x.Name)?.ToList() ?? new());
        }
        
        if (args.Variables.TryGetValue(Globals.TV_SHOW_INFO, out object oShowInfo) && oShowInfo is TVShowInfo show)
        {
            args.Logger?.ILog("Found TV Show info");
            videoGenres.AddRange(show.Genres?.Select(x => x.Name)?.ToList() ?? new());
        }

        videoGenres = videoGenres.Distinct().ToList();

        if (videoGenres?.Any() != true)
        {
            args.Logger?.ILog("No genres found");
            return 2;
        }
        args.Logger?.ILog("Genres in info: " + string.Join(", ", videoGenres));

        var matches = videoGenres
            .Where(x => expected.Contains(x.ToLowerInvariant()))
            .ToList();
        if (matches.Count == 0)
        {
            args.Logger?.ILog("No matching genres found");
            return 2;
        }
        args.Logger?.ILog("Matching Genres:" + string.Join(", ", matches));
        if (MatchAll == false)
            return 1;

        if (expected.Count < matches.Count)
        {
            args.Logger?.ILog("Not all genres were matched");
            return 2;
        }

        return 1;
    }
}