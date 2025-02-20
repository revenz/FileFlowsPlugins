namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Remuxes a file to webm
/// </summary>
public class FfmpegBuilderRemuxToWebm : FfmpegBuilderNode
{
    /// <inheritdoc />
    public override string Icon => "svg:webm";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/remux-to-webm";

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        if (this.Model.Extension?.ToLowerInvariant() != "webm")
        {
            args.Logger?.ILog($"Needs remuxing from '{this.Model.Extension}' to 'webm', forcing encode");
            this.Model.ForceEncode = true;
        }
        this.Model.Extension = "webm";
        return 1;
    }
}