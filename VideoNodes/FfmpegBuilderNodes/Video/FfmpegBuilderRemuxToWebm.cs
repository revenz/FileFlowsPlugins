namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderRemuxToWebm : FfmpegBuilderNode
{
    /// <inheritdoc />
    public override string Icon => "svg:webm";
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/remux-to-webm";

    public override int Execute(NodeParameters args)
    {
        this.Model.Extension = "webm";
        return 1;
    }
}