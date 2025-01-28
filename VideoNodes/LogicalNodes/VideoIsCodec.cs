namespace FileFlows.VideoNodes;

/// <summary>
/// Flow element to test a video is a specific codec
/// </summary>
public abstract class VideoIsCodec: VideoNode
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string Icon => "fas fa-question";

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the arguments</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        var videoInfo = GetVideoInfo(args);
        if (videoInfo == null)
            return args.Fail("Failed to retrieve video info");

        var matches = videoInfo.VideoStreams.Any(x => CodecMatches(x.Codec));
        args.Logger?.ILog($"Codec is {(matches ? "" : "not ")}a match");
        return matches ? 1 : 2;
    }

    /// <summary>
    /// Performs the check if the codec matches, return true if a match, otherwise false
    /// </summary>
    /// <param name="codec">the codec to test</param>
    /// <returns>true if matches, otherwise false</returns>
    protected abstract bool CodecMatches(string codec);
}