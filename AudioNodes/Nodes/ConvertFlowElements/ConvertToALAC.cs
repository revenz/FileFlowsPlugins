namespace FileFlows.AudioNodes;

/// <summary>
/// A node for converting an audio file to ALAC (Apple Lossless Audio Codec).
/// </summary>
public class ConvertToALAC : ConvertNode
{
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/audio-nodes/convert-to-alac";

    /// <inheritdoc />
    protected override string DefaultExtension => "alac";

    /// <inheritdoc />
    public override string Icon => "svg:alac";
}