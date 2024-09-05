namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Flow element to set the video as 10-bit
/// </summary>
public class FfmpegBuilderVideo10Bit : FfmpegBuilderNode
{
    /// <summary>
    /// Gets the number of outputs
    /// </summary>
    public override int Outputs => 1;

    /// <summary>
    /// Gets the URL to the help page
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/video-10-bit";

    /// <summary>
    /// Gets that this is obsolete and should no longer be used
    /// </summary>
    public override bool Obsolete => true;
    /// <summary>
    /// Gets the obsolete message
    /// </summary>
    public override string ObsoleteMessage => "Specify 10-bit in the Video Encode or Video Codec flow element.  This can flow element can cause failures with QSV.";

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        args?.Logger?.WLog("This flow element has been marked obsolete and should no longer be used.");
        return 1; // do nothing
        
        // var videoInfo = GetVideoInfo(args);
        // if (videoInfo == null || videoInfo.VideoStreams?.Any() != true)
        //     return -1;
        //
        // var stream = Model?.VideoStreams?.Where(x => x.Deleted == false)?.FirstOrDefault();
        // if (stream != null)
        // {
        //     args.Logger?.ILog(
        //         "Adding optional encoding parameters: -pix_fmt:v:{index} p010le -profile:v:{index} main10");
        //     
        //     stream.OptionalEncodingParameters.AddRange(new[]
        //         { "-pix_fmt:v:{index}", "p010le", "-profile:v:{index}", "main10" });
        // }
        //
        // return 1;
    }
}
