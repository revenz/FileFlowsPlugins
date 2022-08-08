namespace FileFlows.ImageNodes.Images;

using FileFlows.Plugin;

public class ImageFile : ImageBaseNode
{
    public override int Outputs => 1;
    public override FlowElementType Type => FlowElementType.Input;
    public override string HelpUrl => "https://docs.fileflows.com/plugins/image-nodes/image-file";

    public override string Icon => "fas fa-file-image";

    private Dictionary<string, object> _Variables;
    public override Dictionary<string, object> Variables => _Variables;
    public ImageFile()
    {
        _Variables = new Dictionary<string, object>()
        {
            { "img.Width", 1920 },
            { "img.Height", 1080 },
            { "img.Format", "PNG" },
            { "img.IsPortrait", true },
            { "img.IsLandscape", false }
        };
    }

    public override int Execute(NodeParameters args)
    {
        try
        {
            UpdateImageInfo(args, this.Variables);
            if(string.IsNullOrEmpty(base.CurrentFormat?.Name) == false)
                args.RecordStatistic("IMAGE_FORMAT", base.CurrentFormat.Name);

            return 1;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Failed processing MusicFile: " + ex.Message);
            return -1;
        }
    }
}
