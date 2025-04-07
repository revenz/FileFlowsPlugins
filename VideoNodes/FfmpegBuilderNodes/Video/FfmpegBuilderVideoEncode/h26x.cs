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

    private static IEnumerable<string> H26x_Amd(bool h265, int quality, string speed, out string[] bit10Filters)
    {
        bit10Filters =
        [
            "-pix_fmt:v:{index}", "p010le", "-profile:v:{index}", "1" // 1 is main
        ];
        string preset = "6"; // Default to "medium" (6) if speed is null or invalid

        switch (speed)
        {
            case "ultrafast": preset = "0"; break;
            case "superfast": preset = "1"; break;
            case "veryfast":  preset = "2"; break;
            case "faster": preset = "3"; break;
            case "fast": preset = "4"; break;
            case "medium": preset = "6"; break;
            case "slow": preset = "8"; break;
            case "slower": preset = "9"; break;
            case "veryslow": preset = "10"; break;
        }

        return new[]
        {
            h265 ? "hevc_amf" : "h264_amf",
            "-qp", quality.ToString(),
            "-preset", preset
        };
    }

    private static IEnumerable<string> H26x_Vaapi(bool h265, int quality, string speed)
    {
        return
        [
            h265 ? "hevc_vaapi" : "h264_vaapi",
            "-qp", quality.ToString(),
            "-preset", speed?.EmptyAsNull() ?? "slower"
        ];
    }

    /// <summary>
    /// Generates encoding parameters for VideoToolbox (macOS hardware acceleration).
    /// Maps a CRF-style quality value (1-51) to VideoToolbox's scale (50-80) for consistency across encoders.
    /// </summary>
    /// <param name="h265">True for HEVC, false for H.264.</param>
    /// <param name="quality">CRF-style quality value (1-51).</param>
    /// <param name="speed">Encoding speed preset.</param>
    /// <returns>List of FFmpeg parameters for VideoToolbox encoding.</returns>
    private static IEnumerable<string> H26x_VideoToolbox(bool h265, int quality, string speed)
    {
        // Map quality (1-51 CRF style) to VideoToolbox's Q scale (50-80)
        int q = MapQualityToVideoToolbox(quality);
        
        return new[]
        {
            h265 ? "hevc_videotoolbox" : "h264_videotoolbox",
            "-q", q.ToString(),
            "-preset", speed?.EmptyAsNull() ?? "slower"
        };
    }

    /// <summary>
    /// Maps a CRF-style quality value (1-51) to VideoToolbox's Q scale (50-80).
    /// </summary>
    /// <param name="crf">CRF-style quality value (1-51).</param>
    /// <returns>Equivalent VideoToolbox quality value (50-80).</returns>
    private static int MapQualityToVideoToolbox(int crf)
    {
        // Enforce CRF bounds
        crf = Math.Clamp(crf, 1, 51);

        // More aggressive mapping to VideoToolbox -q scale
        return (int)Math.Round(82 - (crf - 1) * (32.0 / 50.0));
    }
}