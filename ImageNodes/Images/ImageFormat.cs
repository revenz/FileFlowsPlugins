using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace FileFlows.ImageNodes.Images;

public class ImageFormat: ImageNode
{
    public override int Inputs => 1;
    public override int Outputs => 1;
    public override FlowElementType Type => FlowElementType.Process; 
    public override string Icon => "fas fa-file-image";

    public override int Execute(NodeParameters args)
    {
        using var image = Image.Load(args.WorkingFile, out IImageFormat format);
        
        var formatOpts = GetFormat(args);
        SaveImage(args, image, formatOpts.file, formatOpts.format ?? format);
        return 1;
    }
}
