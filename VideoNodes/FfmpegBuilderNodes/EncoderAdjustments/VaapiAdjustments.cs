using FileFlows.VideoNodes.Helpers;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes.EncoderAdjustments;

/// <summary>
/// Adjustments for VAAPI
/// </summary>
public class VaapiAdjustments : IEncoderAdjustment
{
    /// <summary>
    /// Gets if VAAPI hardware encoding is being used
    /// </summary>
    /// <param name="args">the ffmepg args</param>
    /// <returns>true if using VAAPI hardware encoding, otherwise false</returns>
    public static bool IsUsingVaapi(List<string> args)
        => args.Any(x => x == "hevc_vaapi" || x == "h264_vaapi");
    
    /// <summary>
    /// Runt the adjustments
    /// </summary>
    /// <param name="args">the ffmpeg args</param>
    /// <returns>the adjusted arguments</returns>
    public List<string> Run(List<string> args)
    {
        int iIndex = args.IndexOf("-i");
        if (iIndex >= 0 && VaapiHelper.VaapiLinux)
        {
            args.InsertRange(iIndex, new[]
            {
                "-vaapi_device",
                VaapiHelper.VaapiRenderDevice
            });
        }

        for(int i=0;i<args.Count;i++)
        {
            var arg = args[i];
            if (arg.StartsWith("scale=")) // scale should use scale_vaapi
                args[i] = arg.Replace("scale=", "scale_vaapi=");
        }

        return args;
    }
}