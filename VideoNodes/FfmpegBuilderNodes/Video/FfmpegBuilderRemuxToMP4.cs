namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Remuxes the file to MP4
/// </summary>
public class FfmpegBuilderRemuxToMP4: FfmpegBuilderNode
{
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/remux-to-mp4";

    /// <summary>
    /// Gets if the editor should be shown on add by default
    /// </summary>
    public override bool NoEditorOnAdd => true;
    /// <inheritdoc />
    public override string Icon => "svg:mp4";

    /// <summary>
    /// Gets or sets if hvc1 tag should be added
    /// </summary>
    [Boolean(1)] public bool UseHvc1 { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        if (this.Model.Extension?.ToLowerInvariant() != "mp4")
        {
            args.Logger?.ILog($"Needs remuxing from '{this.Model.Extension}' to 'mp4', forcing encode");
            this.Model.ForceEncode = true;
        }
        
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
