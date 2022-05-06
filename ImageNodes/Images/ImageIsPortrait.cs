namespace FileFlows.ImageNodes.Images;

public class ImageIsPortrait : ImageBaseNode
{
    public override int Inputs => 1;
    public override int Outputs => 2;
    public override FlowElementType Type => FlowElementType.Logic; 
    public override string Icon => "fas fa-portrait";


    public override int Execute(NodeParameters args)
    {
        var img = GetImageInfo(args);
        if (img == null)
            return -1;
        return img.IsPortrait ? 1 : 2;
    }
}
