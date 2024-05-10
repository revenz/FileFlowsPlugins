namespace FileFlows.ImageNodes.Images;

/// <summary>
/// Checks if an image is landascape
/// </summary>
public class ImageIsLandscape: ImageBaseNode
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc /> 
    public override string Icon => "fas fa-image";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/image-nodes/image-is-landscape";


    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var img = GetImageInfo(args);
        if (img == null)
            return -1;
        return img.IsLandscape ? 1 : 2;
    }
}
