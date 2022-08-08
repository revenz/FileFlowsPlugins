using SixLabors.ImageSharp.Formats;

namespace FileFlows.ImageNodes.Images;

public class ImageFormat: ImageNode
{
    public override int Inputs => 1;
    public override int Outputs => 2;
    public override FlowElementType Type => FlowElementType.Process;

    public override string HelpUrl => "https://docs.fileflows.com/plugins/image-nodes/image-format";
    public override string Icon => "fas fa-file-image";

    public override int Execute(NodeParameters args)
    {
        var formatOpts = GetFormat(args);
        
        if(formatOpts.format?.Name == CurrentFormat.Name)
        {
            args.Logger?.ILog("File already in format: " + formatOpts.format.Name);
            return 2;
        }

        using var image = Image.Load(args.WorkingFile, out IImageFormat format);
        SaveImage(args, image, formatOpts.file, formatOpts.format ?? format);
        return 1;
    }
}
