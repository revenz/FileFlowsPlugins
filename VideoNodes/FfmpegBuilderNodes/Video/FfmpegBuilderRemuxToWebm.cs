namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderRemuxToWebm : FfmpegBuilderNode
{
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/remux-to-webm";

    public override int Execute(NodeParameters args)
    {
        this.Model.Extension = "webm";
        return 1;
    }
}