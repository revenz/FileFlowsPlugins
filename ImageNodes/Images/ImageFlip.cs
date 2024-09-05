namespace FileFlows.ImageNodes.Images;

/// <summary>
/// Flow element to flip an image
/// </summary>
public class ImageFlip : ImageNode
{
    /// <inheritdoc />
    public override int Inputs => 1;

    /// <inheritdoc />
    public override int Outputs => 1;

    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;

    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/image-nodes/image-flip";

    /// <inheritdoc />
    public override string Icon => "fas fa-sync-alt";

    /// <summary>
    /// Gets or sets if the image should be flipped vertically, otherwise its flipped horizontally
    /// </summary>
    [Boolean(2)]
    public bool Vertical { get; set; }

    /// <inheritdoc />
    protected override Result<bool> PerformAction(NodeParameters args, string localFile, string destination)
        => Vertical
            ? args.ImageHelper.FlipVertically(localFile, destination, GetImageTypeFromFormat(), Quality)
            : args.ImageHelper.FlipHorizontally(localFile, destination, GetImageTypeFromFormat(), Quality);
}
