using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Flow element that just removes languages
/// </summary>
public class FFmpegBuilderLanguageRemover: FfmpegBuilderNode
{
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/language-remover";

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
    [StringArray(2)]
    public string[] Languages { get; set; }
    
    /// <summary>
    /// Gets or sets if the language should be removed if it does not match
    /// </summary>
    [Boolean(3)]
    public bool NotMatching { get; set; }

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
        if (Languages?.Any() == false)
        {
            args.Logger?.WLog("No languages set");
            return 2;
        }
        args.Logger?.ILog("Languages: " + string.Join(",", Languages));
        args.Logger?.ILog("Not Matching: " + NotMatching);
        int changes = 0;
        if(StreamType is "Audio" or "Both")
        {
            args.Logger?.ILog("Processing Audio Streams");
            changes += ProcessStreams(args, Model.AudioStreams);
        }
        if(StreamType is "Subtitle" or "Both")
        {
            args.Logger?.ILog("Processing Subtitle Streams");
            changes += ProcessStreams(args, Model.SubtitleStreams);
        }
        args.Logger?.ILog("Changes: " + changes);

        return changes > 0 ? 1 : 2;
    }

    /// <summary>
    /// Processes the streams 
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <param name="streams">the streams to process for deletion</param>
    /// <typeparam name="T">the stream type</typeparam>
    /// <returns>the number of streams changed</returns>
    private int ProcessStreams<T>(NodeParameters args, List<T> streams) where T : FfmpegStream
    {
        if (streams?.Any() != true)
            return 0;
        
        int changed = 0;
        foreach (var stream in streams)
        {
            if (stream.Deleted)
                continue;

            bool matches = false;
            for(int i=0;i<Languages.Length;i++)
            {
                var language = Languages[i];
                if (string.IsNullOrWhiteSpace(stream.Language))
                    continue;

                if (language.ToLowerInvariant() is "orig" or "originallanguage" or "original")
                    language = "{OriginalLanguage}";
                language = args.ReplaceVariables(language, stripMissing: true);
                if (string.IsNullOrWhiteSpace(language))
                    continue;
                
                matches = LanguageHelper.Matches(args, language, stream.Language);
                if (matches)
                    break;
            }
            if(NotMatching)
                matches = !matches;
            
            if(matches == false)
                continue;
            
            stream.Deleted = true;
            ++changed;
            args.Logger?.ILog($"Stream '{stream.GetType().Name}' '{stream}' deleted");
        }

        return changed;
    }
}
