namespace FileFlows.VideoNodes;

/// <summary>
/// Flow element to test the file size of a video based on its duration (size per hour)
/// </summary>
public class SizePerHour : VideoNode
{
    /// <inheritdoc />
    public override int Inputs => 1;
    
    /// <inheritdoc />
    public override int Outputs => 2;
    
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/size-per-hour";
    
    /// <inheritdoc />
    public override string Icon => "fas fa-hourglass-half";
    
    /// <summary>
    /// Gets or sets the megabytes allowed per hour.
    /// </summary>
    [NumberInt(1)]
    public int MegabytesPerHour { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var videoInfo = GetVideoInfo(args);
        if (videoInfo == null || videoInfo.VideoStreams?.Any() != true)
        {
            args.FailureReason = "Failed to retrieve video info";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        TimeSpan duration = videoInfo.VideoStreams.Max(x => x.Duration);
        args.Logger?.ILog("Duration: " + duration);

        var sizeInBytes = args.WorkingFileSize;
        double sizeInMB = sizeInBytes / 1_000_000d; // Convert bytes to megabytes

        // Calculate the allowed size for the duration in hours
        double maxAllowedSize = MegabytesPerHour * duration.TotalHours;
        
        // Round the file size and max allowed size
        double roundedSizeInMB = Math.Round(sizeInMB, sizeInMB % 1 == 0 ? 0 : 1);
        double roundedMaxAllowedSize = Math.Round(maxAllowedSize, maxAllowedSize % 1 == 0 ? 0 : 1);

        // Compare size to the allowed size
        if (sizeInMB <= maxAllowedSize)
        {
            args.Logger?.ILog($"File size is within the allowed limit: {roundedSizeInMB}MB <= {roundedMaxAllowedSize}MB");
            return 1; // Passes the check
        }
        
        args.Logger?.ILog($"File size exceeds the allowed limit: {roundedSizeInMB}MB > {roundedMaxAllowedSize}MB");
        return 2; // Fails the check
            
    }
}
