namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Flow element that adds a deinterlace filter to the video stream
/// </summary>
public class FfmpegBuilderDeinterlace : FfmpegBuilderNode
{
    /// <summary>
    /// Gets the number of outputs
    /// </summary>
    public override int Outputs => 1;

    /// <summary>
    /// Get the help URL
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/deinterlace";
    
    
    /// <summary>
    /// Gets or sets the mode for the deinterlacing
    /// </summary>
    [Select(nameof(ModeOptions), 1)]
    [DefaultValue(1)]
    public int Mode { get; set; }
    
    private static List<ListOption> _ModeOptions;
    /// <summary>
    /// Gets the mode options
    /// </summary>
    public static List<ListOption> ModeOptions
    {
        get
        {
            if (_ModeOptions == null)
            {
                _ModeOptions = new List<ListOption>
                {
                    new () { Label = "Send Frame", Value = 0 },
                    new () { Label = "Send Field", Value = 1 },
                    new () { Label = "Send Frame No Spatial", Value = 2 },
                    new () { Label = "Send Field No Spatial", Value = 3}
                };
            }
            return _ModeOptions;
        }
    }

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the parameters</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        var videoInfo = GetVideoInfo(args);
        if (videoInfo == null || videoInfo.VideoStreams?.Any() != true)
            return -1;

        var vidStream = Model.VideoStreams?.FirstOrDefault(x => x.Deleted == false);
        if (vidStream == null)
        {
            args.Logger.ILog("No video stream found");
            return 2;
        }

        vidStream.Filter.Add($"yadif={Mode}");

        return 1;
    }
}