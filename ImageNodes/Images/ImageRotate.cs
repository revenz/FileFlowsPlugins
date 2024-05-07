using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace FileFlows.ImageNodes.Images;

public class ImageRotate: ImageNode
{
    public override int Inputs => 1;
    public override int Outputs => 1;
    public override FlowElementType Type => FlowElementType.Process; 
    public override string Icon => "fas fa-undo";

    public override string HelpUrl => "https://fileflows.com/docs/plugins/image-nodes/image-rotate";

    [Select(nameof(AngleOptions), 2)]
    public int Angle { get; set; }

    private static List<ListOption>? _AngleOptions;
    /// <summary>
    /// Gest the Angle Options
    /// </summary>
    public static List<ListOption> AngleOptions
    {
        get
        {
            if (_AngleOptions == null)
            {
                _AngleOptions = new List<ListOption>
                {
                    new () { Value = 90, Label = "90"},
                    new () { Value = 180, Label = "180"},
                    new () { Value = 270, Label = "270"}
                };
            }
            return _AngleOptions;
        }
    }

    public override int Execute(NodeParameters args)
    {
        string inputFile = ConvertImageIfNeeded(args);
        var format = Image.DetectFormat(inputFile);
        using var image = Image.Load(inputFile);
        image.Mutate(c => c.Rotate(Angle));
        var formatOpts = GetFormat(args);
        SaveImage(args, image, formatOpts.file, formatOpts.format ?? format);
        
        return 1;
    }
}
