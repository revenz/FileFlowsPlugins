namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderRemuxToMP4: FfmpegBuilderNode
{
    public override string HelpUrl => "https://github.com/revenz/FileFlows/wiki/FFMPEG-Builder:-Remux-to-MP4";

    public override int Execute(NodeParameters args)
    {
        this.Model.Extension = "mp4";
        return 1;
    }
}
