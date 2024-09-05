namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public partial class FfmpegBuilderVideoBitrateEncode
{
    /// <summary>
    /// Gets FFmpeg arguments for encoding VP9 using the CPU
    /// </summary>
    /// <param name="bitrate">the bitrate in Kbps</param>
    /// <returns>the FFmpeg arguments</returns>
    private static IEnumerable<string> VP9_CPU(int bitrate)
    {
        return new []
        {
            "libvpx-vp9",
            "-b:v:{index}", bitrate + "k",
            "-minrate", bitrate + "k",
            "-maxrate", bitrate + "k",
            "-bufsize", (bitrate * 2) + "k"
        };
    }

    /// <summary>
    /// Gets FFmpeg arguments for encoding VP9 using the Intel's QSV hardware encoder 
    /// </summary>
    /// <param name="bitrate">the bitrate in Kbps</param>
    /// <returns>the FFmpeg arguments</returns>
    private static IEnumerable<string> VP9_Qsv(int bitrate)
    {
        var parameters = new List<string>();
        parameters.AddRange(new[]
        {
            "vp9_qsv",
            "-b:v:{index}", bitrate + "k",
            "-minrate", bitrate + "k",
            "-maxrate", bitrate + "k",
            "-bufsize", (bitrate * 2) + "k"
        });
        return parameters;
    }
}