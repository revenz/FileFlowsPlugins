namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Disabled Intel QSV encoding
/// </summary>
public class DisableIntelQsv:DisableEncoder
{
    /// <summary>
    /// Gets the encoder variable
    /// </summary>
    protected override string EncoderVariable => "NoQSV";

    /// <summary>
    /// Gets the help URL
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/hardware-encoders/disable-intel-qsv";
}