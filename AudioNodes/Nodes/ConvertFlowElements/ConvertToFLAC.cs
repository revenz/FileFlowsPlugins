namespace FileFlows.AudioNodes;

/// <summary>
/// A node used for converting audio files to the FLAC format.
/// </summary>
/// <remarks>
/// This class inherits from the <see cref="ConvertNode"/> and provides specific implementation
/// details for handling audio conversions to the FLAC format. It defines the default extension
/// for output files as "flac" and uses a specific icon for identification.
/// </remarks>
public class ConvertToFLAC : ConvertNode
{
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/audio-nodes/convert-to-flac";
    
    /// <inheritdoc />
    protected override string DefaultExtension => "flac";

    /// <inheritdoc />
    public override string Icon => "svg:flac";
}
