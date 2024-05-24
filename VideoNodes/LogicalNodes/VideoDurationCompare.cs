namespace FileFlows.VideoNodes;

/// <summary>
/// Tests if a file video duration is within a range of the original video
/// </summary>
public class VideoDurationCompare : VideoNode
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/video-duration-compare";
    /// <inheritdoc />
    public override string Icon => "fas fa-clock";
    
    /// <summary>
    /// Gets or sets the allowed time difference from the original
    /// </summary>
    [Time(1)]
    public TimeSpan AllowedDifference { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var video = GetVideoInfo(args)?.VideoStreams?.FirstOrDefault();
        if (video == null)
        {
            args.FailureReason = "Failed to retrieve video info";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        if (args.Variables.TryGetValue("vi.OriginalDuration", out var oDuration) == false ||
            oDuration is TimeSpan originalDuration == false)
        {
            args.FailureReason = "Original duration for the video file not set";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        var finalDuration = video.Duration;
        
        args.Logger?.ILog("Original Duration: " + originalDuration);
        args.Logger?.ILog("Final Duration: " + finalDuration);
        args.Logger?.ILog("Allowed Difference: " + AllowedDifference);
        
        var diff = (finalDuration - originalDuration).Duration(); // Calculate the absolute difference
        args.Logger?.ILog("Difference: " + diff);

        if (diff > AllowedDifference)
        {
            args.Logger?.ILog("Time difference is greater than the allowed difference");
            return 2;
        }
        
        args.Logger?.ILog("Time difference is within the allowed difference");
        return 1;
    }
}