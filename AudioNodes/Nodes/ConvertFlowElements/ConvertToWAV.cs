namespace FileFlows.AudioNodes;

public class ConvertToWAV : ConvertNode
{
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/audio-nodes/convert-to-wav";
    /// <inheritdoc />
    protected override string Extension => "wav";
    /// <inheritdoc />
    public override string Icon => "svg:wav";
        
    private static List<ListOption> _BitrateOptions;
    public new static List<ListOption> BitrateOptions
    {
        get
        {
            if (_BitrateOptions == null)
            {
                _BitrateOptions = new List<ListOption>
                {
                    new () { Label = "Automatic", Value = 0 },
                    new () { Label = "Same as source", Value = -1 },
                    new () { Label = "64 Kbps", Value = 64},
                    new () { Label = "96 Kbps", Value = 96},
                    new () { Label = "128 Kbps", Value = 128},
                    new () { Label = "160 Kbps", Value = 160},
                    new () { Label = "192 Kbps", Value = 192},
                    new () { Label = "224 Kbps", Value = 224},
                    new () { Label = "256 Kbps", Value = 256},
                    new () { Label = "288 Kbps", Value = 288},
                    new () { Label = "320 Kbps", Value = 320},
                };
            }
            return _BitrateOptions;
        }
    }
}