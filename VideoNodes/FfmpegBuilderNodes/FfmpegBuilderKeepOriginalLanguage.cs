using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// FFmpeg Builder flow element to keep the original language track
/// </summary>
public class FfmpegBuilderKeepOriginalLanguage: FfmpegBuilderNode
{
    /// <summary>
    /// Gets the help URL for the flow element
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/keep-original-language";

    /// <summary>
    /// Gets the number of outputs of the flow element
    /// </summary>
    public override int Outputs => 2;

    /// <summary>
    /// Gets the icon of the flow element
    /// </summary>
    public override string Icon => "fas fa-globe";

    /// <summary>
    /// Gets or sets the stream type
    /// </summary>
    [Select(nameof(StreamTypeOptions), 1)]
    public string StreamType { get; set; }

    private static List<ListOption> _StreamTypeOptions;
    /// <summary>
    /// Gets the stream options to show in the UI
    /// </summary>
    public static List<ListOption> StreamTypeOptions
    {
        get
        {
            if (_StreamTypeOptions == null)
            {
                _StreamTypeOptions = new List<ListOption>
                {
                    new () { Label = "Audio", Value = "Audio" },
                    new () { Label = "Subtitle", Value = "Subtitle" },
                    new () { Label = "Both", Value = "Both" },
                };
            }
            return _StreamTypeOptions;
        }
    }

    /// <summary>
    /// Gets or sets the languages
    /// </summary>
    [StringArray(2)]
    public List<string> AdditionalLanguages { get; set; }
    
    /// <summary>
    /// Gets or sets if only the first of each language should be kept
    /// </summary>
    [Boolean(3)]
    public bool KeepOnlyFirst { get; set; }
    
    /// <summary>
    /// Gets or sets if the first stream should be kept if no other streams match
    /// </summary>
    [Boolean(4)]
    public bool FirstIfNone { get; set; }
    
    /// <summary>
    /// Gets or sets if tracks with no language should be treated as the original langauge
    /// </summary>
    [Boolean(5)]
    public bool TreatEmptyAsOriginal { get; set; }

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the flow parameters</param>
    /// <returns>the flow output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        string? originalLanguage;
        if (args.Variables.TryGetValue("OriginalLanguage", out object? oValue) == false ||
            string.IsNullOrWhiteSpace(originalLanguage = oValue as string))
        {
            args.Logger?.ILog("OriginalLanguage variable was not set.");
            return 2;
        }
        args.Logger?.ILog("OriginalLanguage: " + originalLanguage);
        if (AdditionalLanguages?.Any() == true)
        {
            foreach (var lang in AdditionalLanguages)
            {
                args.Logger?.ILog("Additional Language: " + lang);
            }
        }

        int changes = 0;
        if(StreamType is "Audio" or "Both")
        {
            args.Logger?.ILog("Processing Audio Streams");
            changes += ProcessStreams(args, Model.AudioStreams, originalLanguage);
        }
        if(StreamType is "Subtitle" or "Both")
        {
            args.Logger?.ILog("Processing Subtitle Streams");
            changes += ProcessStreams(args, Model.SubtitleStreams, originalLanguage);
        }
        args.Logger?.ILog("Changes: " + changes);

        return changes > 0 ? 1 : 2;
    }

    /// <summary>
    /// Processes the streams 
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <param name="streams">the streams to process for deletion</param>
    /// <param name="originalLanguage">the original language of the source material</param>
    /// <typeparam name="T">the stream type</typeparam>
    /// <returns>the number of streams changed</returns>
    private int ProcessStreams<T>(NodeParameters args, List<T> streams, string originalLanguage) where T : FfmpegStream
    {
        if (streams?.Any() != true)
            return 0;
        
        int changed = 0;
        bool firstStreamDeleted = streams[0].Deleted;
        var foundLanguages = new List<string>();
        foreach (var stream in streams)
        {
            bool deleted;
            if (TreatEmptyAsOriginal && string.IsNullOrWhiteSpace(stream.Language))
                deleted = false;
            else
                deleted = KeepStream(args.Logger, originalLanguage, stream.Language) == false;

            if (deleted == false)
            {
                string lang = LanguageHelper.GetIso2Code(stream.Language?.EmptyAsNull() ?? originalLanguage);
                if (foundLanguages.Contains(lang) == false)
                    foundLanguages.Add(lang);
                else if (KeepOnlyFirst)
                    deleted = true;
            }
            
            if (stream.Deleted == deleted)
                continue;
            stream.Deleted = deleted;
            ++changed;
            args.Logger?.ILog($"Stream '{stream.GetType().Name}' '{stream.Language}' " + (deleted ? "deleted" : "restored"));
        }

        if (FirstIfNone && streams.Any(x => x.Deleted == false) == false)
        {
            if (firstStreamDeleted == false)
            {
                --changed; // remove the change
            }
            streams[0].Deleted = false;
            args.Logger?.ILog($"Stream '{streams[0].GetType().Name}' '{streams[0].Language}' restored as only stream");
        }

        return changed;
    }

    private bool KeepStream(ILogger logger, string originalLanguage, string streamLanguage)
    {
        if (LanguageMatches(streamLanguage, originalLanguage))
        {
            logger?.ILog($"Language '{streamLanguage}' matches original language '{originalLanguage}'");
            return true;
        }

        if (AdditionalLanguages?.Any() != true)
        {
            logger?.ILog($"Language '{streamLanguage}' did not match.");
            return false;
        }

        foreach (var lang in this.AdditionalLanguages)
        {
            if (LanguageMatches(streamLanguage, lang))
            {
                logger?.ILog($"Language '{streamLanguage}' matches additional language '{lang}'");
                return true;
            }
        }

        logger?.ILog($"Language '{streamLanguage}' did not match any wanted language.");
        return false;
    }

    /// <summary>
    /// Tests if a language matches
    /// </summary>
    /// <param name="streamLanguage">the language of ths stream</param>
    /// <param name="testLanguage">the language to test</param>
    /// <returns>true if matches, otherwise false</returns>
    private bool LanguageMatches(string streamLanguage, string testLanguage)
    {
        if (string.IsNullOrWhiteSpace(testLanguage))
            return false;
        if (string.IsNullOrWhiteSpace(streamLanguage))
            return false;
        if (testLanguage.ToLowerInvariant().Contains(streamLanguage.ToLowerInvariant()))
            return true;
        try
        {
            if (LanguageHelper.GetIso2Code(streamLanguage) == LanguageHelper.GetIso2Code(testLanguage))
                return true;
        }
        catch (Exception)
        {
        }

        try
        {
            if (LanguageHelper.GetIso1Code(streamLanguage) == LanguageHelper.GetIso1Code(testLanguage))
                return true;
        }
        catch (Exception)
        {
        }

        try
        {
            var rgx = new Regex(testLanguage, RegexOptions.IgnoreCase);
            return rgx.IsMatch(streamLanguage);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
