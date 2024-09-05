namespace FileFlows.VideoNodes;

/// <summary>
/// Flow element to test if a video file has already been processed, this is done by looking for the FileFlows comment
/// </summary>
public class VideoAlreadyProcessed : VideoNode
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
    /// Gets the help URL 
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/video-already-processed";
    /// <inheritdoc />
    public override string Icon => "fas fa-running";

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the arguments</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        var videoInfo = GetVideoInfo(args);
        if (videoInfo == null)
        {
            args.FailureReason = "Failed to retrieve video info";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        bool alreadyProcessed = videoInfo.AlreadyProcessed;
        if (alreadyProcessed)
        {
            args.Logger?.ILog("Video has already been processed by FileFlows");
            return 1;
        }

        args.Logger?.ILog("Video has not been processed by FileFlows");
        return 2;
    }
}
