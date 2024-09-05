namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Base for the common Video Encode flow elements
/// </summary>
public abstract class VideoEncodeBase : FfmpegBuilderNode
{
    
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

}