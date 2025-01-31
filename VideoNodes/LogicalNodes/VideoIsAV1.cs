namespace FileFlows.VideoNodes;

/// <summary>
/// Video is AV1
/// </summary>
public class VideoIsAV1 : VideoIsCodec
{
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/video-is-av1";

    /// <inheritdoc />
    protected override bool CodecMatches(string codec)
        => codec.Contains("av1", StringComparison.InvariantCultureIgnoreCase);
}