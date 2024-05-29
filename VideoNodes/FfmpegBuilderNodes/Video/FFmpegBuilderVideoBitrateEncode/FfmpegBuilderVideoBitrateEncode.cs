using System.Runtime.InteropServices;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Set a video codec encoding by bitrate for a video stream based on users settings
/// </summary>
public partial class FfmpegBuilderVideoBitrateEncode:VideoEncodeBase
{
    /// <summary>
    /// The number of outputs for this flow element
    /// </summary>
    public override int Outputs => 1;

    /// <summary>
    /// The Help URL for this flow element
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/bitrate-encode";

    /// <summary>
    /// Gets or sets the codec used to encode
    /// </summary>
    [DefaultValue(CODEC_H265)]
    [Select(nameof(CodecOptions), 1)]
    public string Codec { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the encoder to use
    /// </summary>
    [Select(nameof(Encoders), 2)]
    //[ConditionEquals(nameof(Codec), "/av1/", inverse: true)]
    public string Encoder { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the bitrate to use in Kbps
    /// </summary>
    [NumberInt(3)]
    [DefaultValue(5_000)]
    public int Bitrate { get; set; }

    

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
                args.Variables?.TryGetValue("HW_OFF", out object? oHwOff) == true && (oHwOff as bool? == true || oHwOff?.ToString() == "1")
            ) ? ENCODER_CPU : this.Encoder;

        args.Logger?.ILog("Bitrate: " + Bitrate);
        args.Logger?.ILog("Codec: " + Codec);
        if (Codec == CODEC_H264)
        {
            var encodingParameters = H264(args, false, encoder, Bitrate).ToArray();
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
            var encodingParameters = H265(stream, args, tenBit, Bitrate, encoder, stream.Stream.FramesPerSecond, forceBitSetting: forceBitSetting).ToArray();
            
            
            args.Logger?.ILog("Encoding Parameters: " +
                              string.Join(" ", encodingParameters.Select(x => x.Contains(" ") ? "\"" + x + "\"" : x)));
            stream.EncodingParameters.AddRange(encodingParameters);
            stream.Codec = "hevc";
        }
        else if (Codec is CODEC_AV1 or CODEC_AV1_10BIT)
        {
            bool tenBit = Codec == CODEC_AV1_10BIT || stream.Stream.Is10Bit;
            args.Logger?.ILog("10 Bit: " + tenBit);
            var encodingParameters = AV1(args, tenBit, Bitrate, encoder).ToArray();
            
            args.Logger?.ILog("Encoding Parameters: " +
                              string.Join(" ", encodingParameters.Select(x => x.Contains(" ") ? "\"" + x + "\"" : x)));
            stream.EncodingParameters.AddRange(encodingParameters);
            stream.Codec = "av1";
        }
        else if (Codec == CODEC_VP9)
        {
            var encodingParameters = VP9(args, Bitrate, encoder).ToArray();
            
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

    /// <summary>
    /// Adjust the parameters to use a constant bitrate
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <param name="parameters">the parameters to alter</param>
    /// <returns>the adjusted parmaters</returns>
    private string[] AdjustForBitrate(NodeParameters args, string[] parameters)
    {
        var toRemove = new [] { "-rc", "-qp", "-preset", "-spatial-aq", "-g", "-global_quality:v" };
        int index = Array.FindIndex(parameters, p => toRemove.Contains(p));
        var modified = new List<string>();
        for (int i = 0; i < parameters.Length - 1; i++)
        {
            if (toRemove.Contains(parameters[i]))
            {
                i++;
                continue;
            }
            modified.Add(parameters[i]);
        }
        modified.Insert(index, "-b:v:{index}");
        modified.Insert(index + 1, Bitrate + "k");
        return modified.ToArray();
    }

    internal static IEnumerable<string> GetEncodingParameters(NodeParameters args, string codec, int bitrate, string encoder, float fps)
    {
        if (codec == CODEC_H264)
            return H264(args, false, encoder, bitrate).Select(x => x.Replace("{index}", "0")); 
        if (codec == CODEC_H265 || codec == CODEC_H265_10BIT)
            return H265(null, args, codec == CODEC_H265_10BIT, bitrate, encoder, fps).Select(x => x.Replace("{index}", "0"));
        if(codec == CODEC_AV1)
            return AV1(args, codec == CODEC_AV1_10BIT, bitrate, encoder).Select(x => x.Replace("{index}", "0")); 
            
        throw new Exception("Unsupported codec: " + codec);
    }

    private static readonly bool IsMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    private static IEnumerable<string> H264(NodeParameters args, bool tenBit, string encoder, int bitrate)
    {
        List<string> parameters = new List<string>();
        string[]? bit10Filters = null;
        string[]? non10BitFilters = null;
        if (encoder == ENCODER_CPU)
            parameters.AddRange(H26x_CPU(false, bitrate, out bit10Filters));
        else if(IsMac && encoder == ENCODER_MAC)
            parameters.AddRange(H26x_VideoToolbox(false, bitrate));
        else if (encoder == ENCODER_NVIDIA)
            parameters.AddRange(H26x_Nvidia(false, bitrate, out non10BitFilters));
        else if (encoder == ENCODER_QSV)
            parameters.AddRange(H26x_Qsv(false, bitrate, 0));
        else if (encoder == ENCODER_AMF)
            parameters.AddRange(H26x_Amd(false, bitrate));
        else if (encoder == ENCODER_VAAPI)
            parameters.AddRange(H26x_Vaapi(false, bitrate));
        else if(IsMac && CanUseHardwareEncoding.CanProcess_VideoToolbox_H264(args))
            parameters.AddRange(H26x_VideoToolbox(false, bitrate));
        else if (CanUseHardwareEncoding.CanProcess_Nvidia_H264(args))
            parameters.AddRange(H26x_Nvidia(false,bitrate, out non10BitFilters));
        else if (CanUseHardwareEncoding.CanProcess_Qsv_H264(args))
        {
            parameters.AddRange(H26x_Qsv(false, bitrate, 0));
            encoder = ENCODER_QSV;
        }
        else if (CanUseHardwareEncoding.CanProcess_Amd_H264(args))
            parameters.AddRange(H26x_Amd(false, bitrate));
        else if (CanUseHardwareEncoding.CanProcess_Vaapi_H264(args))
            parameters.AddRange(H26x_Vaapi(false, bitrate));
        else
            parameters.AddRange(H26x_CPU(false, bitrate, out bit10Filters));

        if (tenBit)
        {
            parameters.AddRange(bit10Filters ?? new string[]
                { "-pix_fmt:v:{index}", "p010le", "-profile:v:{index}", "main10" });
        }

        return parameters;
    }
    
    private static IEnumerable<string> H265(FfmpegVideoStream stream, NodeParameters args, bool tenBit, int bitrate, 
        string encoder, float fps, bool forceBitSetting = false)
    {
        // hevc_qsv -load_plugin hevc_hw -pix_fmt p010le -profile:v main10 -global_quality 21 -g 24 -look_ahead 1 -look_ahead_depth 60
        List<string> parameters = new List<string>();
        string[]? bit10Filters = null;
        string[]? non10BitFilters = null;
        bool qsv = false;
        if (encoder == ENCODER_CPU)
            parameters.AddRange(H26x_CPU(true, bitrate, out bit10Filters));
        else if (IsMac && encoder == ENCODER_MAC)
            parameters.AddRange(H26x_VideoToolbox(true, bitrate));
        else if (encoder == ENCODER_NVIDIA)
            parameters.AddRange(H26x_Nvidia(true, bitrate, out non10BitFilters));
        else if (encoder == ENCODER_QSV)
        {
            parameters.AddRange(H26x_Qsv(true, bitrate, fps));
            qsv = true;
        }
        else if (encoder == ENCODER_AMF)
            parameters.AddRange(H26x_Amd(true, bitrate));
        else if (encoder == ENCODER_VAAPI)
            parameters.AddRange(H26x_Vaapi(true, bitrate));
        
        else if (IsMac && CanUseHardwareEncoding.CanProcess_VideoToolbox_Hevc(args))
            parameters.AddRange(H26x_VideoToolbox(true, bitrate));
        else if (CanUseHardwareEncoding.CanProcess_Nvidia_Hevc(args))
            parameters.AddRange(H26x_Nvidia(true, bitrate, out non10BitFilters));
        else if (CanUseHardwareEncoding.CanProcess_Qsv_Hevc(args))
        {
            parameters.AddRange(H26x_Qsv(true, bitrate, fps));
            qsv = true;
        }
        else if (CanUseHardwareEncoding.CanProcess_Amd_Hevc(args))
            parameters.AddRange(H26x_Amd(true, bitrate));
        else if (CanUseHardwareEncoding.CanProcess_Vaapi_Hevc(args))
            parameters.AddRange(H26x_Vaapi(true, bitrate));
        else
            parameters.AddRange(H26x_CPU(true, bitrate, out bit10Filters));

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

    
    private static IEnumerable<string> AV1(NodeParameters args, bool tenBit, int bitrate, string encoder)
    {
        // hevc_qsv -load_plugin hevc_hw -pix_fmt p010le -profile:v main10 -global_quality 21 -g 24 -look_ahead 1 -look_ahead_depth 60
        List<string> parameters = new List<string>();
        
        if (encoder == ENCODER_CPU)
            parameters.AddRange(AV1_CPU(bitrate));
        else if(encoder == ENCODER_NVIDIA)
            parameters.AddRange(AV1_Nvidia(bitrate));
        else if(encoder == ENCODER_QSV)
            parameters.AddRange(AV1_Qsv(bitrate));
        else if(encoder == ENCODER_AMF)
            parameters.AddRange(AV1_Amd(bitrate));
        
        else if (CanUseHardwareEncoding.CanProcess_Nvidia_AV1(args))
            parameters.AddRange(AV1_Nvidia(bitrate));
        else if (CanUseHardwareEncoding.CanProcess_Qsv_AV1(args))
            parameters.AddRange(AV1_Qsv(bitrate));
        else if (CanUseHardwareEncoding.CanProcess_Amd_AV1(args))
            parameters.AddRange(AV1_Amd(bitrate));
        else
            parameters.AddRange(AV1_CPU(bitrate));

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
    
    private static IEnumerable<string> VP9(NodeParameters args,  int bitrate, string encoder)
    {
        List<string> parameters = new List<string>();
        if (encoder == ENCODER_CPU)
            parameters.AddRange(VP9_CPU(bitrate));
        else if (encoder == ENCODER_QSV) // if can use hevc they can use vp9
            parameters.AddRange(VP9_Qsv(bitrate));
        else if (CanUseHardwareEncoding.CanProcess_Qsv_Hevc(args)) // if can use hevc they can use vp9
            parameters.AddRange(VP9_Qsv(bitrate));
        else
            parameters.AddRange(VP9_CPU(bitrate));
        return parameters;
    }
}
