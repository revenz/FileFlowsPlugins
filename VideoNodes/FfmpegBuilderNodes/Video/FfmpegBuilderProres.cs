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
    public override bool Enterprise => true;

    /// <summary>
    /// Gets or sets the profile to use
    /// </summary>
    [Select(nameof(Profiles), 1)]
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
    [Select(nameof(PixelFormats), 2)]
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
    [Slider(3)]
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
        if (args.Enterprise != true)
        {
            args.Logger.ELog("Requires an Enterprise license to use this flow element.");
            return -1;
        }
        
        var stream = Model.VideoStreams.FirstOrDefault(x => x.Deleted == false);
        if (stream == null)
        {
            args.Logger.ELog("No video stream found");
            return -1;
        }
        
        stream.EncodingParameters.Clear();
        
        // prores_ks -profile:v 3 -qscale:v 13 -vendor apl0 -pix_fmt yuva444p10le
        stream.EncodingParameters.Add("prores_ks");
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

        string pix_fmt = PixelFormat?.EmptyAsNull() ?? "yuva422p10le";
        stream.EncodingParameters.Add("-pix_fmt");
        stream.EncodingParameters.Add(pix_fmt);

        args.Logger?.ILog("Encoding Prores Profile: " + profile);
        args.Logger?.ILog("Encoding Prores Quality: " + quality);
        args.Logger?.ILog("Encoding Prores Pixel Format: " + pix_fmt);

        stream.ForcedChange = true;
        return 1;
    }
}