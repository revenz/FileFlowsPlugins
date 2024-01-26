namespace FileFlows.VideoNodes.FfmpegBuilderNodes.EncoderAdjustments;

public interface IEncoderAdjustment
{
    List<string> Run(ILogger logger, List<string> args);
}