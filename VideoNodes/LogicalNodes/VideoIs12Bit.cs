using FileFlows.VideoNodes.FfmpegBuilderNodes;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using FileFlows.VideoNodes.Helpers;

namespace FileFlows.VideoNodes;

/// <summary>
/// Flow element to test if a video is 12 bit or not
/// </summary>
public class VideoIs12Bit : VideoNode
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
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/video-is-12-bit";

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

        bool is12Bit = videoInfo.VideoStreams?.Any(x => x.Bits == 12) == true;
        if (is12Bit)
        {
            args.Logger?.ILog("Video is 12 bit");
            return 1;
        }

        args.Logger?.ILog("Video is not 12 bit");
        return 2;
    }
}
