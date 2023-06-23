namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderRemuxToMkv : FfmpegBuilderNode
{
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/remux-to-mkv";

    public override int Execute(NodeParameters args)
    {
        this.Model.Extension = "mkv";
        return 1;
    }
}