using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using System.Runtime.InteropServices;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Set a video codec encoding for a video stream based on users settings
/// </summary>
public class FfmpegBuilderVideoEncode:FfmpegBuilderNode
{
    /// <summary>
    /// The number of outputs for this node
    /// </summary>
    public override int Outputs => 1;

    internal const string CODEC_H264 = "h264";
    internal const string CODEC_H265 = "h265";
    internal const string CODEC_H265_10BIT = "h265 10BIT";
    internal const string CODEC_AV1 = "av1";
    internal const string CODEC_AV1_10BIT = "av1 10BIT";
    

    /// <summary>
    /// The Help URL for this node
    /// </summary>
    public override string HelpUrl => "https://docs.fileflows.com/plugins/video-nodes/ffmpeg-builder/video-encode";

    /// <summary>
    /// Gets or sets the codec used to encode
    /// </summary>
    [DefaultValue(CODEC_H264)]
    [ChangeValue(nameof(Quality), 23, CODEC_H264)]
    [ChangeValue(nameof(Quality), 28, CODEC_H265)]
    [ChangeValue(nameof(Quality), 28, CODEC_H265_10BIT)]
    [ChangeValue(nameof(Quality), 28, CODEC_AV1)]
    [ChangeValue(nameof(Quality), 28, CODEC_AV1_10BIT)]
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
                    new () { Label = "H.265", Value = CODEC_H265 },
                    new () { Label = "H.265 (10-Bit)", Value = CODEC_H265_10BIT },
                    new () { Label = "AV1", Value = CODEC_AV1 },
                    new () { Label = "AV1 (10-Bit)", Value = CODEC_AV1_10BIT },
                };
            }
            return _CodecOptions;
        }
    }

    /// <summary>
    /// Gets or sets if hardware encoding should be used if possible
    /// </summary>
    [DefaultValue(true)]
    [Boolean(2)]
    [ConditionEquals(nameof(Codec), "/av1/", inverse: true)]
    public bool HardwareEncoding { get; set; }

    /// <summary>
    /// Gets or sets the quality of the video encode
    /// </summary>
    [Slider(3, inverse: true)]
    [Range(0, 51)]
    [DefaultValue(28)]
    public int Quality { get; set; }

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
        var stream = Model.VideoStreams.Where(x => x.Deleted == false).First();

        stream.EncodingParameters.Clear();        

        if (Codec == CODEC_H264)
            stream.EncodingParameters.AddRange(H264(args, false, Quality, HardwareEncoding));
        else if (Codec == CODEC_H265 || Codec == CODEC_H265_10BIT)
            stream.EncodingParameters.AddRange(H265(args, Codec == CODEC_H265_10BIT, Quality, HardwareEncoding));
        else if (Codec == CODEC_AV1 || Codec == CODEC_AV1_10BIT)
            stream.EncodingParameters.AddRange(AV1(args, Codec == CODEC_AV1_10BIT, Quality));
        else
        {
            args.Logger?.ILog("Unknown codec: " + Codec);
            return 2;
        }

        stream.ForcedChange = true;
        return 1;
    }

    internal static IEnumerable<string> GetEncodingParameters(NodeParameters args, string codec, int quality, bool useHardwareEncoder)
    {
        if (codec == CODEC_H264)
            return H264(args, false, quality, useHardwareEncoder).Select(x => x.Replace("{index}", "0")); 
        if (codec == CODEC_H265 || codec == CODEC_H265_10BIT)
            return H265(args, codec == CODEC_H265_10BIT, quality, useHardwareEncoder).Select(x => x.Replace("{index}", "0"));
        if(codec == CODEC_AV1)
            return AV1(args, codec == CODEC_AV1_10BIT, quality).Select(x => x.Replace("{index}", "0")); 
            
        throw new Exception("Unsupported codec: " + codec);

    }

    private static readonly bool IsMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    private static IEnumerable<string> H264(NodeParameters args, bool tenBit, int quality, bool useHardwareEncoding)
    {
        List<string> parameters = new List<string>();
        string[] bit10Filters = null;
        string[] non10BitFilters = null;
        if (useHardwareEncoding == false)
            parameters.AddRange(H26x_CPU(false, quality, out bit10Filters));
        else if(IsMac && CanUseHardwareEncoding.CanProcess_VideoToolbox_H264(args))
            parameters.AddRange(H26x_VideoToolbox(false, quality));
        else if (CanUseHardwareEncoding.CanProcess_Nvidia_H264(args))
            parameters.AddRange(H26x_Nvidia(false, quality, out non10BitFilters));
        else if (CanUseHardwareEncoding.CanProcess_Qsv_H264(args))
            parameters.AddRange(H26x_Qsv(false, quality));
        else if (CanUseHardwareEncoding.CanProcess_Amd_H264(args))
            parameters.AddRange(H26x_Amd(false, quality));
        else if (CanUseHardwareEncoding.CanProcess_Vaapi_H264(args))
            parameters.AddRange(H26x_Vaapi(false, quality));
        else
            parameters.AddRange(H26x_CPU(false, quality, out bit10Filters));

        if (tenBit)
            parameters.AddRange(bit10Filters ?? new string[] { "-pix_fmt:v:{index}", "p010le", "-profile:v:{index}", "main10" });
        return parameters;
    }
    
    private static IEnumerable<string> H265(NodeParameters args, bool tenBit, int quality, bool useHardwareEncoding)
    {
        // hevc_qsv -load_plugin hevc_hw -pix_fmt p010le -profile:v main10 -global_quality 21 -g 24 -look_ahead 1 -look_ahead_depth 60
        List<string> parameters = new List<string>();
        string[] bit10Filters = null;
        string[] non10BitFilters = null;
        if (useHardwareEncoding == false)
            parameters.AddRange(H26x_CPU(true, quality, out bit10Filters));
        else if (IsMac && CanUseHardwareEncoding.CanProcess_VideoToolbox_Hevc(args))
            parameters.AddRange(H26x_VideoToolbox(true, quality));
        else if (CanUseHardwareEncoding.CanProcess_Nvidia_Hevc(args))
            parameters.AddRange(H26x_Nvidia(true, quality, out non10BitFilters));
        else if (CanUseHardwareEncoding.CanProcess_Qsv_Hevc(args))
            parameters.AddRange(H26x_Qsv(true, quality));
        else if (CanUseHardwareEncoding.CanProcess_Amd_Hevc(args))
            parameters.AddRange(H26x_Amd(true, quality));
        else if (CanUseHardwareEncoding.CanProcess_Vaapi_Hevc(args))
            parameters.AddRange(H26x_Vaapi(true, quality));
        else
            parameters.AddRange(H26x_CPU(true, quality, out bit10Filters));

        if (tenBit)
            parameters.AddRange(bit10Filters ?? new string[] { "-pix_fmt:v:{index}", "p010le", "-profile:v:{index}", "main10" });
        else if(non10BitFilters?.Any() == true)
            parameters.AddRange(non10BitFilters);
        return parameters;
    }

    
    private static IEnumerable<string> AV1(NodeParameters args, bool tenBit, int quality)
    {
        // hevc_qsv -load_plugin hevc_hw -pix_fmt p010le -profile:v main10 -global_quality 21 -g 24 -look_ahead 1 -look_ahead_depth 60
        List<string> parameters = new List<string>();
        parameters.AddRange(AV1_CPU(quality));
        if (tenBit)
            parameters.AddRange(new [] { "-pix_fmt:v:{index}", "yuv420p10le" });
        return parameters;
    }

    private static IEnumerable<string> AV1_CPU(int quality)
    {
        return new []
        {
            //"libaom-av1",
            "libsvtav1",
            "-preset", "8",
            "-crf", quality.ToString()
        };
    }


    private static IEnumerable<string> H26x_CPU(bool h265, int quality, out string[] bit10Filters)
    {
        bit10Filters = new[]
        {
            "-pix_fmt:v:{index}", "yuv420p10le", "-profile:v:{index}", "main10"
        };
        return new []
        {
            h265 ? "libx265" : "libx264",
            "-preset", "slow",
            "-crf", quality.ToString()
        };
    }

    private static IEnumerable<string> H26x_Nvidia(bool h265, int quality, out string[] non10BitFilters)
    {
        if (h265 == false)
            non10BitFilters = new[] { "-pix_fmt:v:{index}", "yuv420p" };
        else
            non10BitFilters = null;

        return new []
        {
            h265 ? "hevc_nvenc" : "h264_nvenc",
            "-rc", "constqp",
            "-qp", quality.ToString(),
            //"-b:v", "0K", // this would do a two-pass... slower
            "-preset", "p6",
            // https://www.reddit.com/r/ffmpeg/comments/gg5szi/what_is_spatial_aq_and_temporal_aq_with_nvenc/
            "-spatial-aq", "1"
        };
    }

    private static IEnumerable<string> H26x_Qsv(bool h265, int quality)
    {
        //hevc_qsv -load_plugin hevc_hw -pix_fmt p010le -profile:v main10 -global_quality 21 -g 24 -look_ahead 1 -look_ahead_depth 60
        var parameters = new List<string>();
        if (h265) 
        {
            parameters.AddRange(new[]
            {
                "hevc_qsv",
                "-load_plugin", "hevc_hw"
            });
        }
        else
        {
            parameters.AddRange(new[]
            {
                "h264_qsv"
            });

        }
        parameters.AddRange(new[]
        {
            "-global_quality", quality.ToString(),
            "-preset", "slower",
        });
        return parameters;
    }

    private static IEnumerable<string> H26x_Amd(bool h265, int quality)
    {
        return new[]
        {
            h265 ? "hevc_amf" : "h264_amf",
            "-qp", quality.ToString(),
            //"-b:v", "0K", // this would do a two-pass... slower
            "-preset", "slower",
            // https://www.reddit.com/r/ffmpeg/comments/gg5szi/what_is_spatial_aq_and_temporal_aq_with_nvenc/
            "-spatial-aq", "1"
        };
    }
    private static IEnumerable<string> H26x_Vaapi(bool h265, int quality)
    {
        return new[]
        {
            h265 ? "hevc_vaapi" : "h264_vaapi",
            "-qp", quality.ToString(),
            //"-b:v", "0K", // this would do a two-pass... slower
            "-preset", "slower",
            // https://www.reddit.com/r/ffmpeg/comments/gg5szi/what_is_spatial_aq_and_temporal_aq_with_nvenc/
            "-spatial-aq", "1"
        };
    }
    private static IEnumerable<string> H26x_VideoToolbox(bool h265, int quality)
    {
        return new[]
        {
            h265 ? "hevc_videotoolbox" : "h264_videotoolbox",
            "-qp", quality.ToString(),
            //"-b:v", "0K", // this would do a two-pass... slower
            "-preset", "slower",
            //"-spatial-aq", "1"
        };
    }
}
