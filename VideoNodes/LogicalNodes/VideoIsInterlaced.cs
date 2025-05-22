using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace FileFlows.VideoNodes;

/// <summary>
/// Flow element to test if a video is interlaced
/// </summary>
public class VideoIsInterlaced : VideoNode
{
    /// <summary>
    /// Gets the number of inputs
    /// </summary>
    public override int Inputs => 1;
    /// <summary>
    /// Gets the number of outputs
    /// </summary>
    public override int Outputs => 2;
    /// <summary>
    /// Gets the type of flow element
    /// </summary>
    public override FlowElementType Type => FlowElementType.Logic;
    /// <summary>
    /// Gets the icon
    /// </summary>
    public override string Icon => "fas fa-question";
    /// <summary>
    /// Gets the help URL 
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/video-is-interlaced";

    /// <summary>
    /// Gets or sets the threshold
    /// </summary>
    [Range(1, 100)]
    [Slider(1)]
    [DefaultValue(10)]
    public int Threshold { get; set; }
    
    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the arguments</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        var videoInfo = GetVideoInfo(args);
        if (videoInfo != null)
        {
            bool interlaced = videoInfo.VideoStreams?.Any(x => x.IsInterlaced) == true;
            if (interlaced)
            {
                args.Logger?.ILog("Video detected as interlaced from VideoInfo");
                return 1;
            }
            
            args.Logger?.ILog("Video detected as not interlaced from VideoInfo");
            return 2;
        }


        string ffmpeg = args.GetToolPath("FFmpeg");
        if (string.IsNullOrEmpty(ffmpeg))
        {
            args.Logger?.ELog("FFmpeg tool not found.");
            return 2;
        }

        var localFile = args.FileService.GetLocalPath(args.WorkingFile);
        if (localFile.Failed(out string error))
        {
            args.FailureReason = "Failed to get local file: " + error;
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        var ffOutput = args.Execute(new()
        {
            Command = ffmpeg,
            Silent = true,
            ArgumentList = new[]
            {
                "-hide_banner",
                "-i", localFile.Value,
                "-vf", "idet",
                "-f", "null", "-"
            }
        });

        if (string.IsNullOrEmpty(ffOutput.StandardOutput))
        {
            args.Logger?.ELog("Failed to get standard output from FFmpeg");
            return 2;
        }

        bool isInterlaced = IsVideoInterlaced(args.Logger, ffOutput.StandardOutput, Math.Max(Threshold, 1));
        args.Logger?.ILog("Is interlaced: " + isInterlaced);
        return isInterlaced ? 1 : 2;
    }

    /// <summary>
    /// Determines if a video is interlaced based on FFmpeg output.
    /// </summary>
    /// <param name="ffmpegOutput">The FFmpeg output as a string.</param>
    /// <param name="interlaceThreshold">An optional threshold for the number of interlaced frames to consider.</param>
    /// <returns>True if the video is interlaced, false if it's not.</returns>
    public static bool IsVideoInterlaced(ILogger logger, string ffmpegOutput, int interlaceThreshold = 10)
    {
        // Define regular expressions to match interlaced and progressive frame counts
        Regex tffRegex = new Regex(@"TFF:[\s]*(\d+)");
        Regex bffRegex = new Regex(@"BFF:[\s]*(\d+)");
        Regex progressiveRegex = new Regex(@"Progressive:[\s]*(\d+)");

        int tffCount = 0;
        int bffCount = 0;
        int progressiveCount = 0;

        // Use regular expressions to extract counts of interlaced and progressive frames
        MatchCollection tffMatches = tffRegex.Matches(ffmpegOutput);
        MatchCollection bffMatches = bffRegex.Matches(ffmpegOutput);
        MatchCollection progressiveMatches = progressiveRegex.Matches(ffmpegOutput);

        foreach (Match match in tffMatches)
        {
            tffCount += int.Parse(match.Groups[1].Value);
        }

        foreach (Match match in bffMatches)
        {
            bffCount += int.Parse(match.Groups[1].Value);
        }

        foreach (Match match in progressiveMatches)
        {
            progressiveCount += int.Parse(match.Groups[1].Value);
        }

        // Calculate the total count of interlaced frames
        int totalInterlacedCount = tffCount + bffCount;

        int totalFrames = totalInterlacedCount + progressiveCount;
        float percent = ((float)totalInterlacedCount) / totalFrames * 100f;
        
        logger?.ILog("Interlaced threshold percentage: " + interlaceThreshold);
        logger?.ILog("Total Interlaced Frames: " + totalInterlacedCount);
        logger?.ILog("Total Progressive Frames: " + progressiveCount);
        logger?.ILog("Total Frames: " + totalFrames);
        logger?.ILog("Interlaced Percent: " + percent);

        // Check if the total count of interlaced frames exceeds the threshold
        return percent >= interlaceThreshold;
    }
}
