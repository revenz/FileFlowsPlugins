namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Flow element that set language fot tracks if the language is missing
/// </summary>
public class FfmpegBuilderAudioSetLanguage : FfmpegBuilderNode
{
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/set-language";

    /// <inheritdoc />
    public override int Outputs => 2;

    /// <inheritdoc />
    public override string Icon => "fas fa-comment-dots";

    /// <summary>
    /// Gets or sets the type of stream to set the language for
    /// </summary>
    [Select(nameof(StreamTypeOptions), 1)]
    public string StreamType { get; set; }

    /// <summary>
    /// Gets or sets the language
    /// </summary>
    [Required]
    [TextVariable(2)]
    public string Language { get; set; }

    private static List<ListOption> _StreamTypeOptions;
    /// <summary>
    /// Gets the possible stream types
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

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        bool changes = false;
        string language = args.ReplaceVariables(Language, stripMissing: true)?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(language))
        {
            args.Logger?.WLog("No language set");
            return 2;
        }
        args.Logger?.ILog("Language: " + language);
        args.Logger?.ILog("Stream Type: " + StreamType);

        if (language.ToLowerInvariant().Contains("orig"))
        {
            if (Variables.TryGetValue("OriginalLanguage", out object oLang) == false || string.IsNullOrEmpty(oLang as string))
            {
                args.Logger?.ILog("OriginalLanguage not found in varaibles.");
                return 2;
            }

            language = oLang as string;
            language = LanguageHelper.GetIso2Code(language);
            args.Logger?.ILog("Using original language:" + language);
        }
        
        if (StreamType is "Subtitle" or "Both")
        {
            foreach (var at in Model.SubtitleStreams)
            {
                if (string.IsNullOrEmpty(at.Language))
                {
                    at.Language = language;
                    at.ForcedChange = true; // this will ensure the language is set even if there are no changes anywhere else
                    changes = true;
                }
            }
        }
        
        if(string.IsNullOrEmpty(StreamType) || StreamType == "Both" || StreamType == "Audio")
        {
            foreach (var at in Model.AudioStreams)
            {
                if (string.IsNullOrEmpty(at.Language))
                {
                    at.Language = Language.ToLower();
                    at.ForcedChange = true; // this will ensure the language is set even if there are no changes anywhere else
                    changes = true;
                }
            }
        }
        return changes ? 1 : 2;
    }
}
