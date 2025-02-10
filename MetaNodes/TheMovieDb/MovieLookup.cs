﻿using System.Text.RegularExpressions;
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

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        var originalName = UseFolderName
            ? FileHelper.GetDirectoryName(args.LibraryFileName)
            : FileHelper.GetShortFileNameWithoutExtension(args.LibraryFileName);
        string lookupName = originalName;
        lookupName = lookupName.Replace(".", " ").Replace("_", " ");

        // look for year
        int year = 0;
        var match = Regex.Matches(lookupName, @"((19[2-9][0-9])|(20[0-9]{2}))(?=([\.\s_\-\)\]]|$))").LastOrDefault();
        if (match != null)
        {
            int.TryParse(match.Value, out year);
            lookupName = lookupName[..lookupName.IndexOf(match.Value, StringComparison.Ordinal)].TrimEnd('(');
        }

        // remove double spaces in case they were added when removing the year
        while (lookupName.IndexOf("  ", StringComparison.Ordinal) > 0)
            lookupName = lookupName.Replace("  ", " ");

        lookupName = lookupName.TrimEnd('(', '-');
        
        args.Logger?.ILog("Lookup name: " + lookupName);

        // RegisterSettings only needs to be called one time when your application starts-up.
        MovieDbFactory.RegisterSettings(Globals.MovieDbBearerToken);

        var movieApi = MovieDbFactory.Create<IApiMovieRequest>().Value;

        var response = movieApi.SearchByTitleAsync(lookupName).Result;
        
        if(response.Results.Count == 0 && originalName.Contains("german", StringComparison.CurrentCultureIgnoreCase) && lookupName.Contains("Ae", StringComparison.InvariantCultureIgnoreCase))
        {
            lookupName = lookupName.Replace("Ae", "Ä", StringComparison.InvariantCultureIgnoreCase);
            response = movieApi.SearchByTitleAsync(lookupName).Result;
        }
        

        // try find an exact match
        var results = response.Results.OrderBy(x =>
            {
                if (year > 0)
                {
                    // sometimes a year can be off by 1, if a movie was released late in the year but recorded in the next year
                    return Math.Abs(year - x.ReleaseDate.Year) < 2 ? 0 : 1;
                }
                return 0;
            })
            .ThenBy(x => x.Title.ToLower().Trim().Replace(" ", "") == lookupName.ToLower().Trim().Replace(" ", "") ? 0 : 1)
            .ThenBy(x =>
            {
                // do some fuzzy logic with roman numerals
                var numMatch = Regex.Match(lookupName, @"[\s]([\d]+)$");
                if (numMatch.Success == false)
                    return 0;
                int number = int.Parse(numMatch.Groups[1].Value);
                string roman = number switch
                {
                    1 => "i",
                    2 => "ii",
                    3 => "iii",
                    4 => "iv",
                    5 => "v",
                    6 => "vi,",
                    7 => "vii",
                    8 => "viii",
                    9 => "ix",
                    10 => "x",
                    11 => "xi",
                    12 => "xii",
                    13 => "xiii",
                    _ => string.Empty
                };
                string ln = lookupName[..lookupName.LastIndexOf(number.ToString(), StringComparison.Ordinal)].ToLower().Trim().Replace(" ", "");
                string softTitle = x.Title.ToLower().Replace(" ", "").Trim();
                if (softTitle == ln + roman)
                    return 0;
                if (softTitle.StartsWith(ln) && softTitle.EndsWith(roman))
                    return 0;
                return 1;
             })
            .ThenBy(x => lookupName.ToLower().Trim().Replace(" ", "").StartsWith(x.Title.ToLower().Trim().Replace(" ", "")) ? 0 : 1)
            // .ThenBy(x => x.Title)
            .ToList();
        var result = results.FirstOrDefault();

        if (result == null)
            return 2; // no match

        args.SetParameter(Globals.MOVIE_INFO, result);

        args.Variables["movie.Title"] = result.Title;
        args.Logger?.ILog("Detected Movie Title: " + result.Title);
        args.Variables["movie.Year"] = result.ReleaseDate.Year;
         
        args.SetDisplayName($"{result.Title} ({result.ReleaseDate.Year})");
        
        args.Logger?.ILog("Detected Movie Year: " + result.ReleaseDate.Year);
        var meta = GetVideoMetadata(args, movieApi, result.Id, args.TempPath);
        args.Variables["VideoMetadata"] = meta;
        if (string.IsNullOrWhiteSpace(meta.OriginalLanguage) == false)
        {
            args.Logger?.ILog("Detected Original Language: " + meta.OriginalLanguage);
            args.Variables["OriginalLanguage"] = meta.OriginalLanguage;
        }

        args.Variables[Globals.MOVIE_INFO] = result;
        var movie = movieApi.FindByIdAsync(result.Id).Result.Item;
        if(movie != null)
            args.Variables[Globals.MOVIE] = movie;

        return 1;
    }


    /// <summary>
    /// Gets the VideoMetadata
    /// </summary>
    /// <param name="movieApi">the movie API</param>
    /// <param name="id">the ID of the movie</param>
    /// <param name="tempPath">the temp path to save any images to</param>
    /// <returns>the VideoMetadata</returns>
    internal static VideoMetadata GetVideoMetadata(NodeParameters args, IApiMovieRequest movieApi, int id, string tempPath)
    {
        var movie = movieApi.FindByIdAsync(id).Result?.Item;
        if (movie == null)
            return null;
        
        if(string.IsNullOrWhiteSpace(movie.ImdbId) == false)
            args.Variables["movie.ImdbId"] = movie.ImdbId;
        if(movie.Genres?.Any() != false)
            args.Variables["movie.Genre"] = movie.Genres.First().Name;
        
        var credits = movieApi.GetCreditsAsync(id).Result?.Item;

        VideoMetadata md = new();
        md.Title = movie.Title;
        md.Genres = movie.Genres?.Select(x => x.Name).ToList();
        md.Description = movie.Overview;
        md.Year = movie.ReleaseDate.Year;
        md.Subtitle = movie.Tagline;
        md.ReleaseDate = movie.ReleaseDate;
        md.OriginalLanguage = movie.OriginalLanguage;
        if (string.IsNullOrWhiteSpace(movie.PosterPath) == false)
        {
            try
            {
                using var httpClient = new HttpClient();
                using var stream = httpClient.GetStreamAsync("https://image.tmdb.org/t/p/w500" + movie.PosterPath).Result;
                string file = Path.Combine(tempPath, Guid.NewGuid() + ".jpg");
                using var fileStream = new FileStream(file, FileMode.CreateNew);
                stream.CopyTo(fileStream);
                md.ArtJpeg = file;
                args.SetThumbnail(file);
            }
            catch (Exception)
            {
                // Ignored
            }
        }
        
        if(credits != null)
        {
            args.Variables[Globals.MOVIE_CREDITS] = credits;
            md.Actors = credits.CastMembers?.Select(x => x.Name)?.ToList();
            md.Writers  = credits.CrewMembers?.Where(x => x.Department == "Writing" || x.Job == "Writer" || x.Job == "Screenplay") ?.Select(x => x.Name)?.ToList();
            md.Directors = credits.CrewMembers?.Where(x => x.Job == "Director")?.Select(x => x.Name)?.ToList();
            md.Producers = credits.CrewMembers?.Where(x => x.Job == "Producer")?.Select(x => x.Name)?.ToList();
        }

        return md;
    }
}