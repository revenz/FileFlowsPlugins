namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Disabled VAAPI encoding
/// </summary>
public class DisableVaapi:DisableEncoder
{
    /// <summary>
    /// Gets the encoder variable
    /// </summary>
    protected override string EncoderVariable => "NoVAAPI";

    /// <summary>
    /// Gets the help URL
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/hardware-encoders/disable-vaapi";
}