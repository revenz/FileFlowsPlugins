using System;
using System.Collections.Generic;
using System.Globalization;
using FileFlows.VideoNodes.Helpers;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Provides methods for generating FFmpeg command parameters for various video encoders.
/// </summary>
public partial class FfmpegBuilderVideoEncode
{
    /// <summary>
    /// Gets the encoding parameters for AV1 CPU encoding.
    /// </summary>
    internal static string[] AV1_CPU(int quality, string speed)
    {
        string preset = speed switch
        {
            "ultrafast" => "13",
            "superfast" => "11",
            "veryfast" => "10",
            "faster" => "9",
            "fast" => "8",
            "medium" => "6",
            "slow" => "4",
            "slower" => "2",
            "veryslow" => "1",
            "1" => "2",
            "2" => "4",
            "3" => "6",
            "4" => "8",
            "5" => "10",
            _ => "4"
        };
        
        return
        [
            "libaom-av1",
            "-crf", MapQuality(quality).ToString(),
            "-cpu-used", preset
        ];
    }

    /// <summary>
    /// Gets the encoding parameters for AV1 AMD encoding.
    /// </summary>
    internal static string[] AV1_Amd(int quality, string speed)
    {
        return
        [
            "av1_amf",
            "-qp", MapQuality(quality).ToString(),
            "-preset", speed switch
            {
                "veryslow" => "veryslow",
                "slower" => "slower",
                "slow" => "slow",
                "medium" => "balanced",
                "fast" or "faster" or "veryfast" or "superfast" or "ultrafast" => "speed",
                "1" => "veryslow",
                "2" => "slower",
                "3" => "slow",
                "4" => "balanced",
                "5" => "speed",
                _ => "slower"
            },
            "-spatial-aq", "1"
        ];
    }

    /// <summary>
    /// Gets the encoding parameters for AV1 NVIDIA encoding.
    /// </summary>
    internal static string[] AV1_Nvidia(int quality, string speed)
    {
        return
        [
            "av1_nvenc",
            "-rc", "constqp",
            "-qp", MapQuality(quality).ToString(),
            "-preset", speed switch
            {
                "veryslow" => "p7",
                "slower" => "p6",
                "slow" => "p5",
                "medium" => "p4",
                "fast" or "faster" or "veryfast" or "superfast" or "ultrafast" => "p1",
                "1" => "p7",
                "2" => "p6",
                "3" => "p5",
                "4" => "p4",
                "5" => "p1",
                _ => "p4"
            },
            "-spatial-aq", "1"
        ];
    }

    /// <summary>
    /// Gets the encoding parameters for AV1 QSV encoding.
    /// </summary>
    internal static string[] AV1_Qsv(string device, int quality, string speed)
    {
        var parameters = new List<string>
        {
            "av1_qsv",
            "-global_quality:v", MapQuality(quality).ToString(),
            "-preset", speed switch
            {
                "veryslow" => "1",
                "slower" => "2",
                "slow" => "3",
                "medium" => "4",
                "fast" or "faster" or "veryfast" or "superfast" or "ultrafast" => "7",
                "1" => "1",
                "2" => "2",
                "3" => "3",
                "4" => "4",
                "5" => "7",
                _ => "3"
            }
        };
        if (VaapiHelper.VaapiLinux)
        {
            if (string.IsNullOrEmpty(device))
                parameters.AddRange(new[] { "-qsv_device", VaapiHelper.VaapiRenderDevice });
            else if (device != "NONE")
                parameters.AddRange(new[] { "-qsv_device", device });
        }

        return parameters.ToArray();
    }
}
