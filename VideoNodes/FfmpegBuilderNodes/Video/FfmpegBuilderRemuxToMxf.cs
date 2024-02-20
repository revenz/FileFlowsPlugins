namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Remuxes a file to the MXF container
/// </summary>
public class FfmpegBuilderRemuxToMxf : FfmpegBuilderNode
{
    /// <summary>
    /// Gets or sets the URL to the helper page
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/remux-to-mxf";

    /// <summary>
    /// Gets that this is an enterprise flow element
    /// </summary>
    public override bool Enterprise => true;
    
    /// <inheritdoc />
    public override string Icon => "svg:mxf";

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        this.Model.Extension = "mxf";
        return 1;
    }
}