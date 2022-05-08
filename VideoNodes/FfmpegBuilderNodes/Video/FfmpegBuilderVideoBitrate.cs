namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderVideoBitrate : FfmpegBuilderNode
{
    public override string HelpUrl => "https://github.com/revenz/FileFlows/wiki/FFMPEG-Builder:-Video-Bitrate";
    
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
        base.Init(args);

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
        
        float currentBitrate = (int)(video.Stream.Bitrate / 1024f);
        if(currentBitrate <= 0)
        {
            // need to work it out                
            currentBitrate = (float)(args.WorkingFileSize / video.Stream.Duration.TotalSeconds);
            // rough estimate of 75% of the file is video
            currentBitrate *= 0.75f;
        }

        float br = Bitrate;
        if (Percent)
            br = currentBitrate * (Bitrate / 100f);
        

        int minimum = (int)(br * 0.75f);
        int maximum = (int)(br * 1.25f);
        
        video.AdditionalParameters.AddRange(new[]
        {
            "-b:v:{index}", Bitrate + "k",
            "-minrate", minimum + "k",
            "-maxrate", maximum + "k",
            "-bufsize", currentBitrate + "k"
        });

        return 1;
    }
}
