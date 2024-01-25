namespace FileFlows.VideoNodes.FfmpegBuilderNodes.EncoderAdjustments;

public interface IEncoderAdjustment
{
    List<string> Run(List<string> args);
}