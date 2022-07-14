//namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

///// <summary>
///// Node that limits the bitrate for video
///// </summary>
//public class FfmpegBuilderVideoMaxBitrate : FfmpegBuilderNode
//{
//    public override string HelpUrl => "https://docs.fileflows.com/plugins/video-nodes/ffmpeg-builder/video-max-bitrate";
    
//    /// <summary>
//    /// Gets or sets the maximum bitrate in K
//    /// </summary>
//    [NumberInt(1)]
//    [DefaultValue(10_000)]
//    public float Bitrate { get; set; }


//    public override int Execute(NodeParameters args)
//    {
//        var video = Model.VideoStreams?.Where(x => x.Deleted == false)?.FirstOrDefault();
//        if (video?.Stream == null)
//        {
//            args.Logger?.ELog("No video stream found");
//            return -1;
//        }
//        if(Bitrate < 0)
//        {
//            args.Logger?.ELog("Minimum bitrate not set");
//            return -1;
//        }

//        video.AdditionalParameters.AddRange(new[]
//        {
//            "-b:v:{index}",
//            "-maxrate", Bitrate + "k"
//        });

//        return 1;
//    }
//}
