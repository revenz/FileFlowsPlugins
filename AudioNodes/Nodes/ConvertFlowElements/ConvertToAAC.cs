namespace FileFlows.AudioNodes;

public class ConvertToAAC : ConvertNode
{
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/audio-nodes/convert-to-aac";
    /// <inheritdoc />
    protected override string DefaultExtension => "aac";
    /// <inheritdoc />
    public override string Icon => "svg:aac";

    /// <summary>
    /// Gets or sets if high efficiency should be used
    /// </summary>
    [Boolean(6)]
    public bool HighEfficiency { get => base.HighEfficiency; set =>base.HighEfficiency = value; }
}