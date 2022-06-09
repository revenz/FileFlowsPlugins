namespace FileFlows.VideoNodes.FfmpegBuilderNodes;
public class FfmpegBuilderCropBlackBars : FfmpegBuilderNode
{
    [NumberInt(1)]
    [DefaultValue(10)]
    public int CroppingThreshold { get; set; }
    public override int Outputs => 2;

    public override string HelpUrl => "https://docs.fileflows.com/plugins/video-nodes/ffmpeg-builder/crop-black-bars";

    public override int Execute(NodeParameters args)
    {
        var videoInfo = GetVideoInfo(args);
        if (videoInfo == null || videoInfo.VideoStreams?.Any() != true)
            return -1;


        string crop = DetectBlackBars.Detect(FFMPEG, videoInfo, args, this.CroppingThreshold);
        if (string.IsNullOrWhiteSpace(crop))
            return 2;

        //var parts = crop.Split(':');
        ////parts[2] = "iw-" + parts[2];
        ////parts[3] = "ih-" + parts[3];
        //crop = String.Join(":", parts.Take(2));

        args.Logger?.ILog("Black bars detected, crop: " + crop);

        var video = Model.VideoStreams[0];
        video.Filter.AddRange(new[] { "crop=" + crop });
        return 1;
    }
}