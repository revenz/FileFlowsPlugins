using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

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
    internal const string CODEC_H264_10BIT = "h264 10BIT";
    internal const string CODEC_H265 = "h265";
    internal const string CODEC_H265_10BIT = "h265 10BIT";

    /// <summary>
    /// The Help URL for this node
    /// </summary>
    public override string HelpUrl => "https://github.com/revenz/FileFlows/wiki/FFMPEG-Builder:-Video-Encode";

    /// <summary>
    /// Gets or sets the codec used to encode
    /// </summary>
    [DefaultValue(CODEC_H264_10BIT)]
    [ChangeValue(nameof(Quality), 23, CODEC_H264)]
    [ChangeValue(nameof(Quality), 23, CODEC_H265_10BIT)]
    [ChangeValue(nameof(Quality), 28, CODEC_H265)]
    [ChangeValue(nameof(Quality), 28, CODEC_H265_10BIT)]
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
                    new () { Label = "H.264 (10-Bit)", Value = CODEC_H264_10BIT },
                    new () { Label = "H.265", Value = CODEC_H265 },
                    new () { Label = "H.265 (10-Bit)", Value = CODEC_H265_10BIT },
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
    public bool HardwareEncoding { get; set; }

    /// <summary>
    /// Gets or sets the quality of the video encode
    /// </summary>
    [Slider(3, inverse: true)]
    [Range(0, 51)]
    [DefaultValue(28)]
    public int Quality { get; set; }

    private string bit10Filter = "yuv420p10le";

    /// <summary>
    /// Executes the node
    /// </summary>
    /// <param name="args">The node arguments</param>
    /// <returns>the output return</returns>
    public override int Execute(NodeParameters args)
    {
        var stream = Model.VideoStreams.Where(x => x.Deleted == false).First();
        if (Codec == CODEC_H264 || Codec == CODEC_H264_10BIT)
            H264(stream, Codec == CODEC_H264_10BIT);
        else if (Codec == CODEC_H265 || Codec == CODEC_H265_10BIT)
            H265(stream, Codec == CODEC_H265_10BIT);
        else
        {
            args.Logger?.ILog("Unknown codec: " + Codec);
            return 2;
        }

        stream.ForcedChange = true;
        return 1;
    }

    private void H264(FfmpegVideoStream stream, bool tenBit)
    {
        if (HardwareEncoding == false)
            H26x_CPU(stream);
        else if (CanUseHardwareEncoding.CanProcess_Nvidia_H264(Args))
            H26x_Nvidia(stream, false);
        else if (CanUseHardwareEncoding.CanProcess_Qsv_H264(Args))
            H26x_Qsv(stream, false);
        else if (CanUseHardwareEncoding.CanProcess_Amd_H264(Args))
            H26x_Amd(stream, false);
        else if (CanUseHardwareEncoding.CanProcess_Vaapi_H264(Args))
            H26x_Vaapi(stream, false);
        else 
            H26x_CPU(stream);

        if (tenBit)
            stream.EncodingParameters.AddRange(new[] { "-pix_fmt:v:{index}", bit10Filter });
    }
    
    private void H265(FfmpegVideoStream stream, bool tenBit)
    {
        // hevc_qsv -load_plugin hevc_hw -pix_fmt p010le -profile:v main10 -global_quality 21 -g 24 -look_ahead 1 -look_ahead_depth 60
        if (HardwareEncoding == false)
            H26x_CPU(stream);
        else if (CanUseHardwareEncoding.CanProcess_Nvidia_Hevc(Args))
            H26x_Nvidia(stream, true);
        else if (CanUseHardwareEncoding.CanProcess_Qsv_Hevc(Args))
            H26x_Qsv(stream, true);
        else if (CanUseHardwareEncoding.CanProcess_Amd_Hevc(Args))
            H26x_Amd(stream, true);
        else if (CanUseHardwareEncoding.CanProcess_Vaapi_Hevc(Args))
            H26x_Vaapi(stream, true);
        else 
            H26x_CPU(stream);

        if (tenBit)
            stream.EncodingParameters.AddRange(new[] { "-pix_fmt:v:{index}", bit10Filter });
    }


    private void H26x_CPU(FfmpegVideoStream stream)
    {
        stream.EncodingParameters.Clear();
        stream.EncodingParameters.AddRange(new []
        {
            Codec == CODEC_H265 ? "libx265" : "libx264",
            "-preset", "slow",
            "-crf", Quality.ToString()
        });
    }

    private void H26x_Nvidia(FfmpegVideoStream stream, bool h265)
    {
        stream.EncodingParameters.Clear();
        stream.EncodingParameters.AddRange(new []
        {
            h265 ? "hevc_nvenc" : "h264_nvenc",
            "-rc", "constqp",
            "-qp", Quality.ToString(),
            //"-b:v", "0K", // this would do a two-pass... slower
            "-preset", "p6",
            // https://www.reddit.com/r/ffmpeg/comments/gg5szi/what_is_spatial_aq_and_temporal_aq_with_nvenc/
            "-spatial-aq", "1"
        });

        if (Codec == CODEC_H264_10BIT)
            bit10Filter = "yuv420p";
    }

    private void H26x_Qsv(FfmpegVideoStream stream, bool h265)
    {
        //hevc_qsv -load_plugin hevc_hw -pix_fmt p010le -profile:v main10 -global_quality 21 -g 24 -look_ahead 1 -look_ahead_depth 60
        stream.EncodingParameters.Clear();
        if (h265) 
        {
            stream.EncodingParameters.AddRange(new[]
            {
                "hevc_qsv",
                "-load_plugin", "hevc_hw"
            });
        }
        else
        {
            stream.EncodingParameters.AddRange(new[]
            {
                "h264_qsv"
            });

        }
        stream.EncodingParameters.AddRange(new[]
        {
            "-qp", Quality.ToString(),
            "-preset", "slower",
        });
    }

    private void H26x_Amd(FfmpegVideoStream stream, bool h265)
    {
        stream.EncodingParameters.Clear();
        stream.EncodingParameters.AddRange(new[]
        {
            h265 ? "amf_nvenc" : "amf_nvenc",
            "-rc", "constqp",
            "-qp", Quality.ToString(),
            //"-b:v", "0K", // this would do a two-pass... slower
            "-preset", "slower",
            // https://www.reddit.com/r/ffmpeg/comments/gg5szi/what_is_spatial_aq_and_temporal_aq_with_nvenc/
            "-spatial-aq", "1"
        });

        if (Codec == CODEC_H264_10BIT)
            bit10Filter = "yuv420p";
    }
    private void H26x_Vaapi(FfmpegVideoStream stream, bool h265)
    {
        stream.EncodingParameters.Clear();
        stream.EncodingParameters.AddRange(new[]
        {
            h265 ? "amf_nvenc" : "amf_nvenc",
            "-rc", "constqp",
            "-qp", Quality.ToString(),
            //"-b:v", "0K", // this would do a two-pass... slower
            "-preset", "slower",
            // https://www.reddit.com/r/ffmpeg/comments/gg5szi/what_is_spatial_aq_and_temporal_aq_with_nvenc/
            "-spatial-aq", "1"
        });

        if (Codec == CODEC_H264_10BIT)
            bit10Filter = "yuv420p";
    }
}
