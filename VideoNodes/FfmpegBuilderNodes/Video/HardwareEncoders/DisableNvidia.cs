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

    /// <summary>
    /// Gets the help URL
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/hardware-encoders/disable-nvidia";
}