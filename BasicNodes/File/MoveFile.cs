using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using FileFlows.BasicNodes.Helpers;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using FileFlows.Plugin.Helpers;

namespace FileFlows.BasicNodes.File;

/// <summary>
/// Moves a file to a new location
/// </summary>
public class MoveFile : Node
{
    /// <summary>
    /// Gets the number of inputs
    /// </summary>
    public override int Inputs => 1;
    /// <summary>
    /// Gets the number of outputs
    /// </summary>
    public override int Outputs => 2;
    /// <summary>
    /// Gets the type of flow element
    /// </summary>
    public override FlowElementType Type => FlowElementType.Process;
    /// <summary>
    /// Gets the icon for the flow element
    /// </summary>
    public override string Icon => "fas fa-file-export";
    /// <summary>
    /// Gets the help URL
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/move-file";

    /// <summary>
    /// Gets or sets the input file to move
    /// </summary>
    [TextVariable(1)]
    public string InputFile{ get; set; }

    /// <summary>
    /// Gets or sets the destination path
    /// </summary>
    [Required]
    [Folder(2)]
    public string DestinationPath { get; set; }

    /// <summary>
    /// Gets or sets the destination file
    /// </summary>
    [TextVariable(3)]
    public string DestinationFile{ get; set; }

    /// <summary>
    /// Gets or sets if the folder should be moved
    /// </summary>
    [Boolean(4)]
    public bool MoveFolder { get; set; }
    
    /// <summary>
    /// Gets or sets if the original should be deleted
    /// </summary>
    [Boolean(5)]
    public bool DeleteOriginal { get; set; }

    /// <summary>
    /// Gets or sets additional files that should also be moved
    /// </summary>
    [StringArray(6)]
    public string[] AdditionalFiles { get; set; }

    /// <summary>
    /// Gets or sets original files from the original file location that should also be moved
    /// </summary>
    [Boolean(7)]
    public bool AdditionalFilesFromOriginal { get; set; }

    /// <summary>
    /// Gets or sets if the original files creation and last write time dates should be preserved
    /// </summary>
    [Boolean(8)]
    public bool PreserverOriginalDates { get; set; }
    
    /// <summary>
    /// Executes the node
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        string destFile = args.ReplaceVariables(DestinationFile ?? string.Empty, stripMissing: true);
        
        string inputFile = args.ReplaceVariables(InputFile ?? string.Empty, stripMissing: true)?.EmptyAsNull() ?? args.WorkingFile;
        if (inputFile != args.WorkingFile && string.IsNullOrWhiteSpace(DestinationFile))
            destFile = FileHelper.GetShortFileName(inputFile);
        
        var dest = GetDestinationPath(args, DestinationPath, destFile, MoveFolder);
        if (dest == null)
        {
            args.FailureReason = "Failed to get move destination";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        if (inputFile != args.WorkingFile && string.IsNullOrWhiteSpace(destFile) == false)
            dest = FileHelper.Combine(FileHelper.GetDirectory(dest), destFile); // ensure the non working file has correct extension/name

        // store srcDir here before we move and the working file is altered
        var srcDir = FileHelper.GetDirectory(AdditionalFilesFromOriginal ? args.FileName : inputFile);
        args.Logger?.ILog("Source Directory: " + srcDir);
        string shortNameLookup = FileHelper.GetShortFileName(args.FileName);
        if (shortNameLookup.LastIndexOf(".", StringComparison.InvariantCulture) > 0)
            shortNameLookup = shortNameLookup[..shortNameLookup.LastIndexOf(".", StringComparison.Ordinal)];
        
        args.Logger?.ILog("shortNameLookup: " + shortNameLookup);
        
        args.Logger?.ILog("Moving file: " + inputFile);
        args.Logger?.ILog("Destination: " + dest);


        if (inputFile == args.WorkingFile)
        {
            args.Logger?.ILog("Moving working file: " + dest);
            if (args.MoveFile(dest).Failed(out var error))
            {
                args.Logger?.WLog("Failed to moving working file: " + error);
                if (error.Contains(": ") == false)
                    error = "Failed to move file: " + error;
                args.FailureReason = error;
                args.Logger?.ELog(args.FailureReason);
                return -1;
            }
        }
        else
        {
            if (args.FileService.FileMove(inputFile, dest).Failed(out string error))
            {
                if (error.Contains(": ") == false)
                    error = "Failed to move file: " + error;
                args.FailureReason = error;
                args.Logger?.ELog(args.FailureReason);
                return -1;
            }
        }

        if (PreserverOriginalDates)
        {
            if (args.Variables.TryGetValue("ORIGINAL_CREATE_UTC", out object oCreateTimeUtc) &&
                args.Variables.TryGetValue("ORIGINAL_LAST_WRITE_UTC", out object oLastWriteUtc) &&
                oCreateTimeUtc is DateTime dtCreateTimeUtc && oLastWriteUtc is DateTime dtLastWriteUtc)
            {
                args.Logger?.ILog("Preserving dates");
                
                args.Logger?.ILog($"Setting creation time: {dest}, {dtCreateTimeUtc}");
                if(args.FileService.SetCreationTimeUtc(dest, dtCreateTimeUtc).Failed(out var error))
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

        if(AdditionalFiles?.Any() == true)
        {
            args.Logger?.ILog("Additional Files: " + string.Join(", ", AdditionalFiles));
            var addFiles = FolderHelper.GetAdditionalFiles(args.Logger, args.FileService, args.ReplaceVariables,
                shortNameLookup, srcDir, AdditionalFiles);
            
            string destDir = FileHelper.GetDirectory(dest);
            foreach (var addFile in addFiles)
            {
                try
                {
                    args.Logger?.ILog("Additional files: " + addFile);
                            
                    string addFileDest = FileHelper.Combine(destDir, FileHelper.GetShortFileName(addFile));
                    args.FileService.FileMove(addFile, addFileDest, true);
                    args.Logger?.ILog("Moved file: \"" + addFile + "\" to \"" + addFileDest + "\"");
                }
                catch(Exception ex)
                {
                    args.Logger?.ILog("Failed moving file: \"" + addFile + "\": " + ex.Message);
                }
            }
        }
        else
        {
            args.Logger?.ILog("No additional files configured to move");
        }


        if (DeleteOriginal && args.LibraryFileName != args.WorkingFile)
        {
            args.Logger?.ILog("Deleting original file: " + args.LibraryFileName);
            try
            {
                args.FileService.FileDelete(args.LibraryFileName);
            }
            catch(Exception ex)
            {
                args.Logger?.WLog("Failed to delete original file: " + ex.Message);
                return 2;
            }
        }
        return 1;
    }

    /// <summary>
    /// Gets the full destination path 
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <param name="destinationPath">the requested destination path</param>
    /// <param name="destinationFile">the requested destination file</param>
    /// <param name="moveFolder">if the relative folder should be also be included, relative to the library</param>
    /// <returns>the full destination path</returns>
    internal static string GetDestinationPath(NodeParameters args, string destinationPath,
        string destinationFile = null, bool moveFolder = false)
    {
        var result = GetDestinationPathParts(args, destinationPath, destinationFile, moveFolder);
        if(result.Filename == null)
            return null;
        return FileHelper.Combine(result.Path, result.Filename);
    }
    
    /// <summary>
    /// Gets the destination path and filename 
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <param name="destinationPath">the requested destination path</param>
    /// <param name="destinationFile">the requested destination file</param>
    /// <param name="moveFolder">if the relative folder should be also be included, relative to the library</param>
    /// <returns>the path and filename</returns>
    internal static (string? Path, string? Filename, string? Separator) GetDestinationPathParts(NodeParameters args, string destinationPath, string destinationFile = null, bool moveFolder = false)
    {
        string separator = args.WorkingFile.IndexOf('/') >= 0 ? "/" : "\\";
        string destFolder = args.ReplaceVariables(destinationPath, stripMissing: true);
        destFolder = destFolder.Replace("\\", separator);
        destFolder = destFolder.Replace("/", separator);
        string destFilename = args.FileName.Replace("\\", separator)
                                           .Replace("/", separator);
        destFilename = destFilename.Substring(destFilename.LastIndexOf(separator, StringComparison.Ordinal) + 1);
        if (string.IsNullOrEmpty(destFolder))
        {
            args.Logger?.ELog("No destination specified");
            args.Result = NodeResult.Failure;
            return (null, null, null);
        }
        args.Result = NodeResult.Failure;

        if (moveFolder) // we only want the full directory relative to the library, we don't want the original filename
        {
            args.Logger?.ILog("Relative File: " + args.RelativeFile);
            string relative = args.RelativeFile.Replace("\\", separator).Replace("/", separator);
            if (relative.StartsWith(separator))
                relative = relative[1..];
            if (relative.IndexOf(separator, StringComparison.Ordinal) > 0)
            {
                destFolder = destFolder + separator + relative[..relative.LastIndexOf(separator, StringComparison.Ordinal)];
                args.Logger?.ILog("Using relative directory: " + destFolder);
            }
        }

        // dest = Path.Combine(dest, argsFilename);
        if (string.IsNullOrEmpty(destinationFile) == false)
        {
            // FF-154 - changed file.Name and file.Orig.Filename to be the full short filename including the extension
            destinationFile = destinationFile.Replace("{file.Orig.FileName}{file.Orig.Extension}", "{file.Orig.FileName}");
            destinationFile = destinationFile.Replace("{file.Name}{file.Extension}", "{file.Name}");
            destinationFile = destinationFile.Replace("{file.Name}{ext}", "{file.Name}");
            destFilename = args.ReplaceVariables(destinationFile, stripMissing: true);
        }
        
        string destExtension = FileHelper.GetExtension(destFilename).TrimStart('.');
        string workingExtension = FileHelper.GetExtension(args.WorkingFile).TrimStart('.');
            
        if (string.IsNullOrEmpty(destExtension) == false && destExtension != workingExtension)
        {
            destFilename = destFilename[..(destFilename.LastIndexOf(".", StringComparison.Ordinal) + 1)] + workingExtension;
        }

        args.Logger?.ILog("Final destination path: " + destFolder);
        args.Logger?.ILog("Final destination filename: " + destFilename);
        
        return (destFolder, destFilename, separator);
    }
}