namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// FFmpeg Builder flow element to set a track as default
/// </summary>
public class FfmpegBuilderSetDefaultTrack: FfmpegBuilderNode
{
    /// <summary>
    /// Gets the help URL for the flow element
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/set-default-track";

    /// <summary>
    /// Gets the number of outputs of the flow element
    /// </summary>
    public override int Outputs => 2;

    /// <summary>
    /// Gets the icon of the flow element
    /// </summary>
    public override string Icon => "fas fa-flag";

    /// <summary>
    /// Gets or sets the stream type
    /// </summary>
    [Select(nameof(StreamTypeOptions), 1)]
    public string StreamType { get; set; }

    /// <summary>
    /// Gets or sets the language of the track to set as default
    /// </summary>
    [TextVariable(2)]
    public string Language { get; set; }
    
    /// <summary>
    /// Gets or sets the index of the track to set as default, only used if language is null or empty
    /// </summary>
    [NumberInt(3)]
    public int Index { get; set; }

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
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the flow parameters</param>
    /// <returns>the flow output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        bool changes = false;
        string language = args.ReplaceVariables(Language, stripMissing: true)?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(language))
        {
            // use index
            if (string.IsNullOrEmpty(StreamType) || StreamType is "Audio" or "Both")
            {
                if (Index >= Model.AudioStreams.Count)
                {
                    args.Logger?.WLog("Index is outside the bounds of the audio streams");
                    return 2;
                }

                for (int i = 0; i < Model.AudioStreams.Count; i++)
                {
                    if (Model.AudioStreams[i].IsDefault != (i == Index))
                    {
                        var at = Model.AudioStreams[i]; 
                        args.Logger?.ILog("Setting audio track as default: " + at.Language + " , " + at.Title + " , " + at.Index);
                        at.IsDefault = i == Index;
                        at.ForcedChange = at.Deleted == false;
                    }
                }
            }
            
            if (StreamType is "Subtitle" or "Both")
            {
                if (Index >= Model.SubtitleStreams.Count)
                {
                    args.Logger?.WLog("Index is outside the bounds of the Subtitle streams");
                    return 2;
                }

                for (int i = 0; i < Model.SubtitleStreams.Count; i++)
                {
                    if (Model.SubtitleStreams[i].IsDefault != (i == Index))
                    {
                        var at = Model.SubtitleStreams[i]; 
                        args.Logger?.ILog("Setting subtitle track as default: " + at.Language + " , " + at.Title + " , " + at.Index);
                        at.IsDefault = i == Index;
                        at.ForcedChange = at.Deleted == false;
                    }
                }
            }
            
            return 1;
        }

        bool found = false;
        if (StreamType is "Subtitle" or "Both")
        {
            foreach (var at in Model.SubtitleStreams)
            {
                if (string.IsNullOrWhiteSpace(at.Language))
                    continue;
                if(LanguageMatches(at.Language))
                {
                    args.Logger?.ILog("Setting subtitle track as default: " + at.Language + " , " + at.Title + " , " + at.Index);
                    at.IsDefault = true;
                    found = true;
                    at.ForcedChange = true;
                    break;
                }
            }
        }
        
        if(string.IsNullOrEmpty(StreamType) || StreamType is "Both" or "Audio")
        {
            foreach (var at in Model.AudioStreams)
            {
                if (string.IsNullOrWhiteSpace(at.Language))
                    continue;
                if(LanguageMatches(at.Language))
                {
                    args.Logger?.ILog("Setting audio track as default: " + at.Language + " , " + at.Title + " , " + at.Index);
                    at.IsDefault = true;
                    found = true;
                    at.ForcedChange = true;
                    break;
                }
            }
        }

        if (found)
            return 1;
        
        args.Logger.ILog("Failed to find track to set as default");
        return 2;
    }

    /// <summary>
    /// Tests if a language matches
    /// </summary>
    /// <param name="testLanguage">the language to test</param>
    /// <returns>true if matches, otherwise false</returns>
    private bool LanguageMatches(string testLanguage)
    {
        if (string.IsNullOrWhiteSpace(testLanguage))
            return false;
        if (string.IsNullOrWhiteSpace(this.Language))
            return false;
        if (testLanguage.ToLowerInvariant().Contains(this.Language.ToLowerInvariant()))
            return true;
        try
        {
            var rgx = new Regex(this.Language, RegexOptions.IgnoreCase);
            return rgx.IsMatch(testLanguage);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
