namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Changes the parameters in the final FFmpeg parameters
/// </summary>
public class FfmpegBuilderParameterReplacer : FfmpegBuilderNode
{
    /// <inheritdoc/>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/parameter-replacer";

    /// <inheritdoc/>
    public override int Outputs => 1;

    /// <inheritdoc/>
    public override string Icon => "fas fa-exchange-alt";
    
    /// <summary>
    /// Gets or sets replacements to replace
    /// </summary>
    [KeyValue(1, null)]
    [Required]
    public List<KeyValuePair<string, string>> Replacements{ get; set; }
    
    /// <inheritdoc/>
    public override int Execute(NodeParameters args)
    {
        Model.ParameterReplacements = Replacements ?? [];
        return 1;
    }
}