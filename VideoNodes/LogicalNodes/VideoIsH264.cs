namespace FileFlows.VideoNodes;

/// <summary>
/// Video is H264
/// </summary>
public class VideoIsH264 : VideoIsCodec
{
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/video-is-h264";
    
    /// <inheritdoc />
    protected override bool CodecMatches(string codec)
        => codec.ToLowerInvariant() is "h264" or "h.264" or "264";
}