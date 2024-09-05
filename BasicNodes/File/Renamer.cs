using FileFlows.Plugin.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.File;

public class Renamer : Node
{
    public override int Inputs => 1;
    public override int Outputs => 1;
    public override string Icon => "fas fa-font";

    public string _Pattern = string.Empty;

    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/renamer"; 

    public override FlowElementType Type => FlowElementType.Process;

    [Required]
    [TextVariable(1)]
    public string? Pattern
    {
        get => _Pattern;
        set { _Pattern = value ?? ""; }
    }

    private string _DestinationPath = string.Empty;

    [Folder(2)]
    public string DestinationPath
    {
        get => _DestinationPath;
        set { _DestinationPath = value ?? ""; }
    }

    [Boolean(3)]
    public bool LogOnly { get; set; }

    [File(4)]
    public string CsvFile { get; set; }

    public override int Execute(NodeParameters args)
    {
        if(string.IsNullOrEmpty(Pattern))
        {
            args.Logger?.ELog("No pattern specified");
            return -1;
        }

        args.Logger?.ILog("Pattern: " + (Pattern ?? string.Empty));
        args.Logger?.ILog("Destination Path: " + (DestinationPath ?? string.Empty));
        
        string newFile = Pattern;
        // in case they set a linux path on windows or vice versa
        newFile = newFile.Replace('\\', args.FileService.PathSeparator);
        newFile = newFile.Replace('/', args.FileService.PathSeparator);

        newFile = args.ReplaceVariables(newFile, stripMissing: true, cleanSpecialCharacters: true);
        newFile = Regex.Replace(newFile, @"\.(\.[\w\d]+)$", "$1");
        // remove empty [], (), {}
        newFile = newFile.Replace("()", "").Replace("{}", "").Replace("[]", "");
        // remove double space that may have been introduced by empty [], () removals
        while (newFile.IndexOf("  ", StringComparison.Ordinal) >= 0)
            newFile = newFile.Replace("  ", " ");
        newFile = Regex.Replace(newFile, @"\s(\.[\w\d]+)$", "$1");
        newFile = newFile.Replace(" \\", "\\");
        newFile = newFile.Replace(" /", "/");

        args.Logger?.ILog("New File: " + newFile);
        
        string destFolder = args.ReplaceVariables(DestinationPath ?? string.Empty, stripMissing: true, cleanSpecialCharacters: true);
        args.Logger?.ILog("destFolder[0]: " + destFolder);
        if (string.IsNullOrEmpty(destFolder))
        {
            destFolder = FileHelper.GetDirectory(args.WorkingFile);
            args.Logger?.ILog("destFolder[1]: " + destFolder);
        }

        if (destFolder.EndsWith("/") | destFolder.EndsWith(@"\"))
        {
            destFolder = destFolder[..^1];
            args.Logger?.ILog("destFolder[2]: " + destFolder);
        }

        var dest = FileHelper.Combine(destFolder, args.GetSafeName(newFile));
        args.Logger?.ILog("dest: " + dest);
        
        string destExtension = FileHelper.GetExtension(dest);

        if (string.IsNullOrEmpty(destExtension) == false)
        {
            // just ensures extensions are lowercased
            dest = dest[..dest.LastIndexOf(destExtension, StringComparison.Ordinal)] +
                   destExtension.ToLower();
        }

        args.Logger?.ILog("Renaming file to: " + dest);

        if (string.IsNullOrEmpty(CsvFile) == false)
        {
            var result = args.FileService.FileAppendAllText(CsvFile, EscapeForCsv(args.FileName) + "," + EscapeForCsv(dest) + Environment.NewLine);
            if(result.IsFailed)
                args.Logger?.ELog("Failed to append to CSV file: " + result.Error);
        }

        if (LogOnly)
            return 1;

        return args.MoveFile(dest) ? 1 : -1;
    }

    private string EscapeForCsv(string input)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append('"');
        foreach(char c in input)
        {
            sb.Append(c);
            if(c == '"')
                sb.Append('"');
        }
        sb.Append('"');
        return sb.ToString();
    }
}
