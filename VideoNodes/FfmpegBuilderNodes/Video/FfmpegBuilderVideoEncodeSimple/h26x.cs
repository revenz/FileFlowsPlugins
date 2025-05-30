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
            "-crf", MapQuality(quality, h265 == false).ToString()
        ];
    }

    /// <summary>
    /// Gets the encoding parameters for NVIDIA GPU-based H.264/H.265 encoding.
    /// </summary>
    internal static string[] H26x_Nvidia(bool h265, int quality, int speed, out string[] non10BitFilters)
    {
        non10BitFilters = h265 ? null : ["-pix_fmt:v:{index}", "yuv420p"];
        var speedStr = speed switch
        {
            1 => "p7",
            2 => "p5",
            3 => "p3",
            4 => "p2",
            5 => "p1",
            _ => "p3"
        };

        return
        [
            h265 ? "hevc_nvenc" : "h264_nvenc",
            "-rc", "constqp",
            "-qp", MapQuality(quality, h265 == false).ToString(),
            "-preset", speedStr,
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
            "-global_quality:v", MapQuality(quality, h265 == false).ToString(),
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
            "-qp", MapQuality(quality, h265 == false).ToString(),
            "-preset", MapSpeedAmd(speed)
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
            "-qp", MapQuality(quality, h265 == false).ToString(),
            "-preset", MapSpeed(speed, "slower")
        ];
    }

    /// <summary>
    /// Generates encoding parameters for VideoToolbox (macOS hardware acceleration).
    /// </summary>
    internal static string[] H26x_VideoToolbox(bool h265, int quality, int speed)
    {
        return
        [
            h265 ? "hevc_videotoolbox" : "h264_videotoolbox",
            "-q", MapQualityVideoToolbox(quality).ToString(),
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
    internal static int MapQuality(int quality, bool isH264 = false)
    {
        // Define CRF ranges
        int minH265 = 18, maxH265 = 30;
        int minH264 = 18, maxH264 = 25;  // Slightly tighter for H.264

        int min = isH264 ? minH264 : minH265;
        int max = isH264 ? maxH264 : maxH265;

        quality = Math.Clamp(quality, minQuality, maxQuality);

        // Linear interpolation
        return (int)Math.Round(max - ((quality - minQuality) / (double)(maxQuality - minQuality)) * (max - min));
    }
    /// <summary>
    /// Maps a 1-9 quality scale to VideoToolbox's quality range (60-80).
    /// </summary>
    internal static int MapQualityVideoToolbox(int quality)
    {
        int minVTB = 60, maxVTB = 80; // VideoToolbox quality range

        quality = Math.Clamp(quality, minQuality, maxQuality);

        return (int)Math.Round(minVTB + ((quality - minQuality) / 8.0) * (maxVTB - minVTB));
    }



}
