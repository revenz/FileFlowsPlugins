using System.ComponentModel;

namespace FileFlows.ImageNodes.Images;

/// <summary>
/// Flow element that crops an image of any white/black borders
/// </summary>
public class AutoCropImage : ImageNode
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/image-nodes/auto-crop-image";
    /// <inheritdoc />
    public override string Icon => "fas fa-crop";

    /// <summary>
    /// Gets or sets the crop threshold
    /// </summary>
    [Slider(1)]
    [Range(1, 100)]
    [DefaultValue(50)]
    public int Threshold { get; set; }

    /// <inheritdoc />
    protected override Result<bool> PerformAction(NodeParameters args, string localFile, string destination)
        => args.ImageHelper.Trim(localFile, destination, Threshold, GetImageTypeFromFormat(), Quality);
}
