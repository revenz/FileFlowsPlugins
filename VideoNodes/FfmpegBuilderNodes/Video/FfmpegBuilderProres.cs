namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// FFmpeg Builder Prores encoding node
/// </summary>
public class FfmpegBuilderProres : FfmpegBuilderNode
{
    /// <summary>
    /// The number of outputs for this flow element
    /// </summary>
    public override int Outputs => 1;
    /// <summary>
    /// The Help URL for this flow element
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/prores";

    /// <summary>
    /// Gets that this is an enterprise flow element
    /// </summary>
    public override LicenseLevel LicenseLevel => LicenseLevel.Enterprise;
    
    /// <summary>
    /// Gets or sets the encoder to use
    /// </summary>
    [Select(nameof(Encoders), 1)]
    [DefaultValue(4)]
    public string Encoder { get; set; }
    
    private static List<ListOption> _Encoders;
    /// <summary>
    /// Gets or sets the encoder options
    /// </summary>
    public static List<ListOption> Encoders
    {
        get
        {
            if (_Encoders == null)
            {
                _Encoders = new List<ListOption>
                {
                    new () { Label = "prores", Value = "prores" },
                    new () { Label = "prores_ks / prores_kostya", Value = "prores_ks" },
                    new () { Label = "prores_aw / prores_anatolyi", Value = "prores_aw" }
                };
            }
            return _Encoders;
        }
    } 

    /// <summary>
    /// Gets or sets the profile to use
    /// </summary>
    [Select(nameof(Profiles), 2)]
    [DefaultValue(3)]
    public int Profile { get; set; }
    
    private static List<ListOption> _Profiles;
    /// <summary>
    /// Gets or sets the profile options
    /// </summary>
    public static List<ListOption> Profiles
    {
        get
        {
            if (_Profiles == null)
            {
                _Profiles = new List<ListOption>
                {
                    new () { Label = "Proxy", Value = 0 },
                    new () { Label = "LT", Value = 1 },
                    new () { Label = "SQ", Value = 2 },
                    new () { Label = "HQ", Value = 3 }
                };
            }
            return _Profiles;
        }
    } 
    
    /// <summary>
    /// Gets or sets the pixel format to use
    /// </summary>
    [Select(nameof(PixelFormats), 3)]
    [DefaultValue("yuva444p10le")]
    public string PixelFormat { get; set; }
    
    private static List<ListOption> _PixelFormats;
    /// <summary>
    /// Gets or sets the pixel formats options
    /// </summary>
    public static List<ListOption> PixelFormats
    {
        get
        {
            if (_PixelFormats == null)
            {
                _PixelFormats = new List<ListOption>
                {
                    new () { Label = "Not Set", Value = string.Empty },
                    new () { Label = "4:2:2", Value = "yuva422p10le" },
                    new () { Label = "4:4:4", Value = "yuva444p10le" }
                };
            }
            return _PixelFormats;
        }
    } 
    
    /// <summary>
    /// Gets or sets the profile to use
    /// </summary>
    [Slider(4, inverse: true)]
    [Range(0, 32)]
    [DefaultValue(9)]
    public int Quality { get; set; }
    

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        var stream = Model.VideoStreams.FirstOrDefault(x => x.Deleted == false);
        if (stream == null)
        {
            args.Logger.ELog("No video stream found");
            return -1;
        }
        
        stream.EncodingParameters.Clear();
        
        // prores_ks -profile:v 3 -qscale:v 13 -vendor apl0 -pix_fmt yuva444p10le
        string encoder = Encoder?.EmptyAsNull() ?? "prores";
        stream.EncodingParameters.Add(encoder);
        stream.EncodingParameters.Add("-profile:v");
        int profile = this.Profile;
        if (profile < 0)
            profile = 0;
        if (profile > 3)
            profile = 3;
        stream.EncodingParameters.Add(profile.ToString());
        
        int quality = this.Quality;
        if (quality < 0)
            quality = 0;
        if (quality > 32)
            quality = 32;
        stream.EncodingParameters.Add("-qscale:v");
        stream.EncodingParameters.Add(quality.ToString());

        stream.EncodingParameters.Add("-vendor");
        stream.EncodingParameters.Add("apl0");

        args.Logger?.ILog("Encoding Prores Encoder: " + encoder);
        args.Logger?.ILog("Encoding Prores Profile: " + profile);
        args.Logger?.ILog("Encoding Prores Quality: " + quality);
        if (string.IsNullOrWhiteSpace(PixelFormat) == false)
        {
            stream.EncodingParameters.Add("-pix_fmt");
            stream.EncodingParameters.Add(PixelFormat);
            args.Logger?.ILog("Encoding Prores Pixel Format: " + PixelFormat);
        }

        stream.ForcedChange = true;
        return 1;
    }
}