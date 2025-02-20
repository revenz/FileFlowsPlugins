namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderRemuxToMkv : FfmpegBuilderNode
{
    /// <inheritdoc />
    public override string Icon => "svg:mkv";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/remux-to-mkv";

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        if (this.Model.Extension?.ToLowerInvariant() != "mkv")
        {
            args.Logger?.ILog($"Needs remuxing from '{this.Model.Extension}' to 'mkv', forcing encode");
            this.Model.ForceEncode = true;
        }

        this.Model.Extension = "mkv";
        return 1;
    }
}