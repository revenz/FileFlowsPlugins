using System.Runtime.InteropServices;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Set a video codec encoding for a video stream based on users settings
/// </summary>
public partial class FfmpegBuilderVideoEncode:FfmpegBuilderNode
{
    /// <summary>
    /// The number of outputs for this flow element
    /// </summary>
    public override int Outputs => 1;

    internal const string CODEC_H264 = "h264";
    internal const string CODEC_H265 = "h265";
    internal const string CODEC_H265_8BIT = "h265 8BIT";
    internal const string CODEC_H265_10BIT = "h265 10BIT";
    internal const string CODEC_AV1 = "av1";
    internal const string CODEC_AV1_10BIT = "av1 10BIT";
    internal const string CODEC_VP9 = "vp9";
    
    internal const string ENCODER_CPU = "CPU";
    internal const string ENCODER_NVIDIA = "NVIDIA";
    internal const string ENCODER_QSV = "Intel QSV";
    internal const string ENCODER_VAAPI = "VAAPI";
    internal const string ENCODER_AMF = "AMD AMF";
    internal const string ENCODER_MAC = "Mac Video Toolbox";

    /// <summary>
    /// The Help URL for this flow element
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/video-encode";

    /// <summary>
    /// Gets or sets the codec used to encode
    /// </summary>
    [DefaultValue(CODEC_H264)]
    [ChangeValue(nameof(Quality), 23, CODEC_H264)]
    [ChangeValue(nameof(Quality), 28, CODEC_H265)]
    [ChangeValue(nameof(Quality), 28, CODEC_H265_10BIT)]
    [ChangeValue(nameof(Quality), 28, CODEC_AV1)]
    [ChangeValue(nameof(Quality), 28, CODEC_AV1_10BIT)]
    [ChangeValue(nameof(Quality), 28, CODEC_VP9)]
    [Select(nameof(CodecOptions), 1)]
    public string Codec { get; set; }

    private static List<ListOption> _CodecOptions;
    /// <summary>
    /// Gets or sets the codec options
    /// </summary>
    public static List<ListOption> CodecOptions
    {
        get
        {
            if (_CodecOptions == null)
            {
                _CodecOptions = new List<ListOption>
                {
                    new () { Label = "H.264", Value = CODEC_H264 },
                    // new () { Label = "H.264 (10-Bit)", Value = CODEC_H264_10BIT },
                    new () { Label = "HEVC (Automatic)", Value = CODEC_H265 },
                    new () { Label = "HEVC (8-Bit)", Value = CODEC_H265_8BIT },
                    new () { Label = "HEVC (10-Bit)", Value = CODEC_H265_10BIT },
                    new () { Label = "AV1", Value = CODEC_AV1 },
                    new () { Label = "AV1 (10-Bit)", Value = CODEC_AV1_10BIT },
                    new () { Label = "VP9", Value = CODEC_VP9 },
                };
            }
            return _CodecOptions;
        }
    }

    /// <summary>
    /// Gets or sets the encoder to use
    /// </summary>
    [Select(nameof(Encoders), 2)]
    [ConditionEquals(nameof(Codec), "/av1/", inverse: true)]
    public string Encoder { get; set; }
    
    private static List<ListOption> _Encoders;
    /// <summary>
    /// Gets or sets the encoders options
    /// </summary>
    public static List<ListOption> Encoders
    {
        get
        {
            if (_Encoders == null)
            {
                _Encoders = new List<ListOption>
                {
                    new () { Label = "Automatic", Value = "" },
                    new () { Label = "CPU", Value = "CPU" },
                    new () { Label = "NVIDIA", Value = "NVIDIA" },
                    new () { Label = "Intel QSV", Value = "Intel QSV" },
                    new () { Label = "VAAPI", Value = "VAAPI" },
                    new () { Label = "AMD AMF", Value = "AMD AMF" },
                    new () { Label = "Mac Video Toolbox", Value = "Mac Video Toolbox" },
                };
            }
            return _Encoders;
        }
    } 

    /// <summary>
    /// Gets or sets the quality of the video encode
    /// </summary>
    [Slider(3, inverse: true)]
    [Range(0, 51)]
    [DefaultValue(28)]
    public int Quality { get; set; }
    
    /// <summary>
    /// Gets or sets the speed to encode
    /// </summary>
    [Select(nameof(SpeedOptions), 4)]
    [DefaultValue("medium")]
    public string Speed { get; set; }

    private static List<ListOption> _SpeedOptions;
    /// <summary>
    /// Gets or sets the codec options
    /// </summary>
    public static List<ListOption> SpeedOptions
    {
        get
        {
            if (_SpeedOptions == null)
            {
                _SpeedOptions = new List<ListOption>
                {
                    new () { Label = "Very Slow", Value = "veryslow" },
                    new () { Label = "Slower", Value = "slower" },
                    new () { Label = "Slow", Value = "slow" },
                    new () { Label = "Medium", Value = "medium" },
                    new () { Label = "Fast", Value = "fast" },
                    new () { Label = "Faster", Value = "faster" },
                    new () { Label = "Very Fast", Value = "veryfast" },
                    new () { Label = "Super Fast", Value = "superfast" },
                    new () { Label = "Ultra Fast", Value = "ultrafast" },
                };
            }
            return _SpeedOptions;
        }
    }

    //private string bit10Filter = "yuv420p10le";
    private string[] bit10Filters = new[]
    {
        "-pix_fmt:v:{index}", "p010le", "-profile:v:{index}", "main10"
    };
    private string[] non10BitFilters = new string[]{};

    /// <summary>
    /// Executes the node
    /// </summary>
    /// <param name="args">The node arguments</param>
    /// <returns>the output return</returns>
    public override int Execute(NodeParameters args)
    {
        var stream = Model.VideoStreams.First(x => x.Deleted == false);
        
        stream.EncodingParameters.Clear();

        // remove anything that may have been added before
        stream.Filter = stream.Filter.Where(x => x?.StartsWith("scale_qsv") != true).ToList();

        string encoder = Environment.GetEnvironmentVariable("HW_OFF") == "1" || 
            (
                args.Variables?.TryGetValue("HW_OFF", out object oHwOff) == true && (oHwOff as bool? == true || oHwOff?.ToString() == "1")
            ) ? ENCODER_CPU : this.Encoder;

        args.Logger?.ILog("Quality: " + Quality);
        args.Logger?.ILog("Codec: " + Codec);
        if (Codec == CODEC_H264)
        {
            var encodingParameters = H264(args, false, Quality, encoder, Speed).ToArray();
            args.Logger?.ILog("Encoding Parameters: " +
                              string.Join(" ", encodingParameters.Select(x => x.Contains(" ") ? "\"" + x + "\"" : x)));
            stream.EncodingParameters.AddRange(encodingParameters);
            stream.Codec = CODEC_H264;
        }
        else if (Codec is CODEC_H265 or CODEC_H265_10BIT or CODEC_H265_8BIT)
        {
            bool tenBit = (Codec == CODEC_H265_10BIT || stream.Stream.Is10Bit) && (Codec != CODEC_H265_8BIT);
            bool forceBitSetting = Codec is CODEC_H265_10BIT or CODEC_H265_8BIT;
            args.Logger?.ILog("10 Bit: " + tenBit);
            var encodingParameters = H265(stream, args, tenBit, Quality, encoder, stream.Stream.FramesPerSecond, Speed, forceBitSetting: forceBitSetting).ToArray();
            args.Logger?.ILog("Encoding Parameters: " +
                              string.Join(" ", encodingParameters.Select(x => x.Contains(" ") ? "\"" + x + "\"" : x)));
            stream.EncodingParameters.AddRange(encodingParameters);
            stream.Codec = "hevc";
        }
        else if (Codec is CODEC_AV1 or CODEC_AV1_10BIT)
        {
            bool tenBit = Codec == CODEC_AV1_10BIT || stream.Stream.Is10Bit;
            args.Logger?.ILog("10 Bit: " + tenBit);
            var encodingParameters = AV1(args, tenBit, Quality, encoder, Speed).ToArray();
            args.Logger?.ILog("Encoding Parameters: " +
                              string.Join(" ", encodingParameters.Select(x => x.Contains(" ") ? "\"" + x + "\"" : x)));
            stream.EncodingParameters.AddRange(encodingParameters);
            stream.Codec = "av1";
        }
        else if (Codec == CODEC_VP9)
        {
            var encodingParameters = VP9(args, Quality, encoder, Speed).ToArray();
            args.Logger?.ILog("Encoding Parameters: " +
                              string.Join(" ", encodingParameters.Select(x => x.Contains(" ") ? "\"" + x + "\"" : x)));
            stream.EncodingParameters.AddRange(encodingParameters);
            stream.Codec = "vp9";
        }
        else
        {
            args.Logger?.ILog("Unknown codec: " + Codec);
            return 2;
        }

        stream.ForcedChange = true;
        return 1;
    }

    internal static IEnumerable<string> GetEncodingParameters(NodeParameters args, string codec, int quality, string encoder, float fps, string speed)
    {
        if (codec == CODEC_H264)
            return H264(args, false, quality, encoder, speed).Select(x => x.Replace("{index}", "0")); 
        if (codec == CODEC_H265 || codec == CODEC_H265_10BIT)
            return H265(null, args, codec == CODEC_H265_10BIT, quality, encoder, fps, speed).Select(x => x.Replace("{index}", "0"));
        if(codec == CODEC_AV1)
            return AV1(args, codec == CODEC_AV1_10BIT, quality, encoder, speed).Select(x => x.Replace("{index}", "0")); 
            
        throw new Exception("Unsupported codec: " + codec);
    }

    private static readonly bool IsMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    private static IEnumerable<string> H264(NodeParameters args, bool tenBit, int quality, string encoder, string speed)
    {
        List<string> parameters = new List<string>();
        string[] bit10Filters = null;
        string[] non10BitFilters = null;
        if (encoder == ENCODER_CPU)
            parameters.AddRange(H26x_CPU(false, quality, speed, out bit10Filters));
        else if(IsMac && encoder == ENCODER_MAC)
            parameters.AddRange(H26x_VideoToolbox(false, quality, speed));
        else if (encoder == ENCODER_NVIDIA)
            parameters.AddRange(H26x_Nvidia(false, quality, speed, out non10BitFilters));
        else if (encoder == ENCODER_QSV)
            parameters.AddRange(H26x_Qsv(false, quality, 0, speed));
        else if (encoder == ENCODER_AMF)
            parameters.AddRange(H26x_Amd(false, quality, speed));
        else if (encoder == ENCODER_VAAPI)
            parameters.AddRange(H26x_Vaapi(false, quality, speed));
        else if(IsMac && CanUseHardwareEncoding.CanProcess_VideoToolbox_H264(args))
            parameters.AddRange(H26x_VideoToolbox(false, quality, speed));
        else if (CanUseHardwareEncoding.CanProcess_Nvidia_H264(args))
            parameters.AddRange(H26x_Nvidia(false, quality, speed, out non10BitFilters));
        else if (CanUseHardwareEncoding.CanProcess_Qsv_H264(args))
        {
            parameters.AddRange(H26x_Qsv(false, quality, 0, speed));
            encoder = ENCODER_QSV;
        }
        else if (CanUseHardwareEncoding.CanProcess_Amd_H264(args))
            parameters.AddRange(H26x_Amd(false, quality, speed));
        else if (CanUseHardwareEncoding.CanProcess_Vaapi_H264(args))
            parameters.AddRange(H26x_Vaapi(false, quality, speed));
        else
            parameters.AddRange(H26x_CPU(false, quality, speed, out bit10Filters));

        if (tenBit)
        {
            parameters.AddRange(bit10Filters ?? new string[]
                { "-pix_fmt:v:{index}", "p010le", "-profile:v:{index}", "main10" });
        }

        return parameters;
    }
    
    private static IEnumerable<string> H265(FfmpegVideoStream stream, NodeParameters args, bool tenBit, int quality, 
        string encoder, float fps, string speed, bool forceBitSetting = false)
    {
        // hevc_qsv -load_plugin hevc_hw -pix_fmt p010le -profile:v main10 -global_quality 21 -g 24 -look_ahead 1 -look_ahead_depth 60
        List<string> parameters = new List<string>();
        string[] bit10Filters = null;
        string[] non10BitFilters = null;
        bool qsv = false;
        if (encoder == ENCODER_CPU)
            parameters.AddRange(H26x_CPU(true, quality, speed, out bit10Filters));
        else if (IsMac && encoder == ENCODER_MAC)
            parameters.AddRange(H26x_VideoToolbox(true, quality, speed));
        else if (encoder == ENCODER_NVIDIA)
            parameters.AddRange(H26x_Nvidia(true, quality, speed, out non10BitFilters));
        else if (encoder == ENCODER_QSV)
        {
            parameters.AddRange(H26x_Qsv(true, quality, fps, speed));
            qsv = true;
        }
        else if (encoder == ENCODER_AMF)
            parameters.AddRange(H26x_Amd(true, quality, speed));
        else if (encoder == ENCODER_VAAPI)
            parameters.AddRange(H26x_Vaapi(true, quality, speed));
        
        else if (IsMac && CanUseHardwareEncoding.CanProcess_VideoToolbox_Hevc(args))
            parameters.AddRange(H26x_VideoToolbox(true, quality, speed));
        else if (CanUseHardwareEncoding.CanProcess_Nvidia_Hevc(args))
            parameters.AddRange(H26x_Nvidia(true, quality, speed, out non10BitFilters));
        else if (CanUseHardwareEncoding.CanProcess_Qsv_Hevc(args))
        {
            parameters.AddRange(H26x_Qsv(true, quality, fps, speed));
            qsv = true;
        }
        else if (CanUseHardwareEncoding.CanProcess_Amd_Hevc(args))
            parameters.AddRange(H26x_Amd(true, quality, speed));
        else if (CanUseHardwareEncoding.CanProcess_Vaapi_Hevc(args))
            parameters.AddRange(H26x_Vaapi(true, quality, speed));
        else
            parameters.AddRange(H26x_CPU(true, quality, speed, out bit10Filters));

        if (tenBit)
        {
            if (qsv)
            {
                parameters.AddRange(new []
                {
                    "-profile:v:{index}", "main10", "-pix_fmt", "p010le" //, "-vf", "scale_qsv=format=p010le"
                });
                // if the stream is passed in, we add it to the filter
                // if(stream != null)
                //     stream.Filter.Add("scale_qsv=format=p010le");
                // else // if there is no stream, we specify the -vf directly, this is called by audio to video flow elements
                //     parameters.AddRange(new [] {  "-vf", "scale_qsv=format=p010le" });
                
            }
            else
            {
                parameters.AddRange(bit10Filters ?? new []
                    { "-pix_fmt:v:{index}", "p010le", "-profile:v:{index}", "main10" });
            }
        }
        else if(non10BitFilters?.Any() == true)
            parameters.AddRange(non10BitFilters);
        else if (forceBitSetting)
        {
            parameters.AddRange(new[] { "-pix_fmt:v:{index}", "yuv420p" });
        }
        return parameters;
    }

    
    private static IEnumerable<string> AV1(NodeParameters args, bool tenBit, int quality, string encoder, string speed)
    {
        // hevc_qsv -load_plugin hevc_hw -pix_fmt p010le -profile:v main10 -global_quality 21 -g 24 -look_ahead 1 -look_ahead_depth 60
        List<string> parameters = new List<string>();
        
        if (encoder == ENCODER_CPU)
            parameters.AddRange(AV1_CPU(quality, speed));
        else if(encoder == ENCODER_NVIDIA)
            parameters.AddRange(AV1_Nvidia(quality, speed));
        else if(encoder == ENCODER_QSV)
            parameters.AddRange(AV1_Qsv(quality, speed));
        else if(encoder == ENCODER_AMF)
            parameters.AddRange(AV1_Amd(quality, speed));
        
        else if (CanUseHardwareEncoding.CanProcess_Nvidia_AV1(args))
            parameters.AddRange(AV1_Nvidia(quality, speed));
        else if (CanUseHardwareEncoding.CanProcess_Qsv_AV1(args))
            parameters.AddRange(AV1_Qsv(quality, speed));
        else if (CanUseHardwareEncoding.CanProcess_Amd_AV1(args))
            parameters.AddRange(AV1_Amd(quality, speed));
        else
            parameters.AddRange(AV1_CPU(quality, speed));

        if (tenBit)
        {
            bool qsv = parameters.Any(x => x.ToLowerInvariant().Contains("qsv"));
            bool av1 = parameters.Any(x => x.ToLowerInvariant().Contains("av1"));
            if(qsv && av1)
                parameters.AddRange(new[] { "-pix_fmt", "p010le" });
            else if(qsv)
                parameters.AddRange(new[] { "-vf", "scale_qsv=format=p010le" });
            else
                parameters.AddRange(new[] { "-pix_fmt:v:{index}", "yuv420p10le" });
        }

        return parameters;
    }
    
    private static IEnumerable<string> VP9(NodeParameters args,  int quality, string encoder, string speed)
    {
        List<string> parameters = new List<string>();
        if (encoder == ENCODER_CPU)
            parameters.AddRange(VP9_CPU(quality, speed));
        else if (encoder == ENCODER_QSV) // if can use hevc they can use vp9
            parameters.AddRange(VP9_Qsv(quality, speed));
        else if (CanUseHardwareEncoding.CanProcess_Qsv_Hevc(args)) // if can use hevc they can use vp9
            parameters.AddRange(VP9_Qsv(quality, speed));
        else
            parameters.AddRange(VP9_CPU(quality, speed));
        return parameters;
    }

    
    
    /// <summary>
    /// Gets the actually speed variable to use
    /// </summary>
    /// <param name="nvidia">true if using nvidia or not</param>
    /// <returns>the speed variable</returns>
    private static string GetSpeed(string speed, bool nvidia = false)
    {
        if (string.IsNullOrWhiteSpace(speed))
            return nvidia ? "p6" : "slower";
        if (nvidia)
        {
            switch (speed)
            {
                case "ultrafast":
                case "superfast":
                case "veryfast": 
                case "faster": return "p1";
                case "fast": return "p2";
                case "medium": return "p3";
                case "slow": return "p5";
                case "slower": return "p6";
                case "veryslow": return "p7";
            }

            return "p6"; // unknown
        }
        return speed.ToLowerInvariant();
    }
}
