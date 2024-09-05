namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Flow element that allows you to cut a video short
/// </summary>
public class FFmpegBuilderDurationStart : FfmpegBuilderNode
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/duration-start";
    /// <inheritdoc />
    public override string Icon => "fas fa-clock";
    
    
    /// <summary>
    /// Gets or sets the time for this file
    /// </summary>
    [Time(1)]
    public TimeSpan Duration { get; set; }
    /// <summary>
    /// Gets or sets when to start the cut
    /// </summary>
    [Time(2)]
    public TimeSpan Start { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var model = GetModel();
        var videoDuration = model.VideoInfo.VideoStreams[0].Duration;
        if (Duration.TotalSeconds == 0 && Start.TotalSeconds == 0)
        {
            args.Logger?.ILog("Neither Duration or Start have been set");
            return 2;
        }

        if (Duration >= videoDuration)
        {
            args.Logger?.ILog("Video is shorter than the time requested.");
            return 2;
        }
        

        if (Start > videoDuration)
        {
            args.Logger?.ILog("The video is shorter than the starting time");
            return 2;
        }

        if (Start + Duration > videoDuration)
        {
            args.Logger?.ILog("The video is shorter than the starting time and duration combined");
            return 2;
        }

        model.StartTime = Start;
        model.CutDuration = Duration;
        args.Logger?.ILog($"Cutting the video from '{Start}' for '{Duration}'");
        return 1;
    }
}