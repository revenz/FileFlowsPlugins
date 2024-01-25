namespace FileFlows.VideoNodes.FfmpegBuilderNodes.EncoderAdjustments;

public class EncoderAdjustment
{
    public static List<string> Run(List<string> args)
    {
        if (VaapiAdjustments.IsUsingVaapi(args))
            return new VaapiAdjustments().Run(args);

        return args;
    }
}