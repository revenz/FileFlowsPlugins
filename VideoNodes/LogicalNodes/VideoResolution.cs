namespace FileFlows.VideoNodes;

/// <summary>
/// Tests a video resolution
/// </summary>
public class VideoResolution: VideoNode
{
    /// <summary>
    /// Gets the number of inputs
    /// </summary>
    public override int Inputs => 1;
    /// <summary>
    /// Gets the number of outputs
    /// </summary>
    public override int Outputs => 4;
    /// <summary>
    /// Gets the type of flow element
    /// </summary>
    public override FlowElementType Type => FlowElementType.Logic;
    /// <summary>
    /// Gets the help URL 
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/video-resolution";
    /// <summary>
    /// Gets the icon for this flow element
    /// </summary>
    public override string Icon => "fas fa-video";

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the arguments</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        VideoInfo videoInfo = GetVideoInfo(args);
        if (videoInfo == null)
        {
            args.Logger?.ELog($"No video info found, run the Video File flow element first.");
            return -1;
        }

        // get the first video stream, likely the only one
        var video = videoInfo.VideoStreams.FirstOrDefault();
        if (video == null)
        {
            args.Logger?.ELog($"No video stream detected");
            return -1; // no video streams detected
        }

        if (video.Width > 3700 || video.Height >= 2060)
        {
            args.Logger?.ILog($"4k Video Detected: {video.Width}x{video.Height}");
            return 1; // 4k 
        }

        if (video.Width > 1800 || video.Height >= 1000)
        {
            args.Logger?.ILog($"1080p Video Detected: {video.Width}x{video.Height}");
            return 2; // 1080p
        }

        if (video.Width > 1200 || video.Height >= 680)
        {
            args.Logger?.ILog($"720p Video Detected: {video.Width}x{video.Height}");
            return 3; // 720p
        }

        args.Logger?.ILog($"SD Video Detected: {video.Width}x{video.Height}");
        return 4; // SD
    }
}