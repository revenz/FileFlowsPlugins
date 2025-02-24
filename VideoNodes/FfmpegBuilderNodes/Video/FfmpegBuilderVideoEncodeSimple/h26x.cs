using System;
using System.Collections.Generic;
using System.Globalization;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Provides methods for generating FFmpeg command parameters for various video encoders.
/// </summary>
public partial class FfmpegBuilderVideoEncodeSimple
{
    /// <summary>
    /// Gets the encoding parameters for CPU-based H.264/H.265 encoding.
    /// </summary>
    internal static string[] H26x_CPU(bool h265, int quality, int speed, out string[] bit10Filters)
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
    internal static string[] H26x_Nvidia(bool h265, int quality, int speed, out string[] non10BitFilters)
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
    internal static string[] H26x_Qsv(bool h265, int quality, float fps, int speed)
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
    internal static string[] H26x_Amd(bool h265, int quality, int speed, out string[] bit10Filters)
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
    internal static string[] H26x_Vaapi(bool h265, int quality, int speed)
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
    internal static string[] H26x_VideoToolbox(bool h265, int quality, int speed)
    {
        int q = quality switch
        {
            1 => 60,  // Lowest quality, most compression (smallest file)
            2 => 62,
            3 => 64,
            4 => 66,
            5 => 68,  // Mid quality (~CRF 23-25)
            6 => 70,
            7 => 72,
            8 => 74,
            9 => 76,
            10 => 78, // Highest quality, least compression (largest file)
            _ => 68   // Default to mid-quality
        };

        return
        [
            h265 ? "hevc_videotoolbox" : "h264_videotoolbox",
            "-q", q.ToString(),
            "-preset", MapSpeed(speed, "slower")
        ];
    }

    /// <summary>
    /// Maps speed presets (1-5) or named values to the appropriate FFmpeg preset.
    /// </summary>
    private static string MapSpeed(int speed, string defaultValue)
    {
        return speed switch
        {
            1 => "veryslow",
            2 => "slow",
            3 => "medium",
            4 => "fast",
            5 => "ultrafast",
            _ => defaultValue
        };
    }

    /// <summary>
    /// Maps AMD speed presets (1-5) or named values to AMD-specific values.
    /// </summary>
    private static string MapSpeedAmd(int speed)
    {
        return speed switch
        {
            1 => "10",
            2 => "8",
            3 => "6",
            4 => "4",
            5 => "0",
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
