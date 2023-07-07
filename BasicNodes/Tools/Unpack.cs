using System.Diagnostics;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using System.IO.Compression;
using FileFlows.BasicNodes;
using SharpCompress.Archives;

namespace BasicNodes.Tools;

/// <summary>
/// Node used to unpack files from archives
/// </summary>
public class Unpack: Node
{
    public override int Inputs => 1;
    public override int Outputs => 1;
    public override FlowElementType Type => FlowElementType.Process;
    public override string Icon => "fas fa-file-archive";
    /// <summary>
    /// Gets the Help URL for this element
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/unpack";
    
    private string _DestinationPath = string.Empty;
    private string _file = string.Empty;

    [Folder(1)]
    public string DestinationPath
    {
        get => _DestinationPath;
        set { _DestinationPath = value ?? ""; }
    }

    [TextVariable(2)]
    public string File
    {
        get => _file;
        set { _file = value ?? ""; }
    }

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

            if (fileInfo.Extension.ToLower() == ".zip")
                ZipFile.ExtractToDirectory(filename, destDir, true);
            else    
                Extract(args, args.WorkingFile, destDir);
            
            return 1;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Failed unzip: " + ex.Message + Environment.NewLine + ex.StackTrace);
            return -1;
        }
    }
    
    /// <summary>
    /// Unpacks a file
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <param name="workingFile">the file to extract</param>
    /// <param name="destinationPath">the location to extract to</param>
    private void Extract(NodeParameters args, string workingFile, string destinationPath)
    {
        bool isRar = workingFile.ToLowerInvariant().EndsWith(".cbr");
        try
        {
            ArchiveFactory.WriteToDirectory(workingFile, destinationPath);
        }
        catch (Exception ex) when (isRar && ex.Message.Contains("Unknown Rar Header"))
        {
            UnrarCommandLineExtract(args, workingFile, destinationPath);
        }
    }
    
    
    /// <summary>
    /// Unpacks a folder
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <param name="workingFile">the file to extract</param>
    /// <param name="destinationPath">the location to extract to</param>
    void UnrarCommandLineExtract(NodeParameters args, string workingFile, string destinationPath)
    {
        var process = new Process();
        string unrar = args.GetToolPath("unrar")?.EmptyAsNull() ?? "unrar";
        process.StartInfo.FileName = unrar;
        process.StartInfo.ArgumentList.Add("x");
        process.StartInfo.ArgumentList.Add("-o+");
        process.StartInfo.ArgumentList.Add(workingFile);
        process.StartInfo.ArgumentList.Add(destinationPath);
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
        string output = process.StandardError.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        args.Logger?.ILog("Unrar output:\n" + output);
        if (string.IsNullOrWhiteSpace(error) == false)
            args.Logger?.ELog("Unrar error:\n" + error);

        if (process.ExitCode != 0)
            throw new Exception(error?.EmptyAsNull() ?? "Failed to extract rar file");
    }
}
