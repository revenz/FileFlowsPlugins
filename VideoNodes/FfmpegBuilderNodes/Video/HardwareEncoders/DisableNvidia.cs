namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Disabled NVIDIA encoding
/// </summary>
public class DisableNvidia:DisableEncoder
{
    /// <summary>
    /// Gets the encoder variable
    /// </summary>
    protected override string EncoderVariable => "NoNvidia";
    
    /// <inheritdoc />
    public override string Icon => "svg:nvidia";

    /// <summary>
    /// Gets the help URL
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/hardware-encoders/disable-nvidia";
}