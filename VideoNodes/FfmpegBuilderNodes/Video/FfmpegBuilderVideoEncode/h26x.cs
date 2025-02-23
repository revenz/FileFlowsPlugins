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
    /// <param name="h265">True for H.265 (HEVC), false for H.264.</param>
    /// <param name="quality">The CRF quality value (1-51, lower is better).</param>
    /// <param name="speed">The encoding speed preset.</param>
    /// <param name="bit10Filters">Filters for 10-bit encoding.</param>
    private static IEnumerable<string> H26x_CPU(bool h265, int quality, string speed, out string[] bit10Filters)
    {
        bit10Filters = new[]
        {
            "-pix_fmt:v:{index}", "yuv420p10le", "-profile:v:{index}", "main10"
        };
        return new[]
        {
            h265 ? "libx265" : "libx264",
            "-preset", speed?.EmptyAsNull() ?? "slow",
            "-crf", quality.ToString()
        };
    }

    /// <summary>
    /// Gets the encoding parameters for NVIDIA GPU-based H.264/H.265 encoding.
    /// </summary>
    private static IEnumerable<string> H26x_Nvidia(bool h265, int quality, string speed, out string[] non10BitFilters)
    {
        non10BitFilters = h265 ? null : new[] { "-pix_fmt:v:{index}", "yuv420p" };
        return new[]
        {
            h265 ? "hevc_nvenc" : "h264_nvenc",
            "-rc", "constqp",
            "-qp", quality.ToString(),
            "-preset", GetSpeed(speed, nvidia: true),
            "-spatial-aq", "1"
        };
    }

    /// <summary>
    /// Gets the encoding parameters for Intel QSV-based H.264/H.265 encoding.
    /// </summary>
    private static IEnumerable<string> H26x_Qsv(bool h265, int quality, float fps, string speed)
    {
        var parameters = new List<string>
        {
            h265 ? "hevc_qsv" : "h264_qsv"
        };
        
        if (h265)
        {
            parameters.AddRange(new[] { "-load_plugin", "hevc_hw" });
        }
        
        if (fps > 0)
        {
            parameters.AddRange(["-r", fps.ToString(CultureInfo.InvariantCulture)]);
            parameters.AddRange(["-g", ((int)Math.Round(fps * 5)).ToString(CultureInfo.InvariantCulture)]);
        }
        
        parameters.AddRange(new[]
        {
            "-global_quality:v", quality.ToString(),
            "-preset", speed?.EmptyAsNull() ?? "slower"
        });
        return parameters;
    }

    /// <summary>
    /// Gets the encoding parameters for AMD GPU-based H.264/H.265 encoding.
    /// </summary>
    private static IEnumerable<string> H26x_Amd(bool h265, int quality, string speed, out string[] bit10Filters)
    {
        bit10Filters = new[]
        {
            "-pix_fmt:v:{index}", "p010le", "-profile:v:{index}", "1"
        };
        string preset = speed switch
        {
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
        
        return new[]
        {
            h265 ? "hevc_amf" : "h264_amf",
            "-qp", quality.ToString(),
            "-preset", preset,
            "-spatial-aq", "1"
        };
    }

    /// <summary>
    /// Gets the encoding parameters for VAAPI-based H.264/H.265 encoding.
    /// </summary>
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

        // Adjusted scale to align with x264/x265/NVENC/AMF/QSV behavior
        return (int)Math.Round(80 - (crf - 1) * (25.0 / 50.0)); 
    }

}
