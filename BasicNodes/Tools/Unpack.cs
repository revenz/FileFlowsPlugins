using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using FileFlows.BasicNodes;
using System.IO;

namespace BasicNodes.Tools;

/// <summary>
/// Node used to unpack files from archives
/// </summary>
public class Unpack: Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 1;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc />
    public override string Icon => "fas fa-file-archive";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/unpack";
    
    /// <summary>
    /// Gets or sets the destination path
    /// </summary>
    [Folder(1)]
    public string DestinationPath { get; set; }

    /// <summary>
    /// Gets or sets the file to unpack
    /// </summary>
    [TextVariable(2)]
    public string File { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        try
        {
            var localFileResult = args.FileService.GetLocalPath(
                args.ReplaceVariables(File ?? string.Empty, stripMissing: true)?.EmptyAsNull() ?? args.WorkingFile);

            if (localFileResult.Failed(out string error))
            {
                args.FailureReason = "Failed to get local file: " + error;
                args.Logger?.ELog(args.FailureReason);
                return -1;
            }

            var filename = localFileResult.Value; 

            var fileInfo = new FileInfo(filename);
            
            if (fileInfo.Exists == false)
            {
                args.FailureReason = "File does not exist: " + filename;
                args.Logger?.ELog(args.FailureReason);
                return -1;
            }

            var destDir = args.ReplaceVariables(DestinationPath, stripMissing: true, cleanSpecialCharacters: true);
            destDir = args.MapPath(destDir);
            if (Directory.Exists(destDir) == false)
            {
                if (args.FileService.DirectoryCreate(destDir).Failed(out error))
                {
                    args.FailureReason = "Failed to create destination directory: " + error;
                    args.Logger?.ELog(args.FailureReason);
                    return -1;
                }
            }

            var result = args.ArchiveHelper.Extract(filename, destDir, (percent) =>
            {
                args.PartPercentageUpdate(percent);
            });

            if (result.Failed(out error))
            {
                args.FailureReason = error;
                args.Logger?.ELog(error);
                return -1;
            }
            
            return 1;
        }
        catch (Exception ex)
        {
            args.FailureReason = "Failed to unpack: " + ex.Message;
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }
    }
}
