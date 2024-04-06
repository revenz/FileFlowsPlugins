using FileFlows.VideoNodes.FfmpegBuilderNodes;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using FileFlows.VideoNodes.Helpers;

namespace FileFlows.VideoNodes;

/// <summary>
/// Flow element to test if a video duration matches the parameters
/// </summary>
public class VideoDuration : VideoNode
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
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/video-duration";


    internal const string MATCH_GREATER_THAN = ">";
    internal const string MATCH_LESS_THAN = "<";
    internal const string MATCH_EQUALS = "=";
    internal const string MATCH_NOT_EQUALS = "!=";
    internal const string MATCH_GREATER_THAN_OR_EQUAL = ">=";
    internal const string MATCH_LESS_THAN_OR_EQUAL = "<=";
    internal const string MATCH_BETWEEN = "||";
    internal const string MATCH_NOT_BETWEEN = "^|";
    /// <summary>
    /// Gets or sets the method to match
    /// </summary>
    [Select(nameof(MatchOptions), 1)]
    [DefaultValue(MATCH_LESS_THAN)]
    public string Match { get; set; }

    private static List<ListOption> _MatchOptions;

    /// <summary>
    /// Gets the match options
    /// </summary>
    public static List<ListOption> MatchOptions
    {
        get
        {
            if (_MatchOptions == null)
            {
                _MatchOptions = new List<ListOption>
                {
                    new () { Label = "Equals", Value = MATCH_EQUALS},
                    new () { Label = "Not Equals", Value = MATCH_NOT_EQUALS},
                    new () { Label = "Less Than", Value = MATCH_LESS_THAN},
                    new () { Label = "Less Than Or Equal", Value = MATCH_LESS_THAN_OR_EQUAL},
                    new () { Label = "Greater Than", Value = MATCH_GREATER_THAN},
                    new () { Label = "Greater Than Or Equal", Value = MATCH_GREATER_THAN_OR_EQUAL},
                    new () { Label = "Between", Value = MATCH_BETWEEN},
                    new () { Label = "Not Between", Value = MATCH_NOT_BETWEEN},
                };
            }

            return _MatchOptions;
        }
    }

    /// <summary>
    /// Gets or sets the first value to match against
    /// </summary>
    [Time(2)]
    public TimeSpan ValueLow { get; set; }
    /// <summary>
    /// Gets or sets the second value to match against
    /// </summary>
    [Time(3)]
    [ConditionEquals(nameof(Match), @"/\|/")]
    public TimeSpan ValueHigh { get; set; }

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the arguments</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        var video = GetVideoInfo(args)?.VideoStreams?.FirstOrDefault();
        if (video == null)
        {
            args.FailureReason = "Failed to retrieve video info";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        int seconds = (int)Math.Round(video.Duration.TotalSeconds);
        args.Logger?.ILog("Total Seconds: " + seconds);
        args.Logger?.ILog("Match Method: " + Match);
        int val1 = (int)ValueLow.TotalSeconds;
        int val2 = (int)ValueHigh.TotalSeconds;

        switch (Match)
        {
            case MATCH_EQUALS:
                args.Logger?.ILog("Match Value: " + val1);
                return seconds == val1 ? 1 : 2;
            case MATCH_NOT_EQUALS:
                args.Logger?.ILog("Match Value: " + val1);
                return seconds != val1 ? 1 : 2;
            case MATCH_LESS_THAN:
                args.Logger?.ILog("Match Value: " + val1);
                return seconds < val1 ? 1 : 2;
            case MATCH_LESS_THAN_OR_EQUAL:
                args.Logger?.ILog("Match Value: " + val1);
                return seconds <= val1 ? 1 : 2;
            case MATCH_GREATER_THAN:
                args.Logger?.ILog("Match Value: " + val1);
                return seconds > val1 ? 1 : 2;
            case MATCH_GREATER_THAN_OR_EQUAL:
                args.Logger?.ILog("Match Value: " + val1);
                return seconds >= val1 ? 1 : 2;
            case MATCH_BETWEEN:
                args.Logger?.ILog("Between: " + val1);
                args.Logger?.ILog("And: " + val2);
                return seconds >= val1 && seconds <= val2  ? 1 : 2;
            case MATCH_NOT_BETWEEN:
                args.Logger?.ILog("Not Between: " + val1);
                args.Logger?.ILog("And: " + val2);
                return seconds >= val1 && seconds <= val2 ? 2 : 1;
        }
        args.Logger?.ELog("Invalid match");
        return 2;
    }
}
