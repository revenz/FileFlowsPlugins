using System.Runtime.InteropServices;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Set a video codec encoding for a video stream based on users settings
/// </summary>
public partial class FfmpegBuilderVideoEncodeSimple:VideoEncodeBase
{
    /// <summary>
    /// The number of outputs for this flow element
    /// </summary>
    public override int Outputs => 1;

    /// <summary>
    /// The Help URL for this flow element
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/video-encode";

    /// <summary>
    /// Gets or sets the codec used to encode
    /// </summary>
    [DefaultValue(CODEC_H264)]
    [Select(nameof(CodecOptions), 1)]
    public string Codec { get; set; }
    
    /// <summary>
    /// Gets or sets the encoder to use
    /// </summary>
    [Select(nameof(Encoders), 2)]
    public string Encoder { get; set; }
    
    /// <summary>
    /// Gets or sets the quality of the video encode
    /// </summary>
    [Slider(3, hideValue: false)]
    [Range(1, 9)]
    [DefaultValue(5)]
    public int Quality { get; set; }

    private const int minQuality = 1;
    private const int maxQuality = 9;
    
    /// <summary>
    /// Gets or sets the speed to encode
    /// </summary>
    [Slider(4, hideValue: false)]
    [Range(1, 5)]
    [DefaultValue(3)]
    public int Speed { get; set; }
    
    /// <summary>
    /// Executes the node
    /// </summary>
    /// <param name="args">The node arguments</param>
    /// <returns>the output return</returns>
    public override int Execute(NodeParameters args)
    {
        var stream = Model.VideoStreams.FirstOrDefault(x => x.Deleted == false);
        if (stream == null)
            return args.Fail("No non-deleted video stream");
        
        stream.EncodingParameters.Clear();

        // remove anything that may have been added before
        stream.Filter = stream.Filter.Where(x => x?.StartsWith("scale_qsv") != true).ToList();

        string encoder = Environment.GetEnvironmentVariable("HW_OFF") == "1" || 
            (
                args.Variables?.TryGetValue("HW_OFF", out object? oHwOff) == true && (oHwOff as bool? == true || oHwOff?.ToString() == "1")
            ) ? ENCODER_CPU : this.Encoder;


        args.Logger?.ILog("Quality: " + Quality);
        args.Logger?.ILog("Speed: " + Speed);
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
            var encodingParameters = AV1(args, tenBit, Quality, encoder, Speed, Model.Device).ToArray();
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

    internal static IEnumerable<string> GetEncodingParameters(NodeParameters args, string codec, int quality, string encoder, float fps, int speed, string? device = null)
    {
        if (codec == CODEC_H264)
            return H264(args, false, quality, encoder, speed).Select(x => x.Replace("{index}", "0")); 
        if (codec == CODEC_H265 || codec == CODEC_H265_10BIT)
            return H265(null, args, codec == CODEC_H265_10BIT, quality, encoder, fps, speed).Select(x => x.Replace("{index}", "0"));
        if(codec == CODEC_AV1)
            return AV1(args, codec == CODEC_AV1_10BIT, quality, encoder, speed, device).Select(x => x.Replace("{index}", "0")); 
            
        throw new Exception("Unsupported codec: " + codec);
    }

    private static readonly bool IsMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    private static IEnumerable<string> H264(NodeParameters args, bool tenBit, int quality, string encoder, int speed)
    {
        List<string> parameters = new List<string>();
        string[]? bit10Filters = null;
        string[]? non10BitFilters = null;
        if (encoder == ENCODER_CPU)
            parameters.AddRange(H26x_CPU(false, quality, speed, out bit10Filters));
        else if(IsMac && encoder == ENCODER_MAC)
            parameters.AddRange(H26x_VideoToolbox(false, quality, speed));
        else if (encoder == ENCODER_NVIDIA)
            parameters.AddRange(H26x_Nvidia(false, quality, speed, out non10BitFilters));
        else if (encoder == ENCODER_QSV)
            parameters.AddRange(H26x_Qsv(false, quality, 0, speed));
        else if (encoder == ENCODER_AMF)
            parameters.AddRange(H26x_Amd(false, quality, speed, out bit10Filters));
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
            parameters.AddRange(H26x_Amd(false, quality, speed, out bit10Filters));
        else if (CanUseHardwareEncoding.CanProcess_Vaapi_H264(args))
            parameters.AddRange(H26x_Vaapi(false, quality, speed));
        else
            parameters.AddRange(H26x_CPU(false, quality, speed, out bit10Filters));

        if (tenBit)
        {
            parameters.AddRange(bit10Filters ?? ["-pix_fmt:v:{index}", "p010le", "-profile:v:{index}", "main10"]);
        }

        return parameters;
    }
    
    private static IEnumerable<string> H265(FfmpegVideoStream stream, NodeParameters args, bool tenBit, int quality, 
        string encoder, float fps, int speed, bool forceBitSetting = false)
    {
        // hevc_qsv -load_plugin hevc_hw -pix_fmt p010le -profile:v main10 -global_quality 21 -g 24 -look_ahead 1 -look_ahead_depth 60
        List<string> parameters = new List<string>();
        string[]? bit10Filters = null;
        string[]? non10BitFilters = null;
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
            parameters.AddRange(H26x_Amd(true, quality, speed, out bit10Filters));
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
            parameters.AddRange(H26x_Amd(true, quality, speed, out bit10Filters));
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
                parameters.AddRange(bit10Filters ?? 
                                    ["-pix_fmt:v:{index}", "p010le", "-profile:v:{index}", "main10"]);
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

    
    private static IEnumerable<string> AV1(NodeParameters args, bool tenBit, int quality, string encoder, int speed, string device)
    {
        // hevc_qsv -load_plugin hevc_hw -pix_fmt p010le -profile:v main10 -global_quality 21 -g 24 -look_ahead 1 -look_ahead_depth 60
        List<string> parameters = new List<string>();
        
        if (encoder == ENCODER_CPU)
            parameters.AddRange(AV1_CPU(quality, speed));
        else if(encoder == ENCODER_NVIDIA)
            parameters.AddRange(AV1_Nvidia(quality, speed));
        else if(encoder == ENCODER_QSV)
            parameters.AddRange(AV1_Qsv(device, quality, speed));
        else if(encoder == ENCODER_AMF)
            parameters.AddRange(AV1_Amd(quality, speed));
        
        else if (CanUseHardwareEncoding.CanProcess_Nvidia_AV1(args))
            parameters.AddRange(AV1_Nvidia(quality, speed));
        else if (CanUseHardwareEncoding.CanProcess_Qsv_AV1(args))
            parameters.AddRange(AV1_Qsv(device, quality, speed));
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
    
    private static IEnumerable<string> VP9(NodeParameters args,  int quality, string encoder, int speed)
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
