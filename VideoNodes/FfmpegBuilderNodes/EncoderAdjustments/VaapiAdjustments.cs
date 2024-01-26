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
        
        // "-pix_fmt:v:{index}", "p010le", "-profile:v:{index}", "main10"
        int main10 = args.IndexOf("main10");
        if (main10 > 0)
        {
            // we dont specify main 10 for vaapi
            args.RemoveAt(main10);
            args.RemoveAt(main10 - 1);
        }

        string scale = args.FirstOrDefault(x => x.StartsWith("scale="));
        bool scaleRemoved = false;

        int vfIndex = args.IndexOf("-vf");
        if (vfIndex < 0)
        {
            int yIndex = args.IndexOf("-y");
            if (yIndex >= 0)
            {
                string vf = "format=nv12,hwupload";
                if (string.IsNullOrWhiteSpace(scale) == false)
                {
                    args.Remove(scale);
                    vf += "," + scale.Replace("scale=", "scale_vaapi=")
                        .Replace(":flags=lanczos", string.Empty);
                    scaleRemoved = true;
                }
                
                args.InsertRange(yIndex + 1, new [] { "-vf", vf });
            }
        }

        if (scaleRemoved == false)
        {
            for (int i = 0; i < args.Count; i++)
            {
                var arg = args[i];
                if (arg.StartsWith("scale=")) // scale should use scale_vaapi
                    args[i] = arg.Replace("scale=", "scale_vaapi=");
            }
        }

        // scale filter was used, but then removed
        int filterV0 = args.IndexOf("-filter:v:0");
        if(filterV0 > 0 && filterV0 < args.Count - 1 && args[filterV0 + 1] == "-map")
            args.RemoveAt(filterV0);

        return args;
    }
}