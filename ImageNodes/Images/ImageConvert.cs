namespace FileFlows.ImageNodes.Images;

/// <summary>
/// Converts an image to another format
/// </summary>
public class ImageConvert: ImageNode
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/image-nodes/image-convert";
    /// <inheritdoc />
    public override string Icon => "fas fa-file-image";

    /// <inheritdoc />
    protected override Result<bool> PerformAction(NodeParameters args, string localFile, string destination)
    {
        var format = GetImageTypeFromFormat();
        if (format == null)
        {
            args.Logger?.ILog("Format not set, nothing to do.");
            return false;
        }
        if (args.WorkingFile.ToLowerInvariant().EndsWith("." + Format.ToLowerInvariant()))
        {
            args.Logger?.ILog("File already in format: " + Format);
            return false;
        }
        
        return args.ImageHelper.ConvertImage(localFile,
            destination,
            format.Value, Quality);
    }
}
