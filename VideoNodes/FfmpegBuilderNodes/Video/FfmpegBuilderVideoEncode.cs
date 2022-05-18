using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Set a video codec encoding for a video stream based on users settings
/// </summary>
public class FfmpegBuilderVideoEncode:FfmpegBuilderNode
{
    public override int Outputs => 1;

    private const string CODEC_H264 = "h264";
    private const string CODEC_H265 = "h265";

    public override string HelpUrl => "https://github.com/revenz/FileFlows/wiki/FFMPEG-Builder:-Video-Encode";

    [DefaultValue("h265")]
    [Select(nameof(CodecOptions), 1)]
    public string Codec { get; set; }

    private static List<ListOption> _CodecOptions;
    public static List<ListOption> CodecOptions
    {
        get
        {
            if (_CodecOptions == null)
            {
                _CodecOptions = new List<ListOption>
                {
                    new () { Label = "h264", Value = CODEC_H264 },
                    new () { Label = "h265", Value = CODEC_H265 }
                };
            }
            return _CodecOptions;
        }
    }

    [DefaultValue(true)]
    [Boolean(2)]
    public bool HardwareEncoding { get; set; }

    [Slider(3)]
    [Range(0, 51)]
    [DefaultValue(23)]
    public int Quality { get; set; }


    public override int Execute(NodeParameters args)
    {
        var stream = Model.VideoStreams.Where(x => x.Deleted == false).First();
        if (Codec == CODEC_H264)
            H264(stream);
        else if (Codec == CODEC_H264)
            H265(stream);
        bool encoding = false;
        return encoding ? 1 : 2;
    }

    private void H264(FfmpegVideoStream stream)
    {
        if (HardwareEncoding == false)
            H26x_CPU(stream);
        else if (SupportsHardwareNvidia264())
            H264_Nvidia(stream);
        else 
            H26x_CPU(stream);
    }
    
    private void H265(FfmpegVideoStream stream)
    {
        // hevc_qsv -load_plugin hevc_hw -pix_fmt p010le -profile:v main10 -global_quality 21 -g 24 -look_ahead 1 -look_ahead_depth 60
        if (HardwareEncoding == false)
            H26x_CPU(stream);
        else if (SupportsHardwareNvidia265())
            H265_Nvidia(stream);
        else 
            H26x_CPU(stream);
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

    private void H264_Nvidia(FfmpegVideoStream stream)
    {
        stream.EncodingParameters.Clear();
        stream.EncodingParameters.AddRange(new []
        {
            "h264_nvenc",
            "-rc", "vbr_hq",
            // 0 == auto, so we set to 1
            "-cq", Quality <= 0 ? "1" : Quality.ToString(),
        });
    }
    private void H265_Nvidia(FfmpegVideoStream stream)
    {
        stream.EncodingParameters.Clear();
        stream.EncodingParameters.AddRange(new []
        {
            "hevc_nvenc",
            "-rc", "constqp",
            "-qp", Quality.ToString(),
            //"-b:v", "0K", // this would do a two-pass... slower
            "-preset", "p6",
            // https://www.reddit.com/r/ffmpeg/comments/gg5szi/what_is_spatial_aq_and_temporal_aq_with_nvenc/
            "-spatial-aq", "1"
        });
    }

}
