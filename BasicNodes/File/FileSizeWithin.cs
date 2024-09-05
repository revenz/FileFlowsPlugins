using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using FileFlows.Plugin.Types;

namespace FileFlows.BasicNodes.File;

/// <summary>
/// Flow element that compares a file size within
/// </summary>
public class FileSizeWithin : Node
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
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/file-size-within";

    /// <summary>
    /// Gets or sets the value to compare
    /// </summary>
    [NumberPercent(6, "MB", 0, false)]
    public NumberPercent? Value { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var key = args.IsDirectory ? "folder.Orig.Size" : "file.Orig.Size";
        // try get from variables
        if (args.Variables.ContainsKey(key) == false ||
            args.Variables[key] is long origSize == false ||
            origSize <= 0)
        {
            args.Logger?.ELog("Original file does not exists, cannot check size");
            return -1;
        }

        var result = args.IsDirectory ? args.FileService.DirectorySize(args.WorkingFile) : args.FileService.FileSize(args.WorkingFile);
        if(result.Failed(out string error))
        {
            args.Logger.ELog("Error getting size: " + error);
            return -1;
        }
        
        long size = result.Value;
        
        args.Logger?.ILog("Original Size: " + origSize);
        args.Logger?.ILog("Current Size: " + size);

        if (Value.Percentage)
        {
            // if Value.Percent, then the size must be + or - in the Value.Value percent value of the origSize
            double percentageDifference = Math.Abs((size - origSize) / (double)origSize) * 100;
            if (percentageDifference > Value.Value)
            {
                args.Logger?.ILog($"Size difference {percentageDifference}% exceeds allowed {Value.Value}% of the original size");
                return 2;
            }
            args.Logger?.ILog($"Size difference {percentageDifference}% within allowed {Value.Value}% of the original size");
            return 1;
        }
        else
        {
            // if Value.Percent == false, then the size must be + or - in the Value.Value MB (ie * 1_000_000) of the origSize
            long byteDifference = Math.Abs(size - origSize);
            long allowedDifference = Value.Value * 1_000_000;
            if (byteDifference > allowedDifference)
            {
                args.Logger?.ILog($"Size difference {byteDifference} bytes exceeds allowed {allowedDifference} bytes ({Value.Value} MB) of the original size");
                return 2;
            }

            args.Logger?.ILog($"Size difference {byteDifference} bytes within allowed {allowedDifference} bytes ({Value.Value} MB) of the original size");
            return 1;
        }

    }
}