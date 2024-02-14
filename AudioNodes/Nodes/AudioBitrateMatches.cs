namespace FileFlows.AudioNodes;

/// <summary>
/// Checks if a audio files bitrate matches the condition
/// </summary>
public class AudioBitrateMatches : AudioNode
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/audio-nodes/audio-bitrate-matches";
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string Icon => "fas fa-question";

    internal const string MATCH_GREATER_THAN = ">";
    internal const string MATCH_LESS_THAN = "<";
    internal const string MATCH_EQUALS = "=";
    internal const string MATCH_NOT_EQUALS = "!=";
    internal const string MATCH_GREATER_THAN_OR_EQUAL = ">=";
    internal const string MATCH_LESS_THAN_OR_EQUAL = "<=";
    
    /// <summary>
    /// Gets or sets the method to match
    /// </summary>
    [Select(nameof(MatchOptions), 1)]
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
                };
            }

            return _MatchOptions;
        }
    }

    /// <summary>
    /// Gets or sets the bitrate value to check
    /// </summary>
    [NumberInt(2)]
    public int BitrateKilobytes { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var audioInfoResult = GetAudioInfo(args);
        if (audioInfoResult.Failed(out string error))
        {
            args.Logger?.ELog(error);
            args.FailureReason = error;
            return -1;
        }

        var audioInfo = audioInfoResult.Value;

        var bitrate = audioInfo.Bitrate;
        long expected = BitrateKilobytes * 1000;

        return DoMatch(args.Logger, Match, bitrate, expected) ? 1 : 2;
    }

    /// <summary>
    /// Executes the match check
    /// </summary>
    /// <param name="logger">the logger</param>
    /// <param name="match">the match to test</param>
    /// <param name="bitrate">the actual bitrate</param>
    /// <param name="expected">the expected bitrate</param>
    /// <returns>true if matches, otherwise false</returns>
    internal static bool DoMatch(ILogger logger, string match, long bitrate, long expected)
    {
        bool matches = false;
        switch (match)
        {
            case MATCH_EQUALS:
                matches = bitrate == expected;
                break;
            case MATCH_NOT_EQUALS:
                matches = bitrate != expected;
                break;
            case MATCH_LESS_THAN:
                matches = bitrate < expected;
                break;
            case MATCH_LESS_THAN_OR_EQUAL:
                matches = bitrate <= expected;
                break;
            case MATCH_GREATER_THAN:
                matches = bitrate > expected;
                break;
            case MATCH_GREATER_THAN_OR_EQUAL:
                matches = bitrate >= expected;
                break;
        }
        
        if(matches)
            logger?.ILog($"Bitrate matches expected: {bitrate} {match} {expected}");
        else
            logger?.ILog($"Bitrate does not match expected: {bitrate} {match} {expected}");

        return matches;
    }
}