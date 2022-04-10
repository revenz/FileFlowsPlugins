namespace FileFlows.VideoNodes.FfmpegBuilderNodes
{
    public class FfmpegBuilderCropBlackBars : FfmpegBuilderNode
    {
        [NumberInt(1)]
        [DefaultValue(10)]
        public int CroppingThreshold { get; set; }
        public override int Outputs => 2;
        public override int Execute(NodeParameters args)
        {
            base.Init(args);

            string ffmpeg = GetFFMpegExe(args);
            if (string.IsNullOrEmpty(ffmpeg))
                return -1;

            var videoInfo = GetVideoInfo(args);
            if (videoInfo == null || videoInfo.VideoStreams?.Any() != true)
                return -1;


            string crop = DetectBlackBars.Detect(ffmpeg, videoInfo, args, this.CroppingThreshold);
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
}
