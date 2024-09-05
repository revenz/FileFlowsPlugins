using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes.EncoderAdjustments;

/// <summary>
/// Adjusts the FFmpeg commands
/// </summary>
public interface IEncoderAdjustment
{
    /// <summary>
    /// Runs the encoder adjustments
    /// </summary>
    /// <param name="logger">the logger to use</param>
    /// <param name="model">the FFmpeg model</param>
    /// <param name="args">the FFmpeg arguments</param>
    /// <returns>the updated FFmpeg arguments</returns>
    List<string> Run(ILogger logger, FfmpegModel model, List<string> args);
}