using System.Text.RegularExpressions;
using DM.MovieApi;
using DM.MovieApi.MovieDb.Movies;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using FileHelper = FileFlows.Plugin.Helpers.FileHelper;

namespace MetaNodes.TheMovieDb;

/// <summary>
/// Movie Lookup 
/// </summary>
public class MovieLookup : Node
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
    public override string HelpUrl => "https://fileflows.com/docs/plugins/meta-nodes/movie-lookup";
    /// <summary>
    /// Gets the icon
    /// </summary>
    public override string Icon => "fas fa-film";

    private Dictionary<string, object> _Variables;
    
    /// <summary>
    /// Gets the Variables this flow element provides
    /// </summary>
    public override Dictionary<string, object> Variables => _Variables;

    /// <summary>
    /// Constructs a new instance of this flow element
    /// </summary>
    public MovieLookup()
    {
        _Variables = new Dictionary<string, object>()
        {
            { "movie.Title", "Batman Begins" },
            { "movie.Year", 2005 },
            { "movie.ImdbId", "tt0372784" },
            { "movie.Genre", "Action" }
        };
    }
    /// <summary>
    /// Gets or sets if the folder name should be used
    /// </summary>
    [Boolean(1)]
    public bool UseFolderName { get; set; }

    
    /// <inheritdoc/>
    public override int Execute(NodeParameters args)
    {
        string lookupName = PrepareLookupName(args, out int year);
        args.Logger?.ILog("Lookup name: " + lookupName);

        MovieDbFactory.RegisterSettings(Globals.MovieDbBearerToken);
        var movieApi = MovieDbFactory.Create<IApiMovieRequest>().Value;

        var result = SearchMovie(args, movieApi, lookupName, year);
        if (result == null)
            return 2; // No match found

        args.SetParameter(Globals.MOVIE_INFO, result);
        PopulateMovieVariables(args, result);
        args.SetDisplayName($"{result.Title} ({result.ReleaseDate.Year})");

        return RetrieveAdditionalMetadata(args, movieApi, result.Id) ? 1 : 2;
    }

    /// <summary>
    /// Prepares the lookup name from the input file name or folder name.
    /// Extracts the year from the name.
    /// </summary>
    /// <param name="args">The node parameters.</param>
    /// <param name="year">The extracted year from the name.</param>
    /// <returns>The cleaned-up lookup name.</returns>
    private string PrepareLookupName(NodeParameters args, out int year)
    {
        var originalName = UseFolderName
            ? FileHelper.GetDirectoryName(args.LibraryFileName)
            : FileHelper.GetShortFileNameWithoutExtension(args.LibraryFileName);

        string lookupName = originalName.Replace(".", " ").Replace("_", " ");
        lookupName = RemoveYearFromName(lookupName, out year);
        lookupName = lookupName.TrimEnd('(', '-', ' ');

        args.Logger?.ILog($"Prepared lookup name: {lookupName}, Detected Year: {year}");
        return lookupName;
    }


    /// <summary>
    /// Removes the year from the lookup name if present.
    /// </summary>
    private static string RemoveYearFromName(string lookupName, out int year)
    {
        year = 0;
        var match = Regex.Matches(lookupName, @"(?<=[\s\.\-\[\(\{])((19[2-9][0-9])|(20[0-9]{2}))(?=[\s.\-_\]\)\}]|$)").LastOrDefault();
        if (match != null)
        {
            int.TryParse(match.Value, out year);
            lookupName = lookupName[..lookupName.IndexOf(match.Value, StringComparison.Ordinal)].TrimEnd('(');
        }


        return lookupName.Replace("  ", " ");
    }
    
    /// <summary>
    /// Searches for a movie using the API.
    /// </summary>
    /// <param name="args">The node parameters for logging.</param>
    /// <param name="movieApi">The API client for movie lookup.</param>
    /// <param name="lookupName">The name of the movie to search for.</param>
    /// <param name="year">The extracted year from the title.</param>
    /// <returns>The movie information if found; otherwise, null.</returns>
    private MovieInfo SearchMovie(NodeParameters args, IApiMovieRequest movieApi, string lookupName, int year)
    {
        try
        {
            args.Logger?.ILog($"Searching for movie: {lookupName}");

            var response = movieApi.SearchByTitleAsync(lookupName).Result;
        
            if (response.Results.Count == 0 && lookupName.Contains("Ae", StringComparison.InvariantCultureIgnoreCase))
            {
                lookupName = lookupName.Replace("Ae", "Ä", StringComparison.InvariantCultureIgnoreCase);
                args.Logger?.ILog($"Retrying search with modified title: {lookupName}");
                response = movieApi.SearchByTitleAsync(lookupName).Result;
            }

            if (response.Results.Count == 0)
            {
                args.Logger?.WLog($"No results found for '{lookupName}'");
                return null;
            }

            // Store the year in a local variable to use inside the lambda
            int searchYear = year;

            var movies  = response.Results
                .OrderBy(x =>
                {
                    if (searchYear > 0)
                        return Math.Abs(searchYear - x.ReleaseDate.Year) < 2 ? 0 : 1;
                    return 0;
                })
                .ThenBy(x => x.VoteCount > 250 ? 1 : 2) // behind the scenes etc, try treduce those
                .ThenBy(x => NormalizeTitle(x.Title) == NormalizeTitle(lookupName) ? 0 : 1)
                .ToList();
            
            var movie = movies.FirstOrDefault();

            args.Logger?.ILog($"Found movie: {movie?.Title} ({movie?.ReleaseDate.Year})");

            return movie;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog($"Error searching for movie '{lookupName}': {ex.Message}");
            return null;
        }
    }



    /// <summary>
    /// Normalizes a movie title by removing spaces and converting to lowercase.
    /// </summary>
    private static string NormalizeTitle(string title)
        => title.ToLower().Trim().Replace(" ", "");

    /// <summary>
    /// Populates movie-related variables into the execution context.
    /// </summary>
    private void PopulateMovieVariables(NodeParameters args, MovieInfo result)
    {
        args.Variables["movie.Title"] = result.Title;
        args.Logger?.ILog("Detected Movie Title: " + result.Title);
        args.Variables["movie.Year"] = result.ReleaseDate.Year;
        args.Logger?.ILog("Detected Movie Year: " + result.ReleaseDate.Year);
    }

    /// <summary>
    /// Retrieves additional metadata such as cast and crew information.
    /// </summary>
    private bool RetrieveAdditionalMetadata(NodeParameters args, IApiMovieRequest movieApi, int movieId)
    {
        try
        {
            var meta = GetVideoMetadata(args, movieApi, movieId, args.TempPath);
            if (meta == null)
                return false;

            args.Variables["VideoMetadata"] = meta;
            if (!string.IsNullOrWhiteSpace(meta.OriginalLanguage))
            {
                args.Logger?.ILog("Detected Original Language: " + meta.OriginalLanguage);
                args.Variables["OriginalLanguage"] = meta.OriginalLanguage;
            }

            return true;
        }
        catch (Exception ex)
        {
            args.Logger?.WLog($"Failed looking up movie: {ex}");
            return false;
        }
    }

    /// <summary>
    /// Retrieves video metadata including movie details and credits.
    /// </summary>
    internal static VideoMetadata GetVideoMetadata(NodeParameters args, IApiMovieRequest movieApi, int id, string tempPath)
    {
        var movie = movieApi.FindByIdAsync(id).Result?.Item;
        if (movie == null)
            return null;

        if (!string.IsNullOrWhiteSpace(movie.ImdbId))
            args.Variables["movie.ImdbId"] = movie.ImdbId;
        if (movie.Genres?.Any() == true)
            args.Variables["movie.Genre"] = movie.Genres.First().Name;

        args.SetParameter(Globals.MOVIE, movie);
        
        var meta = new VideoMetadata
        {
            Title = movie.Title,
            Genres = movie.Genres?.Select(x => x.Name).ToList(),
            Description = movie.Overview,
            Year = movie.ReleaseDate.Year,
            Subtitle = movie.Tagline,
            ReleaseDate = movie.ReleaseDate,
            OriginalLanguage = movie.OriginalLanguage
        };

        DownloadPosterImage(args, movie, tempPath, meta);
        PopulateCredits(args, movieApi, id, meta);

        return meta;
    }

    /// <summary>
    /// Downloads and assigns the movie poster image.
    /// </summary>
    private static void DownloadPosterImage(NodeParameters args, Movie movie, string tempPath, VideoMetadata meta)
    {
        if (string.IsNullOrWhiteSpace(movie.PosterPath))
            return;

        try
        {
            using var httpClient = new HttpClient();
            using var stream = httpClient.GetStreamAsync("https://image.tmdb.org/t/p/w500" + movie.PosterPath).Result;
            string file = Path.Combine(tempPath, Guid.NewGuid() + ".jpg");
            using var fileStream = new FileStream(file, FileMode.CreateNew);
            stream.CopyTo(fileStream);
            meta.ArtJpeg = file;
            args.SetThumbnail(file);
        }
        catch
        {
            // Ignored
        }
    }

    /// <summary>
    /// Populates the cast and crew information.
    /// </summary>
    private static void PopulateCredits(NodeParameters args, IApiMovieRequest movieApi, int id, VideoMetadata meta)
    {
        var credits = movieApi.GetCreditsAsync(id).Result?.Item;
        if (credits == null) return;

        args.Variables[Globals.MOVIE_CREDITS] = credits;
        meta.Actors = credits.CastMembers?.Select(x => x.Name)?.ToList();
        meta.Directors = credits.CrewMembers?.Where(x => x.Job == "Director")?.Select(x => x.Name)?.ToList();
    }
}