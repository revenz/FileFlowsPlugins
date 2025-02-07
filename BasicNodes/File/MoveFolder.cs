using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using FileFlows.Plugin.Helpers;

namespace FileFlows.BasicNodes.File;

/// <summary>
/// Flow element that moves a folder
/// </summary>
public class MoveFolder : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 1;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc />
    public override string Icon => "fas fa-people-carry";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/move-directory";

    /// <summary>
    /// Gets or sets the source path to move
    /// </summary>
    [Folder(1)]
    public string SourcePath { get; set; }

    /// <summary>
    /// Gets or sets the destination path
    /// </summary>
    [Required]
    [Folder(2)]
    public string DestinationPath { get; set; }

    /// <summary>
    /// Gets or sets a sub folder should be created
    /// </summary>
    [Boolean(3)]
    [DefaultValue(true)]
    public bool CreateSubfolder { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        string source = args.ReplaceVariables(SourcePath ?? string.Empty)?.EmptyAsNull() ?? args.WorkingFile;

        bool updateWorkingFolder = args.WorkingFile == source;
        
        var existsResult = args.FileService.DirectoryExists(source);
        if (existsResult.Failed(out var error))
        {
            args.FailureReason = error;
            args.Logger?.ELog(error);
            return -1;
        }

        if (existsResult.Value == false)
        {
            args.FailureReason = "Directory does not exists: " + source;
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        var dest = args.ReplaceVariables(DestinationPath ?? string.Empty);
        if (string.IsNullOrWhiteSpace(dest))
        {
            args.FailureReason = "No destination path set";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        if (CreateSubfolder)
        {
            var subfolder = new DirectoryInfo(source).Name;
            args.Logger?.ILog("Creating sub folder: " + subfolder);
            dest = FileHelper.Combine(dest, subfolder);
        }
        
        args.Logger?.ILog("Moving Directory: " + source);
        args.Logger?.ILog("Destination Directory: " + dest);

        var moveResult = args.FileService.DirectoryMove(source, dest);
        if (moveResult.Failed(out error))
        {
            args.FailureReason = error;
            args.Logger?.ELog(error);
            return -1;
        }

        if (updateWorkingFolder)
        {
            args.Logger?.ILog("Updating working folder to: " + dest);
            args.SetWorkingFile(dest, dontDelete: true);
        }

        args.Logger?.ILog("Directory moved");
        return 1;

    }
}