namespace FileFlows.ImageNodes.Images;

using FileFlows.Plugin;

public class ImageFile : ImageBaseNode
{
    public override int Outputs => 1;
    public override FlowElementType Type => FlowElementType.Input;
    public override string HelpUrl => "https://fileflows.com/docs/plugins/image-nodes/image-file";

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
            var fileInfo = new FileInfo(args.WorkingFile);
            if (fileInfo.Exists)
            {
                args.Variables["ORIGINAL_CREATE_UTC"] = fileInfo.CreationTimeUtc;
                args.Variables["ORIGINAL_LAST_WRITE_UTC"] = fileInfo.LastWriteTimeUtc;
            }

            UpdateImageInfo(args, this.Variables);
            if(string.IsNullOrEmpty(base.CurrentFormat) == false)
                args.RecordStatistic("IMAGE_FORMAT", base.CurrentFormat);

            return 1;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Failed processing MusicFile: " + ex.Message);
            return -1;
        }
    }
}
