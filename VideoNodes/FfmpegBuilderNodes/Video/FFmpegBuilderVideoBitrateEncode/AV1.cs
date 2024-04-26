using FileFlows.VideoNodes.Helpers;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public partial class FfmpegBuilderVideoBitrateEncode
{
    /// <summary>
    /// AV1 CPU encoding
    /// </summary>
    /// <param name="bitrate">the bitrate in Kbps</param>
    /// <returns>the encoding parameters</returns>
    private static IEnumerable<string> AV1_CPU(int bitrate)
    {
        return new []
        {
            //"libaom-av1",
            "libsvtav1",
            "-b:v:{index}", bitrate + "k",
            "-minrate", bitrate + "k",
            "-maxrate", bitrate + "k",
            "-bufsize", (bitrate * 2) + "k"
        };
    }
    
    /// <summary>
    /// AV1 AMD encoding
    /// </summary>
    /// <param name="bitrate">the bitrate in Kbps</param>
    /// <returns>the encoding parameters</returns>
    private static IEnumerable<string> AV1_Amd(int bitrate)
    {
        return new[]
        {
            "av1_amf",
            "-b:v:{index}", bitrate + "k",
            "-minrate", bitrate + "k",
            "-maxrate", bitrate + "k",
            "-bufsize", (bitrate * 2) + "k"
        };
    }
    
    /// <summary>
    /// AV1 NVIDIA encoding
    /// </summary>
    /// <param name="bitrate">the bitrate in Kbps</param>
    /// <returns>the encoding parameters</returns>
    private static IEnumerable<string> AV1_Nvidia(int bitrate)
    {
        return new []
        {
            "av1_nvenc",
            "-b:v:{index}", bitrate + "k",
            "-minrate", bitrate + "k",
            "-maxrate", bitrate + "k",
            "-bufsize", (bitrate * 2) + "k"
        };
    }
    
    /// <summary>
    /// AV1 QSV encoding
    /// </summary>
    /// <param name="bitrate">the bitrate in Kbps</param>
    /// <returns>the encoding parameters</returns>
    private static IEnumerable<string> AV1_Qsv(int bitrate)
    {
        var args = new List<string>
        {
            "av1_qsv",
            "-b:v:{index}", bitrate + "k",
            "-minrate", bitrate + "k",
            "-maxrate", bitrate + "k",
            "-bufsize", (bitrate * 2) + "k"
        };
        if(VaapiHelper.VaapiLinux)
            args.AddRange(new [] { "-qsv_device", VaapiHelper.VaapiRenderDevice});
        
        return args.ToArray();
    }
}