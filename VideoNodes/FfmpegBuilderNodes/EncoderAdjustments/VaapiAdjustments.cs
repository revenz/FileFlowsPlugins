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
    public static bool IsUsingVaapi(IEnumerable<string> args)
        => args.Any(x => x == "hevc_vaapi" || x == "h264_vaapi");
    
    /// <summary>
    /// Runt the adjustments
    /// </summary>
    /// <param name="args">the ffmpeg args</param>
    /// <returns>the adjusted arguments</returns>
    public List<string> Run(ILogger logger, List<string> args)
    {
        //logger.ILog("Original VAAPI parameters: \n" + string.Join("\n", args));
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
        string additionalFilters = string.Empty;
        if (main10 > 0)
        {
            // we dont specify main 10 for vaapi
            args.RemoveAt(main10);
            args.RemoveAt(main10 - 1);
        }
        else
        {
            additionalFilters += "format=nv12,";
        }

        additionalFilters += "hwupload,";
        for (int i = 0; i < args.Count - 2; i++)
        {
            if (args[i].StartsWith("-filter:v:") == false)
                continue;
            ++i;
            var vf = args[i];
            vf = additionalFilters + vf.Replace("scale=", "scale_vaapi=")
                .Replace(":flags=lanczos", string.Empty);
            args[i] = vf;
        }

        // int vfScaleIndex =  args.FindIndex(x => x.StartsWith("scale=") || x.Contains(", scale="));
        // if (vfScaleIndex > 0)
        // {
        //     string vfScale = args[vfScaleIndex];
        //     var filters = vfScale.Split(',').Select(x => x.Trim()).ToArray();
        //     var scale = filters.FirstOrDefault(x => x.StartsWith("scale="));
        //     bool scaleRemoved = false;
        //
        //     int vfIndex = args.IndexOf("-vf");
        //     if (vfIndex < 0)
        //     {
        //         int yIndex = args.IndexOf("-y");
        //         if (yIndex >= 0)
        //         {
        //             string vf = "format=nv12,hwupload";
        //             if (string.IsNullOrWhiteSpace(scale) == false)
        //             {
        //                 vf += "," + scale.Replace("scale=", "scale_vaapi=")
        //                     .Replace(":flags=lanczos", string.Empty);
        //                 if (filters.Length == 1)
        //                 {
        //                     args.RemoveAt(vfScaleIndex);
        //                     args.RemoveAt(vfScaleIndex - 1); // remove the filter for this
        //                     scaleRemoved = true;
        //                 }
        //                 else
        //                 {
        //                     args = args.Select(x =>
        //                     {
        //                         if (x == vfScale)
        //                             return string.Join(", ", filters.Where(x => x != scale));
        //                         return x;
        //                     }).ToList();
        //                 }
        //             }
        //
        //             args.InsertRange(yIndex + 1, new[] { "-vf", vf });
        //         }
        //     }
        // }

        // if (scaleRemoved == false)
        // {
        //     for (int i = 0; i < args.Count; i++)
        //     {
        //         var arg = args[i];
        //         if (arg.StartsWith("scale=")) // scale should use scale_vaapi
        //             args[i] = arg.Replace("scale=", "scale_vaapi=");
        //     }
        // }

        // scale filter was used, but then removed
        // int filterV0 = args.IndexOf("-filter:v:0");
        // if(filterV0 > 0 && filterV0 < args.Count - 1 && args[filterV0 + 1].StartsWith("-"))
        //     args.RemoveAt(filterV0);

        //logger.ILog("Updated VAAPI parameters: \n" + string.Join("\n", args));
        return args;
    }
}