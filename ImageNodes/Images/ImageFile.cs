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
            { "img.IsLandscape", false },

            { "img.DateTaken.Year", 2020 },
            { "img.DateTaken.Month", 4 },
            { "img.DateTaken.Day", 20 },
            { "img.DateTaken.FulLDate", "2020-04-20" }
        };
    }

    public override int Execute(NodeParameters args)
    {
        try
        {
            if (args.FileService.FileCreationTimeUtc(args.WorkingFile).Success(out DateTime createTime))
                args.Variables["ORIGINAL_CREATE_UTC"] = createTime;
            if (args.FileService.FileLastWriteTimeUtc(args.WorkingFile).Success(out DateTime writeTime))
                args.Variables["ORIGINAL_LAST_WRITE_UTC"] = writeTime;

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
