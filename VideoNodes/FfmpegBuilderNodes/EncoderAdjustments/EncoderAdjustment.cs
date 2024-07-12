using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes.EncoderAdjustments;

/// <summary>
/// Adjusts FFmpeg arguments depending on devices being used
/// </summary>
public class EncoderAdjustment
{
    /// <summary>
    /// Run any adjustments that are needed to FFmpeg arguments
    /// </summary>
    /// <param name="logger">a logger to log to</param>
    /// <param name="model">the FFmpeg Builder model</param>
    /// <param name="args">the FFmpeg args to adjust</param>
    /// <returns>the adjusted FFMpeg args</returns>
    public static List<string> Run(ILogger logger, FfmpegModel model, List<string> args)
    {
        if (VaapiAdjustments.IsUsingVaapi(args))
            return new VaapiAdjustments().Run(logger, model, args);

        return args;
    }
}