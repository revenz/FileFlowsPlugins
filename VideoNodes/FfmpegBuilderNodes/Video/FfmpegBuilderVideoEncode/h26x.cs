using System.Globalization;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public partial class FfmpegBuilderVideoEncode
{
    
    private static IEnumerable<string> H26x_CPU(bool h265, int quality, string speed, out string[] bit10Filters)
    {
        bit10Filters = new[]
        {
            "-pix_fmt:v:{index}", "yuv420p10le", "-profile:v:{index}", "main10"
        };
        return new []
        {
            h265 ? "libx265" : "libx264",
            "-preset", speed?.EmptyAsNull() ?? "slow",
            "-crf", quality.ToString()
        };
    }

    private static IEnumerable<string> H26x_Nvidia(bool h265, int quality, string speed, out string[] non10BitFilters)
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
            "-preset", GetSpeed(speed, nvidia:true),
            "-spatial-aq", "1"
        };
    }

    private static IEnumerable<string> H26x_Qsv(bool h265, int quality, float fps, string speed)
    {
        //hevc_qsv -load_plugin hevc_hw -pix_fmt p010le -profile:v main10 -global_quality 21 -g 24 -look_ahead 1 -look_ahead_depth 60
        var parameters = new List<string>();
        if (h265) 
        {
            parameters.AddRange(new[]
            {
                "hevc_qsv",
                "-load_plugin", "hevc_hw",
                // -g is gop/keyframe not framerate
                //"-g",  (fps < 1 ? 29.97 : fps).ToString(CultureInfo.InvariantCulture)
            });

            if (fps > 0)
            {
                parameters.AddRange(["-r", fps.ToString(CultureInfo.InvariantCulture)]);
                parameters.AddRange(["-g", ((int)Math.Round(fps * 5)).ToString(CultureInfo.InvariantCulture)]);
            }

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
            "-global_quality:v", quality.ToString(),
            "-preset", speed?.EmptyAsNull() ?? "slower",
        });
        return parameters;
    }

    private static IEnumerable<string> H26x_Amd(bool h265, int quality, string speed)
    {
        return new[]
        {
            h265 ? "hevc_amf" : "h264_amf",
            "-qp", quality.ToString(),
            "-preset", speed?.EmptyAsNull() ?? "slower",
            "-spatial-aq", "1"
        };
    }
    private static IEnumerable<string> H26x_Vaapi(bool h265, int quality, string speed)
    {
        return new[]
        {
            h265 ? "hevc_vaapi" : "h264_vaapi",
            "-qp", quality.ToString(),
            "-preset", speed?.EmptyAsNull() ?? "slower",
            "-spatial-aq", "1"
        };
    }
    private static IEnumerable<string> H26x_VideoToolbox(bool h265, int quality, string speed)
    {
        return new[]
        {
            h265 ? "hevc_videotoolbox" : "h264_videotoolbox",
            "-qp", quality.ToString(),
            "-preset", speed?.EmptyAsNull() ?? "slower"
        };
    }
}