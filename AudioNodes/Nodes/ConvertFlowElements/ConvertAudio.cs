namespace FileFlows.AudioNodes;

public class ConvertAudio : ConvertNode
{
    protected override string DefaultExtension => Codec;
    public override string HelpUrl => "https://fileflows.com/docs/plugins/audio-nodes/convert-audio";

    public static List<ListOption> BitrateOptions => ConvertNode.BitrateOptions;

    [Select(nameof(CodecOptions), 0)]
    public string Codec { get; set; }

    /// <summary>
    /// Gets or sets if high efficiency should be used
    /// </summary>
    [Boolean(5)]
    [ConditionEquals(nameof(Codec), "aac")]
    public bool HighEfficiency { get => base.HighEfficiency; set =>base.HighEfficiency = value; }

    private static List<ListOption> _CodecOptions;
    public static List<ListOption> CodecOptions
    {
        get
        {
            if (_CodecOptions == null)
            {
                _CodecOptions = new List<ListOption>
                {
                    new () { Label = "AAC", Value = "aac"},
                    new () { Label = "MP3", Value = "MP3"},
                    new () { Label = "OGG (Vorbis)", Value = "ogg"},
                    new () { Label = "OGG (Opus)", Value = "libopus"},
                    new () { Label = "WAV", Value = "wav"},
                };
            }
            return _CodecOptions;
        }
    }

    public override int Execute(NodeParameters args)
    {
        var audioInfoResult = GetAudioInfo(args);
        if (audioInfoResult.Failed(out string error))
        {
            args.Logger?.ELog(error);
            args.FailureReason = error;
            return -1;
        }

        AudioInfo AudioInfo = audioInfoResult.Value; 

        string ffmpegExe = GetFFmpeg(args);
        if (string.IsNullOrEmpty(ffmpegExe))
            return -1;
            
        return base.Execute(args);

    }
}
