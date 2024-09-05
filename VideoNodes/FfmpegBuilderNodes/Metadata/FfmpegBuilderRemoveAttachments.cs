namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderRemoveAttachments : FfmpegBuilderNode
{
    /// <summary>
    /// Gets the Help Url
    /// </summary>
    public override string HelpUrl =>
        "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/remove-attachments";

    /// <summary>
    /// Gets the number of outputs
    /// </summary>
    public override int Outputs => 1;
    /// <summary>
    /// Gets the icon
    /// </summary>
    public override string Icon => "fas fa-paperclip";

    public override int Execute(NodeParameters args)
    {
        Model.RemoveAttachments = true;
        return 1;
    }

}