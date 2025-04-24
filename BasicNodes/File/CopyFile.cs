using System.ComponentModel.DataAnnotations;
using FileFlows.BasicNodes.Helpers;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using FileFlows.Plugin.Helpers;

namespace FileFlows.BasicNodes.File;

/// <summary>
/// Node that copies a file
/// </summary>
public class CopyFile : Node
{
    /// <summary>
    /// Gets the number of inputs
    /// </summary>
    public override int Inputs => 1;
    
    /// <summary>
    /// Gets the number of outputs
    /// </summary>
    public override int Outputs => 1;
    
    /// <summary>
    /// Gets the type of node 
    /// </summary>
    public override FlowElementType Type => FlowElementType.Process;
    
    /// <summary>
    /// Gets the icon for this node
    /// </summary>
    public override string Icon => "far fa-copy";

    /// <summary>
    /// Gets the help URL
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/copy-file";

    private string _DestinationPath = string.Empty;
    private string _DestinationFile = string.Empty;


    /// <summary>
    /// Gets or sets the input file to move
    /// </summary>
    [TextVariable(1)]
    public string InputFile{ get; set; }
    
    /// <summary>
    /// Gets or sets the destination path to copy the file to
    /// </summary>
    [Required]
    [Folder(2)]
    public string DestinationPath 
    { 
        get => _DestinationPath;
        set => _DestinationPath = value ?? string.Empty;
    }
    /// <summary>
    /// Gets or sets the destination file 
    /// </summary>
    [TextVariable(3)]
    public string DestinationFile
    {
        get => _DestinationFile;
        set => _DestinationFile = value ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets if the folder structure should be copied
    /// </summary>
    [Boolean(4)]
    public bool CopyFolder { get; set; }

    /// <summary>
    /// Gets or sets additional files to copy
    /// </summary>
    [StringArray(5)]
    public string[] AdditionalFiles { get; set; }

    /// <summary>
    /// Gets or sets if the additional files should be copied from the source location (true) or temporary location (false)
    /// </summary>
    [Boolean(6)]
    public bool AdditionalFilesFromOriginal { get; set; }

    /// <summary>
    /// Gets or sets if the original files creation and last write time dates should be preserved
    /// </summary>
    [Boolean(7)]
    public bool PreserverOriginalDates { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var destParts = MoveFile.GetDestinationPathParts(args, DestinationPath, DestinationFile, CopyFolder);
        if (destParts.Filename == null)
            return -1;
        
        string inputFile = args.ReplaceVariables(InputFile ?? string.Empty, stripMissing: true)?.EmptyAsNull() ?? args.WorkingFile;
                          
        // cant use new FileInfo(dest).Directory.Name here since
        // if the folder is a linux folder and this node is running on windows
        // /mnt, etc will be converted to c:\mnt and break the destination
        var destDir = destParts.Path;
        var destFile = destParts.Filename;
        if(inputFile != args.WorkingFile && string.IsNullOrWhiteSpace(DestinationFile))
        {
            destFile = FileHelper.GetShortFileName(inputFile);
        }
        string dest = FileHelper.Combine(destParts.Path, destFile);

        //bool copied = args.CopyFile(args.WorkingFile, dest, updateWorkingFile: true);
        //if (!copied)
        //    return -1;
        args.Logger?.ILog("File to copy: " + inputFile);
        args.Logger?.ILog("Destination: " + dest);
        

        if (args.CopyFile(inputFile, dest).Failed(out string error))
        {
            args.FailureReason = "Failed to copy file: " + error;
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        if (inputFile == args.WorkingFile)
        {
            args.Logger?.ILog("Setting working file to: " + dest);
            args.SetWorkingFile(dest);

            if (PreserverOriginalDates)
            {
                if (args.Variables.TryGetValue("ORIGINAL_CREATE_UTC", out object oCreateTimeUtc) &&
                    args.Variables.TryGetValue("ORIGINAL_LAST_WRITE_UTC", out object oLastWriteUtc) &&
                    oCreateTimeUtc is DateTime dtCreateTimeUtc && oLastWriteUtc is DateTime dtLastWriteUtc)
                {
                    args.Logger?.ILog("Preserving dates");
                
                    args.Logger?.ILog($"Setting creation time: {dest}, {dtCreateTimeUtc}");
                    if(args.FileService.SetCreationTimeUtc(dest, dtCreateTimeUtc).Failed(out error))
                        args.Logger?.WLog("Failed to set creation time: " + error);
                    args.Logger?.ILog($"Setting last write time: {dest}, {dtLastWriteUtc}");
                    if(args.FileService.SetLastWriteTimeUtc(dest, dtLastWriteUtc).Failed(out error))
                        args.Logger?.WLog("Failed to set last write time: " + error);
                }
                else
                {
                    args.Logger?.WLog("Preserve dates is on but failed to get original dates from variables");
                }
            }
        }

        var srcDir = FileHelper.GetDirectory(AdditionalFilesFromOriginal
            ? args.FileName
            : inputFile);

        if (AdditionalFiles?.Any() == true)
        {
            string shortNameLookup = FileHelper.GetShortFileName(AdditionalFilesFromOriginal ? args.FileName : inputFile);
            if (shortNameLookup.LastIndexOf(".", StringComparison.InvariantCulture) > 0)
                shortNameLookup = shortNameLookup[..shortNameLookup.LastIndexOf(".", StringComparison.Ordinal)];
            
            args.Logger?.ILog("Additional Files: " + string.Join(", ", AdditionalFiles));
            var addFiles = FolderHelper.GetAdditionalFiles(args.Logger, args.FileService, args.ReplaceVariables,
                shortNameLookup, srcDir, AdditionalFiles);

            foreach (var addFile in addFiles)
            {
                try
                {
                    string shortName = FileHelper.GetShortFileName(addFile);

                    string addFileDest = destDir + args.FileService.PathSeparator + shortName;
                    args.FileService.FileCopy(addFile, addFileDest, true);

                    args.Logger?.ILog("Copied file: \"" + addFile + "\" to \"" + addFileDest + "\"");
                }
                catch (Exception ex)
                {
                    args.Logger?.ILog("Failed copying file: \"" + addFile + "\": " + ex.Message);
                }
            }
        }

        // not needed as args.CopyFile does this
        //args?.SetWorkingFile(dest);
        return 1;
    }
}