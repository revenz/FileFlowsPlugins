namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public partial class FfmpegBuilderVideoEncode
{
    /// <summary>
    /// Gets FFmpeg arguments for encoding VP9 using the CPU
    /// </summary>
    /// <param name="quality">the quality of the video</param>
    /// <param name="speed">the encoding speed</param>
    /// <returns>the FFmpeg arguments</returns>
    private static IEnumerable<string> VP9_CPU(int quality, string speed)
    {
        return new []
        {
            "libvpx-vp9",
            "-preset", speed?.EmptyAsNull() ?? "slow",
            "-crf", quality.ToString()
        };
    }

    /// <summary>
    /// Gets FFmpeg arguments for encoding VP9 using the Intel's QSV hardware encoder 
    /// </summary>
    /// <param name="quality">the quality of the video</param>
    /// <param name="speed">the encoding speed</param>
    /// <returns>the FFmpeg arguments</returns>
    private static IEnumerable<string> VP9_Qsv(int quality, string speed)
    {
        var parameters = new List<string>();
        parameters.AddRange(new[]
        {
            "vp9_qsv",
            "-global_quality", quality.ToString(),
            "-preset",  speed?.EmptyAsNull() ?? "slower",
        });
        return parameters;
    }
}