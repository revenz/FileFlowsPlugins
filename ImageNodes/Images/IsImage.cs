namespace FileFlows.ImageNodes.Images;

using FileFlows.Plugin;

/// <summary>
/// Tests if a file is an image
/// </summary>
public class IsImage : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/image-nodes/is-image";
    /// <inheritdoc />
    public override string Icon => "fas fa-question";

    /// <summary>
    /// Gets or sets the file to test
    /// </summary>
    [TextVariable(1)]
    public string File { get; set; } = null!;

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var file = args.ReplaceVariables(File ?? string.Empty)?.EmptyAsNull() ?? args.WorkingFile;
        args.Logger?.ILog("Testing file: " + file);
        var localFile = args.FileService.GetLocalPath(file);
        if (localFile.IsFailed)
        {
            args.FailureReason = "Working file cannot be read: " + localFile.Error;
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }
        
        var info = args.ImageHelper.GetInfo(localFile);
        if(info.Failed(out string error))
        {
            args.Logger?.ILog("Not an image");
            args.Logger?.ILog(error);
            return 2;
        }
        args.Logger?.ILog("Is an image");
        args.Logger?.ILog("Width: " + info.Value.Width);
        args.Logger?.ILog("Height: " + info.Value.Height);
        if(string.IsNullOrEmpty(info.Value.Format) == false)
            args.Logger?.ILog("Format: " + info.Value.Format);

        return 1;
    }
}
