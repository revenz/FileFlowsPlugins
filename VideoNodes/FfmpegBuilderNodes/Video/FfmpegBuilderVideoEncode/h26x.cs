using System;
using System.Collections.Generic;
using System.Globalization;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Provides methods for generating FFmpeg command parameters for various video encoders.
/// </summary>
public partial class FfmpegBuilderVideoEncode
{
    /// <summary>
    /// Gets the encoding parameters for CPU-based H.264/H.265 encoding.
    /// </summary>
    internal static string[] H26x_CPU(bool h265, int quality, string speed, out string[] bit10Filters)
    {
        bit10Filters = ["-pix_fmt:v:{index}", "yuv420p10le", "-profile:v:{index}", "main10"];
        return
        [
            h265 ? "libx265" : "libx264",
            "-preset", MapSpeed(speed, "slow"),
            "-crf", MapQuality(quality).ToString()
        ];
    }

    /// <summary>
    /// Gets the encoding parameters for NVIDIA GPU-based H.264/H.265 encoding.
    /// </summary>
    internal static string[] H26x_Nvidia(bool h265, int quality, string speed, out string[] non10BitFilters)
    {
        non10BitFilters = h265 ? null : ["-pix_fmt:v:{index}", "yuv420p"];
        return
        [
            h265 ? "hevc_nvenc" : "h264_nvenc",
            "-rc", "constqp",
            "-qp", MapQuality(quality).ToString(),
            "-preset", MapSpeed(speed, "medium"),
            "-spatial-aq", "1"
        ];
    }

    /// <summary>
    /// Gets the encoding parameters for Intel QSV-based H.264/H.265 encoding.
    /// </summary>
    internal static string[] H26x_Qsv(bool h265, int quality, float fps, string speed)
    {
        var parameters = new List<string> { h265 ? "hevc_qsv" : "h264_qsv" };

        if (h265)
            parameters.AddRange(new[] { "-load_plugin", "hevc_hw" });

        if (fps > 0)
        {
            parameters.AddRange(["-r", fps.ToString(CultureInfo.InvariantCulture)]);
            parameters.AddRange(["-g", ((int)Math.Round(fps * 5)).ToString(CultureInfo.InvariantCulture)]);
        }

        parameters.AddRange(new[]
        {
            "-global_quality:v", MapQuality(quality).ToString(),
            "-preset", MapSpeed(speed, "slower")
        });
        return parameters.ToArray();
    }

    /// <summary>
    /// Gets the encoding parameters for AMD GPU-based H.264/H.265 encoding.
    /// </summary>
    internal static string[] H26x_Amd(bool h265, int quality, string speed, out string[] bit10Filters)
    {
        bit10Filters = ["-pix_fmt:v:{index}", "p010le", "-profile:v:{index}", "1"];
        return
        [
            h265 ? "hevc_amf" : "h264_amf",
            "-qp", MapQuality(quality).ToString(),
            "-preset", MapSpeedAmd(speed),
            "-spatial-aq", "1"
        ];
    }

    /// <summary>
    /// Gets the encoding parameters for VAAPI-based H.264/H.265 encoding.
    /// </summary>
    internal static string[] H26x_Vaapi(bool h265, int quality, string speed)
    {
        return
        [
            h265 ? "hevc_vaapi" : "h264_vaapi",
            "-qp", MapQuality(quality).ToString(),
            "-preset", MapSpeed(speed, "slower"),
            "-spatial-aq", "1"
        ];
    }

    /// <summary>
    /// Generates encoding parameters for VideoToolbox (macOS hardware acceleration).
    /// </summary>
    internal static string[] H26x_VideoToolbox(bool h265, int quality, string speed)
    {
        int q = MapQualityToVideoToolbox(MapQuality(quality));
        return
        [
            h265 ? "hevc_videotoolbox" : "h264_videotoolbox",
            "-q", q.ToString(),
            "-preset", MapSpeed(speed, "slower")
        ];
    }

    /// <summary>
    /// Maps a CRF-style quality value (1-51) to VideoToolbox's Q scale (50-80).
    /// </summary>
    private static int MapQualityToVideoToolbox(int crf)
    {
        crf = Math.Clamp(crf, 1, 51);
        return (int)Math.Round(80 - (crf - 15) * (30.0 / 15.0));
    }

    /// <summary>
    /// Maps speed presets (1-5) or named values to the appropriate FFmpeg preset.
    /// </summary>
    private static string MapSpeed(string speed, string defaultValue)
    {
        return speed switch
        {
            "1" => "veryslow",
            "2" => "slow",
            "3" => "medium",
            "4" => "fast",
            "5" => "ultrafast",
            "ultrafast" or "superfast" => "ultrafast",
            "veryfast" => "veryfast",
            "faster" => "faster",
            "fast" => "fast",
            "medium" => "medium",
            "slow" => "slow",
            "slower" => "slower",
            "veryslow" => "veryslow",
            _ => defaultValue
        };
    }

    /// <summary>
    /// Maps AMD speed presets (1-5) or named values to AMD-specific values.
    /// </summary>
    private static string MapSpeedAmd(string speed)
    {
        return speed switch
        {
            "1" => "10",
            "2" => "8",
            "3" => "6",
            "4" => "4",
            "5" => "0",
            "ultrafast" => "0",
            "superfast" => "1",
            "veryfast" => "2",
            "faster" => "3",
            "fast" => "4",
            "medium" => "6",
            "slow" => "8",
            "slower" => "9",
            "veryslow" => "10",
            _ => "6"
        };
    }

    /// <summary>
    /// Maps a 1-10 quality scale to a 1-51 CRF-style quality value.
    /// </summary>
    internal static int MapQuality(int quality)
    {
        quality = Math.Clamp(quality, 1, 10);
        return (int)Math.Round(30 - (quality - 1) * (15.0 / 9.0));
    }
}
