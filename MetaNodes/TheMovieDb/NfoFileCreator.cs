using System.Text;
using System.Xml;
using DM.MovieApi.MovieDb.Movies;
using DM.MovieApi.MovieDb.TV;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using FileFlows.Plugin.Helpers;

namespace MetaNodes.TheMovieDb;

/// <summary>
/// A flow element that will create a NFO file for a video once the Movie or TV Show info has been read
/// </summary>
public class NfoFileCreator : Node
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
    public override FlowElementType Type => FlowElementType.Process;
    /// <summary>
    /// Gets the help URL
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/meta-nodes/nfo-file-creator";
    /// <summary>
    /// Gets the icon
    /// </summary>
    public override string Icon => "svg:nfo";

    private const string TAB = "    ";


    private string _DestinationPath = string.Empty;
    private string _DestinationFile = string.Empty;
    /// <summary>
    /// Gets or sets the destination path for zipping.
    /// </summary>
    [Folder(1)]
    public string DestinationPath
    {
        get => _DestinationPath;
        set { _DestinationPath = value ?? ""; }
    }

    /// <summary>
    /// Gets or sets the destination file name for zipping.
    /// </summary>
    [TextVariable(2)]
    public string DestinationFile
    {
        get => _DestinationFile;
        set { _DestinationFile = value ?? ""; }
    }

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public override int Execute(NodeParameters args)
    {
        string nfoXml = null;
        if (args.Variables.TryGetValue(Globals.TV_SHOW_INFO, out object oTvShowInfo) &&
            oTvShowInfo is TVShowInfo tvShowInfo)
        {
            if (args.Variables.TryGetValue(Globals.TV_EPISODE_INFO, out object oTvEpisodeInfo) &&
                oTvEpisodeInfo is Episode epInfo)
            {
                args.Logger?.ILog("TVEpisodeInfo found");
                nfoXml = CreateTvShowNfo(args, tvShowInfo, epInfo);
                
                
                // look for more episodes
                for (int i = 1; 1 <= 100; i++)
                {
                    if (args.Variables.TryGetValue(Globals.TV_EPISODE_INFO + "_" + i, out object oOther) == false)
                        break;
                    if (oOther is Episode epOther == false)
                        break;

                    nfoXml += CreateTvShowNfo(args, tvShowInfo, epOther);
                }
                
            }
        }
        else if (args.Variables.TryGetValue(Globals.MOVIE, out object oMovieInfo) &&
                 oMovieInfo is Movie movie)
        {
            args.Logger?.ILog("MovieInformation found");
            nfoXml = CreateMovieNfo(args, movie);
        }


        if (string.IsNullOrWhiteSpace(nfoXml))
        {
            args.Logger?.ILog("No TV or Movie information found in flow");
            return 2;
        }

        // remove any white space
        nfoXml = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\" ?>" + Environment.NewLine + nfoXml.Trim();
        args.Variables["NFO"] = nfoXml; 

        string path = args.ReplaceVariables(DestinationPath, stripMissing: true)?.EmptyAsNull() ?? FileHelper.GetDirectory(args.LibraryFileName ?? string.Empty);
        string filename = args.ReplaceVariables(DestinationFile, stripMissing: true)?.EmptyAsNull() ??
                          FileHelper.ChangeExtension(FileHelper.GetShortFileName(args.LibraryFileName ?? string.Empty), "nfo");

        if ((path + filename) == ".nfo")
        {
            // this is extremely likely to be a unit test, the library file would be set otherwise
            args.Logger?.ILog("No file set to output to.");
            return 1;
        }

        string output = FileHelper.Combine(path, filename);

        string tempFile = FileHelper.Combine(args.TempPath, Guid.NewGuid() + ".nfo");
        File.WriteAllText(tempFile, nfoXml);
        if (args.FileService.FileMove(tempFile, output).Failed(out string error))
        {
            args.FailureReason = error;
            args.Logger?.ELog(error);
            return -1;
        }
        args.Logger.ILog("NFO File Created at: " + output);
        return 1;
    }

    private string? CreateMovieNfo(NodeParameters args, Movie movie)
    {
        StringBuilder output = new();
        
        output.AppendLine("<movie>");
        
        WriteXmlEscapedElement(output, "title", movie.Title);
        WriteXmlEscapedElement(output, "originaltitle", movie.OriginalTitle);
        WriteXmlEscapedElement(output, "plot", movie.Overview);
        WriteXmlEscapedElement(output, "tagline", movie.Tagline);
        if(movie.Runtime > 0)
            WriteXmlEscapedElement(output, "runtime", movie.Runtime.ToString());
        WriteXmlEscapedElement(output, "premiered", movie.ReleaseDate.ToString("yyyy-MM-dd"));
        output.AppendLine($"{TAB}<uniqueid type=\"tmdb\" default=\"true\">{movie.Id}</uniqueid>");
        if(string.IsNullOrWhiteSpace(movie.ImdbId) == false)
            output.AppendLine($"{TAB}<uniqueid type=\"imdb\">{movie.ImdbId}</uniqueid>");
        
        // Genres
        if (movie.Genres?.Any() == true)
        {
            string genres = string.Join(",", movie.Genres.Select(g => g.Name));
            WriteXmlEscapedElement(output, "genres", genres);
        }
        
        if(movie.ProductionCompanies?.Any() == true)
            WriteXmlEscapedElement(output, "studio", movie.ProductionCompanies.First().Name);

        WriteThumb(output, "poster", movie.PosterPath);
        WriteThumb(output, "landscape", movie.BackdropPath);

        object oCredits = null;
        if (args.Variables.TryGetValue(Globals.MOVIE_CREDITS, out oCredits) && oCredits is MovieCredit credit)
        {
            var writers = credit.CrewMembers
                .Where(x => x.Department == "Writing" || x.Job == "Writer" || x.Job == "Screenplay")
                .Select(crewMember => crewMember.Name).Distinct();

            var directors = credit.CrewMembers.Where(crewMember => crewMember.Job == "Director")
                .Select(crewMember => crewMember.Name).Distinct();

            foreach (var epWriter in writers)
            {
                WriteXmlEscapedElement(output, "credits", epWriter);
            }

            foreach (var director in directors)
            {
                WriteXmlEscapedElement(output, "director", director);
            }
        }

        if (movie.VoteCount > 0)
        {
            output.AppendLine(TAB + "<ratings>");
            output.AppendLine(TAB + TAB + "<rating name=\"themoviedb\" max=\"10\">");
            WriteXmlEscapedElement(output, "value", movie.VoteAverage.ToString(), tabs: 3);
            WriteXmlEscapedElement(output, "votes", movie.VoteCount.ToString(), tabs: 3);
            output.AppendLine(TAB + TAB + "</rating>");
            output.AppendLine(TAB + "</ratings>");
        }

        if (string.IsNullOrWhiteSpace(movie.MovieCollectionInfo?.Name) == false)
        {
            output.AppendLine(TAB + "<set>");
            WriteXmlEscapedElement(output, "name", movie.MovieCollectionInfo.Name, tabs: 2);
            output.AppendLine(TAB + "</set>");
        }


        if (oCredits is MovieCredit credit2)
        {
            var cast = credit2.CastMembers?.ToList();
            if(cast?.Any() == true)
            {
                for (int i = 0; i < cast.Count; i++)
                {
                    var castMember = cast[i];
                    WriteActorElement(output, castMember.Name, castMember.Character, i, castMember.ProfilePath);
                }
            }

        }
        output.AppendLine("</movie>");
        return output.ToString();
    }

    internal string CreateTvShowNfo(NodeParameters args, TVShowInfo tvShowInfo, Episode episode)
    {
        StringBuilder output = new();
        
        output.AppendLine("<episodedetails>");

        WriteXmlEscapedElement(output, "title", episode.Name);
        WriteXmlEscapedElement(output, "originaltitle", tvShowInfo.OriginalName);
        WriteXmlEscapedElement(output, "showtitle", tvShowInfo.Name);
        WriteXmlEscapedElement(output, "season", episode.SeasonNumber.ToString());
        WriteXmlEscapedElement(output, "episode", episode.EpisodeNumber.ToString());
        WriteXmlEscapedElement(output, "plot", episode.Overview);
        WriteXmlEscapedElement(output, "premiered", episode.AirDate.ToString("yyyy-MM-dd"));
        
        output.AppendLine($"{TAB}<uniqueid type=\"tmdb\" default=\"true\">{episode.Id}</uniqueid>");

        // Additional episode information from TVShowInfo
        //WriteXmlEscapedElement(writer, "tvshow_id", tvShowInfo.Id.ToString());
        //WriteXmlEscapedElement(writer, "tvshow_name", tvShowInfo.Name);
        //WriteXmlEscapedElement(writer, "original_name", tvShowInfo.OriginalName);
        // WriteXmlEscapedElement(writer, "poster_path", tvShowInfo.PosterPath);
        // WriteXmlEscapedElement(writer, "backdrop_path", tvShowInfo.BackdropPath);
        
        WriteXmlEscapedElement(output, "overview", tvShowInfo.Overview);
        WriteXmlEscapedElement(output, "first_air_date", tvShowInfo.FirstAirDate.ToString("yyyy-MM-dd"));
        WriteXmlEscapedElement(output, "origin_country", string.Join(",", tvShowInfo.OriginCountry));
        WriteXmlEscapedElement(output, "original_language", tvShowInfo.OriginalLanguage);

        // Genres
        if (tvShowInfo.Genres != null && tvShowInfo.Genres.Any())
        {
            string genres = string.Join(",", tvShowInfo.Genres.Select(g => g.Name));
            WriteXmlEscapedElement(output, "genres", genres);
        }

        // Additional episode information
        WriteXmlEscapedElement(output, "episode_id", episode.Id.ToString());
        WriteXmlEscapedElement(output, "still_path", episode.StillPath);

        if (episode.VoteCount > 0)
        {
            output.AppendLine(TAB + "<ratings>");
            output.AppendLine(TAB + TAB + "<rating namee=\"themoviedb\" max=\"10\">");
            WriteXmlEscapedElement(output, "value", episode.VoteAverage.ToString(), tabs: 3);
            WriteXmlEscapedElement(output, "votes", episode.VoteCount.ToString(), tabs: 3);
            output.AppendLine(TAB + TAB + "</rating>");
            output.AppendLine(TAB + "</ratings>");
        }

        if (episode.Crew?.Any() == true)
        {
            var writers = episode.Crew.Where(crewMember => crewMember.Department == "Writing").Select(crewMember => crewMember.Name).Distinct();
            var directors = episode.Crew.Where(crewMember => crewMember.Job == "Director").Select(crewMember => crewMember.Name).Distinct();
  
            foreach (var epWriter in writers)
            {
                WriteXmlEscapedElement(output, "credits", epWriter);
            }

            foreach (var director in directors)
            {
                WriteXmlEscapedElement(output, "director", director);
            }
        }
        
        if (episode.GuestStars?.Any() == true)
        {
            for (int i = 0; i < episode.GuestStars.Count; i++)
            {
                var guestStar = episode.GuestStars[i];
                WriteActorElement(output, guestStar.Name, guestStar.Character, i, guestStar.ProfilePath);
            }
        }

        output.AppendLine("</episodedetails>");

        return output.ToString();
    }
    
    
    private void WriteXmlEscapedElement(StringBuilder output, string elementName, string elementValue, int tabs = 1)
    {
        if (string.IsNullOrEmpty(elementValue))
            return; // Skip writing if the value is null or empty
        output.Append($"{string.Join("", Enumerable.Range(1, tabs).Select(x => TAB))}<{elementName}>");
        output.Append(XmlEscape(elementValue));
        output.AppendLine($"</{elementName}>");
    }

    private string XmlEscape(string unescaped)
    {
        XmlDocument doc = new XmlDocument();
        var node = doc.CreateElement("root");
        node.InnerText = unescaped;
        return node.InnerXml;
    }
    private void WriteActorElement(StringBuilder output, string name, string role, int order, string thumb)
    {
        output.AppendLine(TAB +"<actor>");
        WriteXmlEscapedElement(output, "name", name, tabs: 2);
        WriteXmlEscapedElement(output, "role", role, tabs: 2);
        WriteXmlEscapedElement(output, "order", order.ToString(), tabs: 2);
        if(string.IsNullOrWhiteSpace(thumb) == false)
            WriteXmlEscapedElement(output, "thumb", "https://image.tmdb.org/t/p/original" + thumb, tabs: 2);
        output.AppendLine(TAB + "</actor>");
    }
    
    private void WriteThumb(StringBuilder output, string aspect, string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;
        string preview = "https://image.tmdb.org/t/p/original" + url;
        output.AppendLine(TAB +$"<thumb aspect=\"{aspect}\" preview=\"{preview}\" />");
    }
}