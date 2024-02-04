namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// FFmpeg Builder flow element that sets the FPS of the resulting video
/// </summary>
public class FfmpegBuilderSetFps:FfmpegBuilderNode
{
    /// <summary>
    /// The number of outputs for this flow element
    /// </summary>
    public override int Outputs => 2;
    /// <summary>
    /// The Help URL for this flow element
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/set-fps";
    /// <summary>
    /// Gets the icon to show for this flow element
    /// </summary>
    public override string Icon => "fas fa-stopwatch";

    /// <summary>
    /// Gets or sets the FPS to use
    /// </summary>
    [DefaultValue(30f)]
    [NumberFloat(1)]
    public float Fps { get; set; }

    /// <summary>
    /// Gets or sets if the FPS should be adjusted only if the current FPS is over the current 
    /// </summary>
    [Boolean(2)]
    public bool OnlyIfHigher { get; set; }

    
    /// <summary>
    /// Executes the node logic based on the provided parameters.
    /// </summary>
    /// <param name="args">Node parameters.</param>
    /// <returns>An integer representing the execution result.</returns>
    public override int Execute(NodeParameters args)
    {
        double desiredFps = Fps;

        VideoInfo videoInfo = GetVideoInfo(args);
        if (videoInfo == null || !videoInfo.VideoStreams?.Any() == true)
        {
            args.Logger?.ELog("No video streams found.");
            return -1;
        }

        int currentFps = (int)Math.Ceiling(videoInfo.VideoStreams[0].FramesPerSecond);
        currentFps = FixHighFrameRateBug(currentFps);

        var ffmpegModel = GetModel();
        if (ffmpegModel == null)
        {
            args.Logger?.ELog("FFMPEG Builder variable not found");
            return -1;
        }

        // check for video stream
        var videoStream = ffmpegModel.VideoStreams[0];
        if (videoStream == null)
        {
            args.Logger?.ELog("FFMPEG Builder no video stream found");
            return -1;
        }

        if (Math.Abs(currentFps - desiredFps) < 0.05f)
        {
            args.Logger?.ILog("The frame rate matches, so does not need changing");
            return 2;
        }

        if (currentFps > desiredFps)
        {
            args.Logger?.ILog($"The frame rate {currentFps}fps is higher than the desired {desiredFps}fps, so will be changed");
            videoStream.Filter.Add($"fps=fps={desiredFps}");
            return 1;
        }

        if (currentFps < desiredFps)
        {
            if (OnlyIfHigher)
            {
                args.Logger?.ILog($"The frame rate {currentFps}fps is lower than the desired {desiredFps}fps, and (Only If Higher) was selected, so no change needed");
                return 2;
            }

            args.Logger?.ILog($"The frame rate {currentFps}fps is lower than the desired {desiredFps}fps, so will be changed");
            videoStream.Filter.Add($"fps=fps={desiredFps}");
            return 1;
        }

        args.Logger?.ILog("The frame rate is unknown");
        return 2;
    }
    
    /// <summary>
    /// Fixes a bug related to high frame rates by adjusting the current frame rate.
    /// </summary>
    /// <param name="currentFps">Current frame rate.</param>
    /// <returns>Adjusted frame rate.</returns>
    private int FixHighFrameRateBug(int currentFps)
    {
        if (currentFps > 200)
        {
            // Adjust the current frame rate for high frame rate bug
            return (int)(currentFps / 100f);
        }

        return currentFps;
    }
}
