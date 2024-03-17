namespace FileFlows.AudioNodes;

public class ConvertToMP3 : ConvertNode
{
    /// <inheritdoc />  
    public override string HelpUrl => "https://fileflows.com/docs/plugins/audio-nodes/convert-to-mp3";
    /// <inheritdoc />
    protected override string Extension => "mp3";

    /// <inheritdoc />
    public override string Icon => "svg:mp3";
}