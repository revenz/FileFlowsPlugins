using System.IO;

namespace FileFlows.VideoNodes;

/// <summary>
/// Node that creates a video thumbnail with resizing options and aspect ratio considerations.
/// </summary>
public class CreateThumbnail : VideoNode
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/create-thumbnail";
    /// <inheritdoc />
    public override string Icon => "fas fa-photo-video";
    
    /// <summary>
    /// Gets or sets the destination path for zipping.
    /// </summary>
    [TextVariable(1)]
    [Required]
    public string Destination { get; set; } = string.Empty;
    
    /// <summary>
    /// The width of the thumbnail.
    /// </summary>
    [NumberInt(2)]
    [DefaultValue(200)]
    public int Width { get; set; }

    /// <summary>
    /// The height of the thumbnail.
    /// </summary>
    [NumberInt(3)]
    [DefaultValue(200)]
    public int Height { get; set; }

    /// <summary>
    /// The time in the video to capture the thumbnail (in seconds or percentage of the video duration).
    /// </summary>
    [Time(4)]
    public TimeSpan Time { get; set; }
    

    /// <summary>
    /// The image resize mode to use.
    /// </summary>
    [Select(nameof(ResizeModeOptions), 5)]
    [DefaultValue(ResizeMode.Contain)]
    public ResizeMode ResizeMode { get; set; } = ResizeMode.Contain;

    /// <summary>
    /// Indicates whether to detect black frames or credits and skip them.
    /// </summary>
    [Boolean(6)]
    public bool SkipBlackFrames { get; set; } = true;

    private static List<ListOption> _ResizeModeOptions;
    public static List<ListOption> ResizeModeOptions
    {
        get
        {
            if (_ResizeModeOptions == null)
            {
                _ResizeModeOptions = new List<ListOption>
                {
                    new () { Label = "Fill", Value = ResizeMode.Fill },
                    new () { Label = "Contain", Value = ResizeMode.Contain },
                    new () { Label = "Cover", Value = ResizeMode.Cover },
                    new () { Label = "Pad", Value = ResizeMode.Pad },
                    new () { Label = "Min", Value = ResizeMode.Min },
                    new () { Label = "Max", Value = ResizeMode.Max }
                };
            }
            return _ResizeModeOptions;
        }
    }

    /// <summary>
    /// Executes the node, creating a thumbnail for the video.
    /// </summary>
    /// <param name="args">The node parameters.</param>
    /// <returns>The result code: 1 for success, 2 for failure.</returns>
    public override int Execute(NodeParameters args)
    {
        try
        {
            VideoInfo videoInfo = GetVideoInfo(args);
            if (videoInfo == null)
            {
                args.FailureReason = "No Video Information found.";
                args.Logger?.ELog(args.FailureReason);
                return -1;
            }
            var lfResult = args.FileService.GetLocalPath(args.WorkingFile);
            if (lfResult.Failed(out var error))
            {
                args.FailureReason = "Failed to get local file: " + error;
                args.Logger?.ELog(args.FailureReason);
                return -1;
            }
            string localFile = lfResult.Value;

            // Ensure time is within bounds
            TimeSpan captureTime = GetValidCaptureTime(videoInfo.VideoStreams[0].Duration);

            // Generate a thumbnail
            string thumbnailPath = Path.Combine(args.TempPath, Guid.NewGuid() + ".png");
            if (CaptureThumbnail(args, localFile, captureTime, thumbnailPath) == false)
            {
                args.Logger?.WLog("Failed to generate a thumbnail");
                return 2;
            }

            // Check for black frames or credits and skip if necessary
            if (SkipBlackFrames && IsBlackOrCredits(thumbnailPath, args))
            {
                captureTime = AdjustCaptureTime(captureTime, videoInfo.VideoStreams[0].Duration);
                if (CaptureThumbnail(args, localFile, captureTime, thumbnailPath) == false)
                {
                    args.Logger?.WLog("Failed to generate a thumbnail 2");
                    return 2;
                }
            }

            // Resize the thumbnail
            string resizedThumbnailPath = Path.Combine(args.TempPath, Guid.NewGuid() + ".png");
            var resizeResult = args.ImageHelper.Resize(thumbnailPath, resizedThumbnailPath, Width, Height, ResizeMode);
            if (resizeResult.Failed(out error))
            {
                args.Logger?.ELog("Failed to resize thumbnail: " + error);
                return 2;
            }


            string dest = args.ReplaceVariables(Destination, stripMissing: true);
            if (string.IsNullOrWhiteSpace(dest))
            {
                dest = resizedThumbnailPath;
            }
            else
            {
                if (args.FileService.FileMove(resizedThumbnailPath, dest).Failed(out error))
                {
                    args.Logger?.WLog("Failed to move file: " + error);
                    return 2;
                }
            }
            args.Logger?.ILog("Thumbnail Path: " + dest);
            // Set output variable
            args.UpdateVariables(new Dictionary<string, object> { { "ThumbnailPath", dest } });
            return 1;
        }
        catch (Exception ex)
        {
            args.FailureReason = "Failed to create thumbnail: " + ex.Message;
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }
    }

    /// <summary>
    /// Gets a valid capture time ensuring it's within the bounds of the video duration.
    /// </summary>
    /// <param name="duration">The video duration.</param>
    /// <returns>The valid capture time as a TimeSpan.</returns>
    private TimeSpan GetValidCaptureTime(TimeSpan duration)
    {
        // If Time is set to a value less than 1 second, treat it as a percentage (e.g., 0.1 means 10% of the video)
        if (Time.TotalSeconds < 1)
            return TimeSpan.FromTicks((long)(duration.Ticks * Time.TotalSeconds));

        // Otherwise, treat it as an absolute value in seconds
        return Time < duration ? Time : duration; // Cap Time to the video duration
    }

    /// <summary>
    /// Captures a thumbnail using ffmpeg.
    /// </summary>
    /// <param name="args">The node parameters.</param>
    /// <param name="localFile">The local file path of the video.</param>
    /// <param name="time">The time to capture the thumbnail in the video.</param>
    /// <param name="outputPath">The output file path for the thumbnail.</param>
    /// <returns>True if successful, otherwise false.</returns>
    private bool CaptureThumbnail(NodeParameters args, string localFile, TimeSpan time, string outputPath)
    {
        var result = args.Process.ExecuteShellCommand(new ExecuteArgs
        {
            Command = FFMPEG,
            ArgumentList = [
                "-ss", ((int)time.TotalSeconds).ToString(), // seek to the time
                "-i", localFile,                            // input file
                "-frames:v", "1",                           // capture just one frame
                "-update", "1",                             // allow overwriting the file
                outputPath                                  // output single image, no sequence pattern
            ]
        }).Result;

        if (result.ExitCode != 0 || File.Exists(outputPath) == false)
        {
            args.Logger?.ELog("FFmpeg failed to capture thumbnail.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if the captured thumbnail is mostly black or contains credits.
    /// </summary>
    /// <param name="thumbnailPath">The path to the thumbnail image.</param>
    /// <param name="args">The node parameters.</param>
    /// <returns>True if the image is mostly black or contains credits, otherwise false.</returns>
    private bool IsBlackOrCredits(string thumbnailPath, NodeParameters args)
    {
        // Example logic for checking if an image is mostly black or very small (e.g., credits)
        try
        {
            var result = args.ImageHelper.CalculateImageDarkness(thumbnailPath);
            if (result.Failed(out var error))
            {
                args.Logger?.WLog("Falied to calculate darkness: " + error);
                return false;
            }

            args.Logger?.ILog($"Darkness level detected: {result.Value}");
            return result.Value < 20;
        }
        catch (Exception ex)
        {
            args.Logger?.WLog($"Error analyzing thumbnail {thumbnailPath}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Adjusts the capture time if the thumbnail is black or contains credits.
    /// </summary>
    /// <param name="currentTime">The current time of the thumbnail.</param>
    /// <param name="duration">The total duration of the video.</param>
    /// <returns>The adjusted capture time.</returns>
    private TimeSpan AdjustCaptureTime(TimeSpan currentTime, TimeSpan duration)
    {
        // Move the capture time by 10% of the video length forwards or backwards
        TimeSpan shift = TimeSpan.FromTicks((long)(duration.Ticks * 0.1));
        if (currentTime + shift < duration)
            return currentTime + shift;

        return currentTime > shift ? currentTime - shift : TimeSpan.Zero; // Shift backwards if near the end
    }
}
