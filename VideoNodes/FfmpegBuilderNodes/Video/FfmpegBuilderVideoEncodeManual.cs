namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Flow element to do manual video encode
/// </summary>
public class FfmpegBuilderVideoManual:FfmpegBuilderNode
{
    /// <inheritdoc />
    public override int Outputs => 1;

    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/video-encode-manual";
    
    /// <summary>
    /// Gets or sets the video encoding parameters
    /// </summary>
    [DefaultValue("hevc_nvenc -preset hq -crf 23")]
    [TextVariable(1)]
    public string Parameters { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        string parameters = args.ReplaceVariables(Parameters);

        if (string.IsNullOrWhiteSpace(parameters))
            return args.Fail("No encoding parameters specified");

        parameters = CheckVideoCodec(FFMPEG, parameters);
        
        var stream = Model.VideoStreams.First(x => x.Deleted == false);

        stream.EncodingParameters.Clear();
        stream.EncodingParameters.AddRange(SplitCommand(parameters));
        args.Logger?.ILog("Setting encoding parameters to: " + parameters);
        return 1;
    }

}