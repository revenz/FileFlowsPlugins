namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Node that starts the FFMPEG Builder
/// </summary>
public class FfmpegBuilderStart: FfmpegBuilderNode
{

    public override string HelpUrl => "https://docs.fileflows.com/plugins/video-nodes/ffmpeg-builder";

    /// <summary>
    /// The number of inputs into this node
    /// </summary>
    public override int Inputs => 1;
    
    /// <summary>
    /// The number of outputs from this node
    /// </summary>
    public override int Outputs => 1;
    
    /// <summary>
    /// The icon for this node
    /// </summary>
    public override string Icon => "far fa-file-video";
    
    
    /// <summary>
    /// The type of this node
    /// </summary>
    public override FlowElementType Type => FlowElementType.BuildStart;


    /// <summary>
    /// Executes the node
    /// </summary>
    /// <param name="args">The node arguments</param>
    /// <returns>the output return</returns>
    public override int Execute(NodeParameters args)
    {
        VideoInfo videoInfo = GetVideoInfo(args);
        if (videoInfo == null)
            return -1;
        this.Model = Models.FfmpegModel.CreateModel(videoInfo);

        args.Logger.ILog("FFMPEG Builder File: " + videoInfo.FileName);
        foreach (var track in videoInfo.VideoStreams)
            args.Logger.ILog($"Video Track '{track.Codec}' ({track.Width}x{track.Height}");
        foreach (var track in videoInfo.AudioStreams)
            args.Logger.ILog("Audio Track: " + track.Codec);
        foreach (var track in videoInfo.SubtitleStreams)
            args.Logger.ILog("Subtitle Track: " + track.Codec);

        return 1;
    }
}