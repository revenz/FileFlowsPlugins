namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Disabled AMD encoding
/// </summary>
public class DisableAmd:DisableEncoder
{
    /// <summary>
    /// Gets the encoder variable
    /// </summary>
    protected override string EncoderVariable => "NoAMD";
    
    /// <inheritdoc />
    public override string Icon => "svg:amd";

    /// <summary>
    /// Gets the help URL
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/hardware-encoders/disable-amd";
}