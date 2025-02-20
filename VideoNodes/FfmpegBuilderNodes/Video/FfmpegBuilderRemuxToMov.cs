namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderRemuxToMov : FfmpegBuilderNode
{
    /// <inheritdoc />
    public override string Icon => "svg:mov";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/remux-to-mov";

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        if (this.Model.Extension?.ToLowerInvariant() != "mov")
        {
            args.Logger?.ILog($"Needs remuxing from '{this.Model.Extension}' to 'mov', forcing encode");
            this.Model.ForceEncode = true;
        }
        this.Model.Extension = "mov";
        return 1;
    }
}