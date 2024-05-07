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
    /// <summary>
    /// Gets the Help URL for this element
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/unpack";
    
    private string _DestinationPath = string.Empty;
    private string _file = string.Empty;

    /// <summary>
    /// Gets or sets the destination path
    /// </summary>
    [Folder(1)]
    public string DestinationPath
    {
        get => _DestinationPath;
        set { _DestinationPath = value ?? ""; }
    }

    /// <summary>
    /// Gets or sets the file to unpack
    /// </summary>
    [TextVariable(2)]
    public string File
    {
        get => _file;
        set => _file = value ?? string.Empty;
    }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        try
        {
            var filename = args.ReplaceVariables(File ?? string.Empty, stripMissing: true)?.EmptyAsNull() ?? args.WorkingFile;

            var fileInfo = new FileInfo(filename);
            
            if (fileInfo.Exists == false)
            {
                args.Logger?.ELog("File does not exist: " + filename);
                return -1;
            }

            string destDir = args.ReplaceVariables(DestinationPath, stripMissing: true, cleanSpecialCharacters: true);
            destDir = args.MapPath(destDir);
            if (Directory.Exists(destDir) == false)
                Directory.CreateDirectory(destDir);

            args.ArchiveHelper.Extract(filename, destDir, (percent) =>
            {
                args.PartPercentageUpdate(percent);
            });

            
            return 1;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Failed unzip: " + ex.Message + Environment.NewLine + ex.StackTrace);
            return -1;
        }
    }
}
