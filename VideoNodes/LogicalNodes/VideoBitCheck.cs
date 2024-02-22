using FileFlows.VideoNodes.FfmpegBuilderNodes;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using FileFlows.VideoNodes.Helpers;

namespace FileFlows.VideoNodes;

/// <summary>
/// Flow element to test if a video is 8/10/12/Unknown bit or not
/// </summary>
public class VideoBitCheck : VideoNode
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
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/video-bit-check";

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

        bool is8Bit = videoInfo.VideoStreams?.Any(x => x.Bits == 8) == true;
        if (is8Bit)
        {
            args.Logger?.ILog("Video is 12 bit");
            return 1;
        }
        bool is10Bit = videoInfo.VideoStreams?.Any(x => x.Bits == 10) == true;
        if (is10Bit)
        {
            args.Logger?.ILog("Video is 10 bit");
            return 2;
        }
        bool is12Bit = videoInfo.VideoStreams?.Any(x => x.Bits == 12) == true;
        if (is12Bit)
        {
            args.Logger?.ILog("Video is 12 bit");
            return 3;
        }
        args.Logger?.ILog("Video Bits unknonw");
        return 4;
    }
}
