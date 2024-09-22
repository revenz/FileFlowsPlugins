namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// A node that forces the aspect ratio of a video using FFmpeg. 
/// Provides options to stretch, add black bars (letterbox), crop, or use custom dimensions.
/// </summary>
public class FfmpegBuilderAspectRatio : FfmpegBuilderNode
{
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/aspect-ratio";
    /// <inheritdoc />
    public override string Icon => "far fa-percent";

    /// <summary>
    /// The desired aspect ratio for the video. Supports standard options like 16:9, 4:3, 21:9, or custom dimensions.
    /// </summary>
    [Select(nameof(AspectRatioOptions), 1)]
    public string AspectRatio { get; set; }

    /// <summary>
    /// The mode of adjustment to enforce the aspect ratio. Options include stretching, adding black bars, cropping, or using custom dimensions.
    /// </summary>
    [Select(nameof(AdjustmentModeOptions), 2)]
    public string AdjustmentMode { get; set; } = "Stretch";

    /// <summary>
    /// Custom width when using the custom aspect ratio option. 
    /// Used if <see cref="AspectRatio"/> is set to "Custom".
    /// </summary>
    [Range(1, int.MaxValue)]
    [ConditionEquals(nameof(AspectRatio), "Custom")]
    public int CustomWidth { get; set; }

    /// <summary>
    /// Custom height when using the custom aspect ratio option. 
    /// Used if <see cref="AspectRatio"/> is set to "Custom".
    /// </summary>
    [Range(1, int.MaxValue)]
    [ConditionEquals(nameof(AspectRatio), "Custom")]
    public int CustomHeight { get; set; }

    /// <summary>
    /// A list of standard aspect ratio options (16:9, 4:3, 21:9, or Custom).
    /// </summary>
    private static List<ListOption> _AspectRatioOptions;
    public static List<ListOption> AspectRatioOptions
    {
        get
        {
            if (_AspectRatioOptions == null)
            {
                _AspectRatioOptions = new List<ListOption>
                {
                    new() { Value = "16:9", Label = "16:9" },
                    new() { Value = "4:3", Label = "4:3" },
                    new() { Value = "21:9", Label = "21:9" },
                    new() { Value = "Custom", Label = "Custom" }
                };
            }
            return _AspectRatioOptions;
        }
    }

    /// <summary>
    /// A list of adjustment modes: Stretch, Add Black Bars (Letterbox), Crop, or Custom.
    /// </summary>
    private static List<ListOption> _AdjustmentModeOptions;
    public static List<ListOption> AdjustmentModeOptions
    {
        get
        {
            if (_AdjustmentModeOptions == null)
            {
                _AdjustmentModeOptions = new List<ListOption>
                {
                    new() { Value = "Stretch", Label = "Stretch to Fit" },
                    new() { Value = "AddBlackBars", Label = "Add Black Bars (Letterbox)" },
                    new() { Value = "Crop", Label = "Crop to Fit" },
                };
            }
            return _AdjustmentModeOptions;
        }
    }

    /// <summary>
    /// Executes the FFmpeg aspect ratio adjustment based on the selected aspect ratio and adjustment mode.
    /// </summary>
    /// <param name="args">The node parameters passed to the FFmpeg builder.</param>
    /// <returns>
    /// 1 if the aspect ratio was changed successfully, 
    /// 2 if the video was already in the desired aspect ratio and no change was needed,
    /// -1 if there was an error.
    /// </returns>
    public override int Execute(NodeParameters args)
    {
        var videoInfo = GetVideoInfo(args);
        if (videoInfo == null || videoInfo.VideoStreams?.Any() != true)
        {
            args.FailureReason = "No valid video stream found.";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        int videoWidth = videoInfo.VideoStreams[0].Width;
        int videoHeight = videoInfo.VideoStreams[0].Height;
        double currentAspectRatio = (double)videoWidth / videoHeight;

        args.Logger?.ILog($"Current video dimensions: {videoWidth}x{videoHeight}, Aspect Ratio: {currentAspectRatio:F2}");

        // Check for custom aspect ratio
        double targetAspectRatio;
        if (AspectRatio == "Custom")
        {
            targetAspectRatio = (double)CustomWidth / CustomHeight;
            args.Logger?.ILog($"Target custom aspect ratio: {CustomWidth}:{CustomHeight} = {targetAspectRatio:F2}");

            if (IsAlreadyDesiredAspectRatio(currentAspectRatio, targetAspectRatio))
            {
                args.Logger?.ILog("The video is already in the desired custom aspect ratio.");
                return 2;
            }
        }
        else
        {
            string[] aspectParts = AspectRatio.Split(':');
            targetAspectRatio = (double)int.Parse(aspectParts[0]) / int.Parse(aspectParts[1]);
            args.Logger?.ILog($"Target aspect ratio: {AspectRatio} = {targetAspectRatio:F2}");

            // Check if the video is already in the desired aspect ratio
            if (IsAlreadyDesiredAspectRatio(currentAspectRatio, targetAspectRatio))
            {
                args.Logger?.ILog("The video is already in the desired aspect ratio.");
                return 2;
            }
        }

        // Generate the filter based on adjustment mode
        string filter = AdjustmentMode switch
        {
            "Stretch" => $"scale={videoWidth}:{(int)(videoWidth / targetAspectRatio)}:flags=lanczos",
            "AddBlackBars" => AddBlackBars(videoWidth, videoHeight, targetAspectRatio),
            "Crop" => CropToFit(videoWidth, videoHeight, targetAspectRatio),
            _ => null
        };

        if (filter == null)
        {
            args.FailureReason = "Failed to generate a filter based on the adjustment mode.";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        args.Logger?.ILog($"Applying filter: {filter}");
        Model.VideoStreams[0].Filter.AddRange(new[] { filter });
        return 1;
    }

    /// <summary>
    /// Determines if the video is already in the desired aspect ratio within a small tolerance.
    /// </summary>
    /// <param name="currentAspectRatio">The current aspect ratio of the video.</param>
    /// <param name="targetAspectRatio">The desired target aspect ratio.</param>
    /// <returns>True if the video is already within the target aspect ratio tolerance.</returns>
    private bool IsAlreadyDesiredAspectRatio(double currentAspectRatio, double targetAspectRatio)
    {
        const double tolerance = 0.01; // 1% tolerance for aspect ratio matching
        bool result = Math.Abs(currentAspectRatio - targetAspectRatio) < tolerance;
        Args?.Logger?.ILog($"Checking aspect ratio: current={currentAspectRatio:F2}, target={targetAspectRatio:F2}, result={result}");
        return result;
    }

    /// <summary>
    /// Adds black bars to the video (letterbox) to fit the target aspect ratio.
    /// </summary>
    /// <param name="width">The width of the original video.</param>
    /// <param name="height">The height of the original video.</param>
    /// <param name="targetAspectRatio">The desired aspect ratio.</param>
    /// <returns>A string representing the FFmpeg filter to add black bars.</returns>
    private string AddBlackBars(int width, int height, double targetAspectRatio)
    {
        double currentAspectRatio = (double)width / height;
        Args?.Logger?.ILog($"Adding black bars: width={width}, height={height}, targetAspectRatio={targetAspectRatio:F2}");

        if (currentAspectRatio > targetAspectRatio)
        {
            // Add vertical black bars
            int paddedHeight = (int)(width / targetAspectRatio);
            Args?.Logger?.ILog($"Adding vertical black bars, padded height={paddedHeight}");
            return $"scale={width}:-2, pad={width}:{paddedHeight}:(ow-iw)/2:(oh-ih)/2";
        }
        else
        {
            // Add horizontal black bars
            int paddedWidth = (int)(height * targetAspectRatio);
            Args?.Logger?.ILog($"Adding horizontal black bars, padded width={paddedWidth}");
            return $"scale=-2:{height}, pad={paddedWidth}:{height}:(ow-iw)/2:(oh-ih)/2";
        }
    }

    /// <summary>
    /// Crops the video to fit the desired aspect ratio, discarding part of the frame.
    /// </summary>
    /// <param name="width">The width of the original video.</param>
    /// <param name="height">The height of the original video.</param>
    /// <param name="targetAspectRatio">The desired aspect ratio.</param>
    /// <returns>A string representing the FFmpeg filter to crop the video.</returns>
    private string CropToFit(int width, int height, double targetAspectRatio)
    {
        double currentAspectRatio = (double)width / height;
        Args?.Logger?.ILog($"Cropping video: width={width}, height={height}, targetAspectRatio={targetAspectRatio:F2}");

        if (currentAspectRatio > targetAspectRatio)
        {
            // Crop width
            int croppedWidth = (int)(height * targetAspectRatio);
            Args?.Logger?.ILog($"Cropping width, new width={croppedWidth}");
            return $"crop={croppedWidth}:{height}";
        }
        else
        {
            // Crop height
            int croppedHeight = (int)(width / targetAspectRatio);
            Args?.Logger?.ILog($"Cropping height, new height={croppedHeight}");
            return $"crop={width}:{croppedHeight}";
        }
    }
}
