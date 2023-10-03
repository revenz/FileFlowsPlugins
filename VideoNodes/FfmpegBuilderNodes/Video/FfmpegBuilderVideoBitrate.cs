namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderVideoBitrate : FfmpegBuilderNode
{
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/video-bitrate";
    
    /// <summary>
    /// Gets or sets the bitrate in K
    /// </summary>
    [NumberInt(1)]
    [DefaultValue(3000)]
    public float Bitrate { get; set; }
    
    [Boolean(2)]
    public bool Percent { get; set; }


    public override int Execute(NodeParameters args)
    {
        var video = Model.VideoStreams?.Where(x => x.Deleted == false)?.FirstOrDefault();
        if (video?.Stream == null)
        {
            args.Logger?.ELog("No video stream found");
            return -1;
        }
        if(Bitrate < 0)
        {
            args.Logger?.ELog("Minimum birate not set");
            return -1;
        }
        float currentBitrate = (int)(video.Stream.Bitrate / 1000f);
        if (currentBitrate <= 0 && Model.VideoInfo.Bitrate > 0)
            currentBitrate = (int)(Model.VideoInfo.Bitrate/ 1000f);
        if (currentBitrate <= 0)
        {
            // need to work it out                
            currentBitrate = args.WorkingFileSize;
            //currentBitrate /= 1000f;
            currentBitrate = (float)(currentBitrate / video.Stream.Duration.TotalSeconds);
            // rough estimate of 75% of the file is video
            currentBitrate *= 0.75f;
            currentBitrate /= 100;
        }

        float br = Bitrate;
        if (Percent)
            br = currentBitrate * (Bitrate / 100f);

        br = (int)Math.Round((double)br, 0);
        currentBitrate = ((int)Math.Round((double)currentBitrate, 0));
        int minimum = (int)(br * 0.75f);
        int maximum = (int)(br * 1.25f);
        args.Logger?.ILog($"Source bitrate: {currentBitrate}k");
        args.Logger?.ILog($"Setting video bitrate to: {br}k");

        video.AdditionalParameters.AddRange(new[]
        {
            "-b:v:{index}",  br + "k",
            "-minrate", minimum + "k",
            "-maxrate", maximum + "k",
            "-bufsize", currentBitrate + "k"
        });

        return 1;
    }
}
