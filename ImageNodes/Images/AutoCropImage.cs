using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using System.ComponentModel;

namespace FileFlows.ImageNodes.Images;

public class AutoCropImage : ImageNode
{
    public override int Inputs => 1;
    public override int Outputs => 2;
    public override FlowElementType Type => FlowElementType.Process;
    public override string HelpUrl => "https://docs.fileflows.com/plugins/image-nodes/auto-crop-image";
    public override string Icon => "fas fa-crop";

    [Slider(1)]
    [Range(1, 100)]
    [DefaultValue(50)]
    public int Threshold { get; set; }


    public override int Execute(NodeParameters args)
    {
        using var image = Image.Load(args.WorkingFile, out IImageFormat format);
        int originalWidth = image.Width;
        int originalHeight= image.Height;
        float threshold = Threshold / 100f;
        if (threshold < 0)
            threshold = 0.5f;

        args.Logger?.ILog("Attempting to auto crop using threshold: " + threshold);
        image.Mutate(c => c.EntropyCrop(threshold));

        if (image.Width == originalWidth && image.Height == originalHeight)
            return 2;

        var formatOpts = GetFormat(args);
        SaveImage(args, image, formatOpts.file, formatOpts.format ?? format);
        args.Logger?.ILog($"Image cropped from '{originalWidth}x{originalHeight}' to '{image.Width}x{image.Height}'");

        return 1;
    }
}
