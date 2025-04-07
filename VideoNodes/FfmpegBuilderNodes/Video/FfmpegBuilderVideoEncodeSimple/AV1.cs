using System;
using System.Collections.Generic;
using System.Globalization;
using FileFlows.VideoNodes.Helpers;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Provides methods for generating FFmpeg command parameters for various video encoders.
/// </summary>
public partial class FfmpegBuilderVideoEncodeSimple
{
    /// <summary>
    /// Gets the encoding parameters for AV1 CPU encoding.
    /// </summary>
    internal static string[] AV1_CPU(int quality, int speed)
    {
        string preset = speed switch
        {
            1 => "2",
            2 => "4",
            3 => "6",
            4 => "8",
            5 => "10",
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
    internal static string[] AV1_Amd(int quality, int speed)
    {
        return
        [
            "av1_amf",
            "-qp", MapQuality(quality).ToString(),
            "-preset", speed switch
            {
                1 => "high_quality",
                2 => "quality",
                3 => "balanced",
                4 => "balanced",
                5 => "speed",
                _ => "balanced"
            }
        ];
    }

    /// <summary>
    /// Gets the encoding parameters for AV1 NVIDIA encoding.
    /// </summary>
    internal static string[] AV1_Nvidia(int quality, int speed)
    {
        return
        [
            "av1_nvenc",
            "-rc", "constqp",
            "-qp", MapQuality(quality).ToString(),
            "-preset", speed switch
            {
                1 => "p7",
                2 => "p5",
                3 => "p4",
                4 => "p3",
                5 => "p1",
                _ => "p4"
            },
            "-spatial-aq", "1"
        ];
    }

    /// <summary>
    /// Gets the encoding parameters for AV1 QSV encoding.
    /// </summary>
    internal static string[] AV1_Qsv(string device, int quality, int speed)
    {
        var parameters = new List<string>
        {
            "av1_qsv",
            "-global_quality:v", MapQuality(quality).ToString(),
            "-preset", speed switch
            {
                1 => "1",
                2 => "3",
                3 => "4",
                4 => "5",
                5 => "7",
                _ => "4"
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
