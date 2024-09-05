namespace FileFlows.ImageNodes.Images;

/// <summary>
/// Checks if an image is portrait
/// </summary>
public class ImageIsPortrait : ImageBaseNode
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic; 
    /// <inheritdoc />
    public override string Icon => "fas fa-portrait";


    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var img = GetImageInfo(args);
        if (img == null)
            return -1;
        return img.IsPortrait ? 1 : 2;
    }
}
