using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using System.IO.Compression;

namespace FileFlows.BasicNodes.File;

/// <summary>
/// Node that unzips a file
/// </summary>
public class Unzip : Node
{
    public override int Inputs => 1;
    public override int Outputs => 1;
    public override FlowElementType Type => FlowElementType.Process;
    public override string Icon => "fas fa-file-archive";
    public override string HelpUrl => "https://docs.fileflows.com/plugins/basic-nodes/unzip";
    
    private string _DestinationPath = string.Empty;
    private string _zip = string.Empty;

    [Folder(1)]
    public string DestinationPath
    {
        get => _DestinationPath;
        set { _DestinationPath = value ?? ""; }
    }

    [TextVariable(2)]
    public string Zip
    {
        get => _zip;
        set { _zip = value ?? ""; }
    }

    public override int Execute(NodeParameters args)
    {
        try
        {
            var zip = args.ReplaceVariables(Zip ?? string.Empty, stripMissing: true)?.EmptyAsNull() ?? args.WorkingFile;
            
            if (System.IO.File.Exists(zip) == false)
            {
                args.Logger?.ELog("File does not exist: " + zip);
                return -1;
            }

            string destDir = args.ReplaceVariables(DestinationPath, stripMissing: true, cleanSpecialCharacters: true);
            
            ZipFile.ExtractToDirectory(zip, destDir);
            return 1;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Failed unzip: " + ex.Message + Environment.NewLine + ex.StackTrace);
            return -1;
        }
    }
}
