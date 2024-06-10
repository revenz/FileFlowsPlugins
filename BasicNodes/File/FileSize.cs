using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.File;

/// <summary>
/// Flow element that compares a file size
/// </summary>
public class FileSize : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string Icon => "fas fa-balance-scale-right";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/file-size";


    /// <summary>
    /// Gets or sets the lower value
    /// </summary>
    [NumberInt(1)]
    public int Lower { get; set; }
    /// <summary>
    /// Gets or sets the upper value
    /// </summary>

    [NumberInt(2)]
    public int Upper { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var result = args.IsDirectory ? args.FileService.DirectorySize(args.WorkingFile) : args.FileService.FileSize(args.WorkingFile);
        if(result.Failed(out string error))
        {
            args.Logger.ELog("Error getting size: " + error);
            return -1;
        }

        return TestSize(args, result.ValueOrDefault);
    }

    public int TestSize(NodeParameters args, long size)
    {
        if (size < (((long)Lower) * 1024 * 1024))
            return 2;
        if (Upper > 0 && size > (((long)Upper) * 1024 * 1024))
            return 2;
        return 1;
    }
}