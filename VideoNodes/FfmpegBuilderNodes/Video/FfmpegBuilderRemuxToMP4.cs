namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Remuxes the file to MP4
/// </summary>
public class FfmpegBuilderRemuxToMP4: FfmpegBuilderNode
{
    /// <summary>
    /// Gets the help URL for this flow element
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/remux-to-mp4";

    /// <summary>
    /// Gets if the editor should be shown on add by default
    /// </summary>
    public override bool NoEditorOnAdd => false;

    /// <summary>
    /// Gets or sets if hvc1 tag should be added
    /// </summary>
    [Boolean(1)] public bool UseHvc1 { get; set; }

    public override int Execute(NodeParameters args)
    {
        this.Model.Extension = "mp4";
        if (UseHvc1)
        {
            var stream = this.Model.VideoStreams.FirstOrDefault(x => x.Deleted == false);
            if(stream != null)
                stream.AdditionalParameters.AddRange(new [] { "-tag:v", "hvc1" });
        }
        return 1;
    }
}
