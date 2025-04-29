namespace FileFlows.VideoNodes;

/// <summary>
/// Flow element to test if a video is HDR
/// </summary>
public class VideoIsHdr : VideoNode
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
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/video-is-hdr";

    
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

        var streamFound = videoInfo.VideoStreams?.Any(x => x.HDR) == true;
        if (streamFound == false)
        {
            args.Logger?.ILog("HDR was not detected.");
            return 2;
        }
        
        args.Logger?.ILog("HDR was detected.");
        return 1;
    }

}
