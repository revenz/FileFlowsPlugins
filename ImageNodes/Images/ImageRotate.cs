using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace FileFlows.ImageNodes.Images;

public class ImageRotate: ImageNode
{
    public override int Inputs => 1;
    public override int Outputs => 1;
    public override FlowElementType Type => FlowElementType.Process; 
    public override string Icon => "fas fa-undo";

    [Select(nameof(AngleOptions), 2)]
    public int Angle { get; set; }

    private static List<ListOption> _AngleOptions;
    public static List<ListOption> AngleOptions
    {
        get
        {
            if (_AngleOptions == null)
            {
                _AngleOptions = new List<ListOption>
                {
                    new ListOption { Value = 90, Label = "90"},
                    new ListOption { Value = 180, Label = "180"},
                    new ListOption { Value = 270, Label = "270"}
                };
            }
            return _AngleOptions;
        }
    }

    public override int Execute(NodeParameters args)
    {
        using var image = Image.Load(args.WorkingFile, out IImageFormat format);
        image.Mutate(c => c.Rotate(Angle));
        var formatOpts = GetFormat(args);
        SaveImage(args, image, formatOpts.file, formatOpts.format ?? format);
        
        return 1;
    }
}
