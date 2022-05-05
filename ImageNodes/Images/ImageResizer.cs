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
    public override int Outputs => 2;
    public override FlowElementType Type => FlowElementType.Process; 
    public override string Icon => "fas fa-image";
    
    
    [Select(nameof(ResizeModes), 2)]
    public ResizeMode Mode { get; set; }

    private static List<ListOption> _ResizeModes;
    public static List<ListOption> ResizeModes
    {
        get
        {
            if (_ResizeModes == null)
            {
                _ResizeModes = new List<ListOption>
                {
                    new ListOption { Value = Images.ResizeMode.Fill, Label = "Fill (Stretches to fit)"},
                    new ListOption { Value = Images.ResizeMode.Contain, Label = "Contain (Preserves aspect ratio but contained in bounds)"},
                    new ListOption { Value = Images.ResizeMode.Cover, Label =  "Cover (Preserves aspect ratio)"},
                    new ListOption { Value = Images.ResizeMode.None, Label = "None (Not resized)"}
                };
            }
            return _ResizeModes;
        }
    }
    
    [NumberInt(3)]
    [Range(0, int.MaxValue)]
    public int Width { get; set; }
    [NumberInt(4)]
    [Range(0, int.MaxValue)]
    public int Height { get; set; }
    
    public override int Execute(NodeParameters args)
    {
        using var image = Image.Load(args.WorkingFile, out IImageFormat format);
        SixLabors.ImageSharp.Processing.ResizeMode rzMode;
        switch (Mode)
        {
            case ResizeMode.Contain: rzMode = SixLabors.ImageSharp.Processing.ResizeMode.Pad;
                break;
            case ResizeMode.Cover: rzMode = SixLabors.ImageSharp.Processing.ResizeMode.Crop;
                break;
            case ResizeMode.Fill: rzMode = SixLabors.ImageSharp.Processing.ResizeMode.Stretch;
                break;
            default: rzMode = SixLabors.ImageSharp.Processing.ResizeMode.BoxPad;
                break;
        }

        var formatOpts = GetFormat(args);
        
        image.Mutate(c => c.Resize(new ResizeOptions()
        {
            Size = new Size(Width, Height),
            Mode = rzMode
        }));
        
        SaveImage(image, formatOpts.file, formatOpts.format ?? format);
        args.SetWorkingFile(formatOpts.file);
        return 1;
    }
}
