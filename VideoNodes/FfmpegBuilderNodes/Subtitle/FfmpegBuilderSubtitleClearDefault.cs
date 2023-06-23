namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// A node that will clear the default tag from subtitles
/// </summary>
public class FfmpegBuilderSubtitleClearDefault : FfmpegBuilderNode
{
    /// <summary>
    /// Gets the help URL
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/subtitle-clear-default";
    /// <summary>
    /// Gets the icon
    /// </summary>
    public override string Icon => "fas fa-comment-slash";
    /// <summary>
    /// Gets the number of outputs
    /// </summary>
    public override int Outputs => 2;

    /// <summary>
    /// Gets or sets if forced should be left alone
    /// </summary>
    [Boolean(1)]
    public bool LeaveForced { get; set; }
    
    /// <summary>
    /// Gets or sets if forced should be changed to be the default
    /// </summary>
    [Boolean(2)]
    public bool SetForcedDefault { get; set; }

    /// <summary>
    /// Executes the node
    /// </summary>
    /// <param name="args">the node arguments</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        
        if (Model.SubtitleStreams.Any() == false)
            return 2;
        bool changed = false;
        bool defaultSet = false;
        foreach (var stream in Model.SubtitleStreams)
        {
            if (SetForcedDefault && stream.IsDefault == false && stream.IsForced && defaultSet == false)
            {
                // check this first since this is a forced subtitle that is not set to default and we want it to be
                args.Logger?.ILog($"Setting forced subtitle[{stream.Index}]  as default: {(stream.Title?.EmptyAsNull() ?? stream.Language?.EmptyAsNull() ?? stream.Stream.Codec)}");
                stream.IsDefault = true;
                defaultSet = true;
                stream.ForcedChange = true;
                continue;
            }
            
            if (stream.IsDefault == false)
                continue; // not set to default, no change needed
            
            if (LeaveForced && stream.Stream.Forced)
            {
                defaultSet = true; // this stream is set to default, therefor one track is set to default
                continue;
            }
            args.Logger?.ILog($"Clearing default flag from subtitle[{stream.Index}]: {(stream.Title?.EmptyAsNull() ?? stream.Language?.EmptyAsNull() ?? stream.Stream.Codec)}");
            stream.IsDefault = false;
            stream.ForcedChange = true;
            changed = true;
        }

        return changed ? 1 : 2;
    }
}
