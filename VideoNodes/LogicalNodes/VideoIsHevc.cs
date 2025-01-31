namespace FileFlows.VideoNodes;

/// <summary>
/// Video is HEVC
/// </summary>
public class VideoIsHevc : VideoIsCodec
{
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/video-is-hevc";
    
    /// <inheritdoc />
    protected override bool CodecMatches(string codec)
        => codec.ToLowerInvariant() is "hevc" or "h265" or "265" or "h.265";
}