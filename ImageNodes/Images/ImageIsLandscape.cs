namespace FileFlows.ImageNodes.Images;

public class ImageIsLandscape: ImageBaseNode
{
    public override int Inputs => 1;
    public override int Outputs => 2;
    public override FlowElementType Type => FlowElementType.Logic; 
    public override string Icon => "fas fa-image";

    public override string HelpUrl => "https://docs.fileflows.com/plugins/image-nodes/image-is-landscape";


    public override int Execute(NodeParameters args)
    {
        var img = GetImageInfo(args);
        if (img == null)
            return -1;
        return img.IsLandscape ? 1 : 2;
    }
}
