namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Sets the device to use
/// </summary>
public class FfmpegBuilderSetDevice : FfmpegBuilderNode
{
    /// <inheritdoc/>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/set-device";

    /// <inheritdoc/>
    public override int Outputs => 1;

    /// <inheritdoc/>
    public override string Icon => "fas fa-plug";
    
    /// <summary>
    /// Gets or sets the device
    /// </summary>
    [TextVariable(1)]
    public string Device { get; set; }

    /// <inheritdoc/>
    public override int Execute(NodeParameters args)
    {
        string device = args.ReplaceVariables(Device ?? string.Empty, stripMissing: true);
        args.Logger?.ILog("Device: " + device);

        Model.Device = device?.EmptyAsNull() ?? "NONE";
        return 1;
    }
}