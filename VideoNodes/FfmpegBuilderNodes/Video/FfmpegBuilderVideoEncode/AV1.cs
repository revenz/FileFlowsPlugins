using FileFlows.VideoNodes.Helpers;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public partial class FfmpegBuilderVideoEncode
{
    /// <summary>
    /// AV1 CPU encoding
    /// </summary>
    /// <param name="quality">the quality</param>
    /// <param name="speed">the speed</param>
    /// <returns>the encoding parameters</returns>
    private static IEnumerable<string> AV1_CPU(int quality, string speed)
    {
        string preset = "4";
        switch (speed)
        {
            case "ultrafast": preset = "13"; break;
            case "superfast": preset = "11"; break;
            case "veryfast":  preset = "10"; break;
            case "faster": preset = "9"; break;
            case "fast": preset = "8"; break;
            case "medium": preset = "6"; break;
            case "slow": preset = "4"; break;
            case "slower": preset = "2"; break;
            case "veryslow": preset = "1"; break;
        }
        return new []
        {
            //"libaom-av1",
            "libsvtav1",
            "-preset", preset,
            "-crf", quality.ToString()
        };
    }
    
    /// <summary>
    /// AV1 AMD encoding
    /// </summary>
    /// <param name="quality">the quality</param>
    /// <param name="speed">the speed</param>
    /// <returns>the encoding parameters</returns>
    private static IEnumerable<string> AV1_Amd(int quality, string speed)
    {
        return new[]
        {
            "av1_amf",
            "-qp", quality.ToString(),
            "-preset", speed?.EmptyAsNull() ?? "slower",
            "-spatial-aq", "1"
        };
    }
    
    /// <summary>
    /// AV1 NVIDIA encoding
    /// </summary>
    /// <param name="quality">the quality</param>
    /// <param name="speed">the speed</param>
    /// <returns>the encoding parameters</returns>
    private static IEnumerable<string> AV1_Nvidia(int quality, string speed)
    {
        switch (speed)
        {
            case "ultrafast": speed = "p1"; break;
            case "superfast": speed = "p1"; break;
            case "veryfast": speed = "p1"; break;
            case "faster": speed = "p2"; break;
            case "fast": speed = "p3"; break;
            case "medium": speed = "p4"; break;
            case "slow": speed = "p5"; break;
            case "slower": speed = "p6"; break;
            case "veryslow": speed = "p7"; break;
            default: speed = "p4"; break; // unexpected
        }
        return new []
        {
            "av1_nvenc",
            "-rc", "constqp",
            "-qp", quality.ToString(),
            "-preset", speed,
            "-spatial-aq", "1"
        };
    }
    
    /// <summary>
    /// AV1 QSV encoding
    /// </summary>
    /// <param name="quality">the quality</param>
    /// <param name="speed">the speed</param>
    /// <returns>the encoding parameters</returns>
    private static IEnumerable<string> AV1_Qsv(int quality, string speed)
    {
        switch (speed)
        {
            case "ultrafast": speed = "7"; break;
            case "superfast": speed = "7"; break;
            case "veryfast": speed = "7"; break;
            case "faster": speed = "6"; break;
            case "fast": speed = "5"; break;
            case "medium": speed = "4"; break;
            case "slow": speed = "3"; break;
            case "slower": speed = "2"; break;
            case "veryslow": speed = "1"; break;
            default: speed = "4"; break; // unexpected
        }
        var args = new List<string>
        {
            "av1_qsv",
            "-global_quality:v", quality.ToString(),
            "-preset", speed
        };
        if(VaapiHelper.VaapiLinux)
            args.AddRange(new [] { "-qsv_device", VaapiHelper.VaapiRenderDevice});
        
        return args.ToArray();
    }
}