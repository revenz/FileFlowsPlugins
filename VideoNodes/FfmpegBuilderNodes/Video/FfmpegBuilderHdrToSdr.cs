namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderHdrToSdr : FfmpegBuilderNode
{
    public override int Outputs => 2;

    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/hdr-to-sdr";

    public override int Execute(NodeParameters args)
    {
        var videoInfo = GetVideoInfo(args);
        if (videoInfo == null || videoInfo.VideoStreams?.Any() != true)
            return -1;

        var vidStream = Model.VideoStreams?.Where(x => x.Deleted == false && x.Stream?.HDR == true).FirstOrDefault();
        if (vidStream == null)
        {
            args.Logger.ILog("No HDR video stream found");
            return 2;
        }

        vidStream.Filter.Add("zscale=t=linear:npl=100,format=gbrpf32le,zscale=p=bt709,tonemap=tonemap=hable:desat=0,zscale=t=bt709:m=bt709:r=tv,format=yuv420p");

        return 1;
    }
}