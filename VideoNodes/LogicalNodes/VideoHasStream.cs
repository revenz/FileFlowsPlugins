using FileFlows.VideoNodes.FfmpegBuilderNodes;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using FileFlows.VideoNodes.Helpers;

namespace FileFlows.VideoNodes;

/// <summary>
/// Flow element to test if a video has a stream
/// </summary>
public class VideoHasStream : VideoNode
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
    /// Gets the type of flow element
    /// </summary>
    public override FlowElementType Type => FlowElementType.Logic;
    /// <summary>
    /// Gets the help URL 
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/video-has-stream";

    /// <summary>
    /// Gets or sets the type of stream to check for
    /// </summary>
    [Select(nameof(StreamTypeOptions), 1)]
    public string Stream { get; set; }

    private static List<ListOption> _StreamTypeOptions;
    /// <summary>
    /// Gets the types of streams available to check for
    /// </summary>
    public static List<ListOption> StreamTypeOptions
    {
        get
        {
            if (_StreamTypeOptions == null)
            {
                _StreamTypeOptions = new List<ListOption>
                {
                    new ListOption { Label = "Video", Value = "Video" },
                    new ListOption { Label = "Audio", Value = "Audio" },
                    new ListOption { Label = "Subtitle", Value = "Subtitle" }
                };
            }
            return _StreamTypeOptions;
        }
    }

    /// <summary>
    /// Gets or sets the title to look for
    /// </summary>
    [TextVariable(2)]
    public string Title { get; set; }
    
    /// <summary>
    /// Gets or sets the codec to look for
    /// </summary>
    [TextVariable(3)]
    public string Codec { get; set; }
    
    /// <summary>
    /// Gets or sets the language to look for
    /// </summary>
    [ConditionEquals(nameof(Stream), "Video", inverse: true)]
    [TextVariable(4)]
    public string Language { get; set; }
    
    /// <summary>
    /// Gets or sets the number of channels to look for
    /// </summary>
    [ConditionEquals(nameof(Stream), "Audio")]
    [NumberFloat(5)]
    public float Channels { get; set; }
    
    /// <summary>
    /// Gets or sets if deleted tracks should also be checked
    /// </summary>
    [Boolean(6)]
    public bool CheckDeleted { get; set; }
    
    /// <summary>
    /// Gets or sets if result should be inverted
    /// </summary>
    [Boolean(7)]
    public bool Invert { get; set; }

    /// <summary>
    /// Tries to get the FFmpegModel if loaded into variables
    /// </summary>
    /// <param name="args">The node parameters</param>
    /// <returns>the FFmpeg model if exists</returns>
    protected FfmpegModel GetFfmpegModel(NodeParameters args)
    {
        if (args.Variables.TryGetValue(FfmpegBuilderNode.MODEL_KEY, out var variable) &&
            variable is FfmpegModel ffmpegModel)
        {
            args.Logger?.ILog("FFmpeg Model found and will be used.");
            return ffmpegModel;
        }
        args.Logger?.ILog("FFmpeg Model not found, using VideoInfo.");

        return null;
    }

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the arguments</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        var videoInfo = GetVideoInfo(args);
        if (videoInfo == null)
            return -1;

        bool found = false;
        string title = args.ReplaceVariables(Title, stripMissing: true);
        string codec = args.ReplaceVariables(Codec, stripMissing: true);
        string lang = args.ReplaceVariables(Language, stripMissing: true);
        var ffmpegModel = GetFfmpegModel(args);
        if (ffmpegModel != null && CheckDeleted)
        {
            args.Logger?.ILog("Checking deleted, ignoring FFmpeg model");
            ffmpegModel = null;
        }

        if (this.Stream == "Video")
        {
            var streams = ffmpegModel == null
                ? videoInfo.VideoStreams
                : ffmpegModel.VideoStreams.Where(x => x.Deleted == false).Select(x => x.Stream).ToList();
            
            found = streams.Where(x =>
            {
                if (ValueMatch(title, x.Title) == MatchResult.NoMatch)
                    return false;
                if (string.IsNullOrWhiteSpace(x.CodecTag) == false && ValueMatch(codec, x.CodecTag) == MatchResult.Matched)
                    return true;
                if (ValueMatch(codec, x.Codec) == MatchResult.NoMatch)
                    return false;
                return true;
            }).Any();
        }
        else if (this.Stream == "Audio")
        {
            var streams = ffmpegModel == null
                ? videoInfo.AudioStreams
                : ffmpegModel.AudioStreams.Where(x => x.Deleted == false).Select(x => x.Stream).ToList();
            
            found = streams.Where(x =>
            {
                if (ValueMatch(title, x.Title) == MatchResult.NoMatch)
                    return false;
                if (ValueMatch(codec, x.Codec) == MatchResult.NoMatch)
                    return false;
                if (ValueMatch(lang, x.Language) == MatchResult.NoMatch)
                    return false;
                if (this.Channels > 0 && Math.Abs(x.Channels - this.Channels) > 0.05f)
                    return false;
                return true;
            }).Any();
        }
        else if (this.Stream == "Subtitle")
        {   
            var streams = ffmpegModel == null
                ? videoInfo.SubtitleStreams
                : ffmpegModel.SubtitleStreams.Where(x => x.Deleted == false).Select(x => x.Stream).ToList();
            
            found = streams.Where(x =>
            {
                if (ValueMatch(title, x.Title) == MatchResult.NoMatch)
                    return false;
                if (ValueMatch(codec, x.Codec) == MatchResult.NoMatch)
                    return false;
                if (ValueMatch(lang, x.Language) == MatchResult.NoMatch)
                    return false;
                return true;
            }).Any();
        }

        args.Logger?.ILog("Found stream: " + found);
        if (Invert)
        {
            args.Logger?.ILog("Invert result");
            found = !found;
        }

        return found ? 1 : 2;
    }

    /// <summary>
    /// Tests if a value matches the pattern
    /// </summary>
    /// <param name="pattern">the pattern</param>
    /// <param name="value">the value</param>
    /// <returns>the result</returns>
    private MatchResult ValueMatch(string pattern, string value)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return MatchResult.Skipped;
        try
        {            

            if (string.IsNullOrEmpty(value))
                return MatchResult.NoMatch;
            
            if (GeneralHelper.IsRegex(pattern))
            {
                var rgx = new Regex(pattern, RegexOptions.IgnoreCase);
                if (rgx.IsMatch(value))
                    return MatchResult.Matched;
            }

            if (value.ToLower() == "hevc" && (pattern.ToLower() == "h265" || pattern == "265" || pattern.ToLower() == "h.265"))
                return MatchResult.Matched; // special case

            return pattern.ToLowerInvariant().Trim() == value.ToLowerInvariant().Trim()
                ? MatchResult.Matched
                : MatchResult.NoMatch;
        }
        catch (Exception)
        {
            return MatchResult.NoMatch;
        }
    }

    /// <summary>
    /// Match results 
    /// </summary>
    private enum MatchResult
    {
        /// <summary>
        /// No Match
        /// </summary>
        NoMatch = 0,
        /// <summary>
        /// Matched
        /// </summary>
        Matched = 1,
        /// <summary>
        /// Skipped
        /// </summary>
        Skipped = 2
    }
}
