using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace FileFlows.ImageNodes.Images;

public class ImageFlip: ImageNode
{
    public override int Inputs => 1;
    public override int Outputs => 1;
    public override FlowElementType Type => FlowElementType.Process; 
    public override string Icon => "fas fa-sync-alt";

    [Boolean(2)]
    public bool Vertical { get; set; }


    public override int Execute(NodeParameters args)
    {
        using var image = Image.Load(args.WorkingFile, out IImageFormat format);
        image.Mutate(c => c.Flip(Vertical ? FlipMode.Vertical : FlipMode.Horizontal));
        var formatOpts = GetFormat(args);
        SaveImage(image, formatOpts.file, formatOpts.format ?? format);
        args.SetWorkingFile(formatOpts.file);
        
        return 1;
    }
}
