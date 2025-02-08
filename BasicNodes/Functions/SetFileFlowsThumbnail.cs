using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.Functions;


/// <summary>
/// Flow element that sets the thumbnail of a file in FileFlows
/// </summary>
public class SetFileFlowsThumbnail: Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 1;
    /// <inheritdoc />
    public override string Icon => "fas fa-image";
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/set-fileflows-thumbnail";
    
    /// <summary>
    /// Gets or sets the name of the file of the thumbnail
    /// </summary>
    [TextVariable(1)]
    public string FilePath { get; set; }
    
    /// <summary>
    /// Gets or sets if the thumbnail should only be set if not already set
    /// </summary>
    [Boolean(2)]
    public bool IfNotSet { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        if (IfNotSet && args.HasThumbnailBeenSet())
        {
            args.Logger?.ILog("Thumbnail already set");
            return 1;
        }
        string file = args.ReplaceVariables(FilePath ?? string.Empty, true)?.EmptyAsNull() ?? args.WorkingFile;
        if(string.IsNullOrWhiteSpace(file))
            return args.Fail("File Path not set");
        var localResult = args.FileService.GetLocalPath(file);
        if(localResult.Failed(out var error))
            return args.Fail("Failed to get local path: " + error);
        var local = localResult.Value;
        args.SetThumbnail(file);
        args.Logger.ILog("Thumbnail set");
        return 1;
    }
}