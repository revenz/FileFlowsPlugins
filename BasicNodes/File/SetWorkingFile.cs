using System.Text.RegularExpressions;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace BasicNodes.File;

/// <summary>
/// Flow element that sets the working file
/// </summary>
public class SetWorkingFile : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 1;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc />
    public override string Icon => "fas fa-file-invoice-dollar";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/set-working-file";

    /// <summary>
    /// Gets or sets the file to set as the working file
    /// </summary>
    [TextVariable(1)]
    public string File { get; set; }
    
    /// <summary>
    /// Gets or sets if the old temporary working file should not be deleted
    /// </summary>
    [Boolean(2)]
    public bool DontDeletePrevious { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var file = args.ReplaceVariables(File ?? string.Empty, stripMissing: true);
        if (string.IsNullOrWhiteSpace(file))
        {
            args.FailureReason = "No file set";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        if (Regex.IsMatch(file, @"^[\w\d]{2,}:"))
        {
            // special case eg nc: for next cloud, where the file wont be accessible so we just set it so it appears nicely in the UI but its gone
            args.SetWorkingFile(file);
            return 1;
        }
        
        if (args.FileService.FileExists(file))
        {
            args.Logger?.ILog("Setting new working file to: " + file);
            args.Logger?.ILog("Dont Delete Previous Temporary Working File: " + DontDeletePrevious);
            args.SetWorkingFile(file, DontDeletePrevious);
            return 1;
        }
        
        if (args.FileService.DirectoryExists(file))
        {
            args.Logger?.ILog("Setting new working file to folder: " + file);
            args.Logger?.ILog("Dont Delete Previous Temporary Working File: " + DontDeletePrevious);
            args.SetWorkingFile(file, DontDeletePrevious);
            return 1;
        }
        
        args.FailureReason = $"File or Folder '{file}' does not exist";
        args.Logger?.ELog(args.FailureReason);
        return -1;
    }
}