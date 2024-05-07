using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Pbm;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace FileFlows.ImageNodes.Images;

public class ImageResizer: ImageNode
{
    public override int Inputs => 1;
    public override int Outputs => 1;
    public override FlowElementType Type => FlowElementType.Process; 
    public override string Icon => "fas fa-expand";

    public override string HelpUrl => "https://fileflows.com/docs/plugins/image-nodes/image-resizer";


    [Select(nameof(ResizeModes), 2)]
    public ResizeMode Mode { get; set; }

    private static List<ListOption>? _ResizeModes;
    
    /// <summary>
    /// Gets the resize modes
    /// </summary>
    public static List<ListOption> ResizeModes
    {
        get
        {
            if (_ResizeModes == null)
            {
                _ResizeModes = new List<ListOption>
                {
                    new () { Value = ResizeMode.Fill, Label = "Fill (Stretches to fit)"},
                    new () { Value = ResizeMode.Contain, Label = "Contain (Preserves aspect ratio but contained in bounds)"},
                    new () { Value = ResizeMode.Cover, Label =  "Cover (Preserves aspect ratio)"},
                    new () { Value = ResizeMode.Min, Label = "Min (Resizes the image until the shortest side reaches the set given dimension)"},
                    new () { Value = ResizeMode.Max, Label = "Max (Constrains the resized image to fit the bounds of its container maintaining the original aspect ratio)"},
                    new () { Value = ResizeMode.None, Label = "None (Not resized)"}
                };
            }
            return _ResizeModes;
        }
    }
    
    [NumberInt(3)]
    [Range(1, int.MaxValue)]
    public int Width { get; set; }
    [NumberInt(4)]
    [Range(1, int.MaxValue)]
    public int Height { get; set; }

    [Boolean(5)]
    public bool Percent { get; set; }

    public override int Execute(NodeParameters args)
    {
        string inputFile = ConvertImageIfNeeded(args);
        var format = Image.DetectFormat(inputFile);
        using var image = Image.Load(inputFile);
        SixLabors.ImageSharp.Processing.ResizeMode rzMode;
        switch (Mode)
        {
            case ResizeMode.Contain: rzMode = SixLabors.ImageSharp.Processing.ResizeMode.Pad;
                break;
            case ResizeMode.Cover: rzMode = SixLabors.ImageSharp.Processing.ResizeMode.Crop;
                break;
            case ResizeMode.Fill: rzMode = SixLabors.ImageSharp.Processing.ResizeMode.Stretch;
                break;
            case ResizeMode.Min: rzMode = SixLabors.ImageSharp.Processing.ResizeMode.Min;
                break;
            case ResizeMode.Max: rzMode = SixLabors.ImageSharp.Processing.ResizeMode.Max;
                break;
            default: rzMode = SixLabors.ImageSharp.Processing.ResizeMode.BoxPad;
                break;
        }

        var formatOpts = GetFormat(args);

        float w = Width;
        float h = Height;
        if (Percent)
        {
            w = (int)(image.Width * (w / 100f));
            h = (int)(image.Height * (h / 100f));
        }
        
        image.Mutate(c => c.Resize(new ResizeOptions()
        {
            Size = new Size((int)w, (int)h),
            Mode = rzMode
        }));
        
        SaveImage(args, image, formatOpts.file, formatOpts.format ?? format);
        return 1;
    }
}
