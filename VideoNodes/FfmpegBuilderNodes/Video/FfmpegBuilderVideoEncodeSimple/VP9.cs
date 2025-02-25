namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public partial class FfmpegBuilderVideoEncodeSimple
{
    /// <summary>
    /// Gets FFmpeg arguments for encoding VP9 using the CPU
    /// </summary>
    /// <param name="quality">the quality of the video</param>
    /// <param name="speed">the encoding speed</param>
    /// <returns>the FFmpeg arguments</returns>
    private static IEnumerable<string> VP9_CPU(int quality, int speed)
    {
        return new []
        {
            "libvpx-vp9",
            "-preset", MapSpeed(speed, "medium"),
            "-crf", MapQuality(quality).ToString(),
        };
    }

    /// <summary>
    /// Gets FFmpeg arguments for encoding VP9 using the Intel's QSV hardware encoder 
    /// </summary>
    /// <param name="quality">the quality of the video</param>
    /// <param name="speed">the encoding speed</param>
    /// <returns>the FFmpeg arguments</returns>
    private static IEnumerable<string> VP9_Qsv(int quality, int speed)
    {
        var parameters = new List<string>();
        parameters.AddRange(new[]
        {
            "vp9_qsv",
            "-global_quality:v", MapQuality(quality).ToString(),
            "-preset", MapSpeed(speed, "medium"),
        });
        return parameters;
    }
}