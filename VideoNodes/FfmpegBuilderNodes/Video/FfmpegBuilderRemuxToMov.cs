namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderRemuxToMov : FfmpegBuilderNode
{
    /// <inheritdoc />
    public override string Icon => "svg:mov";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/remux-to-mov";

    public override int Execute(NodeParameters args)
    {
        this.Model.Extension = "mov";
        return 1;
    }
}