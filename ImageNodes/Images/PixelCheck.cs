using System.ComponentModel;
namespace FileFlows.ImageNodes.Images;

/// <summary>
/// Flow element that checks if the image has the required number of pixels
/// </summary>
public class PixelCheck: ImageBaseNode
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc /> 
    public override string Icon => "fas fa-th";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/image-nodes/pixel-check";

    /// <summary>
    /// Gets or sets the number of pixels to require
    /// </summary>
    [NumberInt(1)]
    [DefaultValue(500 * 500)]
    public int Pixels { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        int width = CurrentWidth;
        int height = CurrentHeight;
        args.Logger?.ILog("Image Width: " + width);
        args.Logger?.ILog("Image Height: " + height);
        int totalPixels = width * height;
        args.Logger?.ILog("Total Pixels: " + totalPixels);
        if (totalPixels < Pixels)
        {
            args.Logger?.ILog($"Total Pixels '{totalPixels}' is less than required '{Pixels}'");
            return 2;
        }
        args.Logger?.ILog($"Total Pixels '{totalPixels}' is greater than or equal to the required '{Pixels}'");
        return 1;
    }
}
