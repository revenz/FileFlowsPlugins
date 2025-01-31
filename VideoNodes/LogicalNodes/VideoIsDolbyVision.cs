namespace FileFlows.VideoNodes;

/// <summary>
/// Flow element to test if a video is dolby vision
/// </summary>
public class VideoIsDolbyVision : VideoNode
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string Icon => "fas fa-question";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/video-is-dolby-vision";

    
    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the arguments</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        var videoInfo = GetVideoInfo(args);
        if (videoInfo == null)
            return args.Fail("Failed to retrieve video info");

        var dolbyVision = videoInfo.VideoStreams?.Any(x => x.DolbyVision) == true;
        if (!dolbyVision)
        {
            args.Logger?.ILog("Dolby Vision was not detected.");
            return 2;
        }
        
        args.Logger?.ILog("Dolby Vision was detected.");
        return 1;
    }

}
