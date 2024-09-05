using FileFlows.Plugin.Helpers;
using FileFlows.Plugin.Types;

namespace FileFlows.ImageNodes.Images;

/// <summary>
/// Flow element that resizes an image
/// </summary>
public class ImageResizer: ImageNode
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 1;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc /> 
    public override string Icon => "fas fa-expand";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/image-nodes/image-resizer";

    /// <summary>
    /// Gets or sets the resize mode
    /// </summary>
    [Select(nameof(ResizeModes), 3)]
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
                    new () { Value = ResizeMode.Fill, Label = "Fill"},
                    new () { Value = ResizeMode.Contain, Label = "Contain"},
                    new () { Value = ResizeMode.Cover, Label =  "Cover"},
                    new () { Value = ResizeMode.Min, Label = "Min"},
                    new () { Value = ResizeMode.Max, Label = "Max"},
                    new () { Value = ResizeMode.Pad, Label = "Pad"}
                };
            }
            return _ResizeModes;
        }
    }
    
    /// <summary>
    /// Gets or sets the new width 
    /// </summary>
    [NumberPercent(5, "Labels.Pixels", 0, false)]
    public NumberPercent? Width { get; set; }
    
    /// <summary>
    /// Gets or sets the new height
    /// </summary>
    [NumberPercent(6, "Labels.Pixels", 0, false)]
    public NumberPercent? Height { get; set; }
    
    /// <inheritdoc />
    protected override Result<bool> PerformAction(NodeParameters args, string localFile, string destination)
    {
        float w = Width!.Percentage ? (int)(CurrentWidth * (Width.Value / 100f)) : Width.Value;
        float h = Height!.Percentage ? (int)(CurrentHeight * (Height.Value / 100f)) : Height.Value;
            
        return args.ImageHelper.Resize(localFile, destination, (int)w, (int)h, Mode, GetImageTypeFromFormat(), Quality);
    }
}
