namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderRemuxToMkv : FfmpegBuilderNode
{
    public override string HelpUrl => "https://github.com/revenz/FileFlows/wiki/FFMPEG-Builder:-Remux-to-MKV";

    public override int Execute(NodeParameters args)
    {
        this.Model.Extension = "mkv";
        return 1;
    }
}