using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

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
    /// Gets or sets the destination path
    /// </summary>
    [Required]
    [Folder(1)]
    public string DestinationPath { get; set; }

    /// <summary>
    /// Gets or sets the destination file
    /// </summary>
    [TextVariable(2)]
    public string DestinationFile{ get; set; }

    /// <summary>
    /// Gets or sets if the folder should be moved
    /// </summary>
    [Boolean(3)]
    public bool MoveFolder { get; set; }
    
    /// <summary>
    /// Gets or sets if the original should be deleted
    /// </summary>
    [Boolean(4)]
    public bool DeleteOriginal { get; set; }

    /// <summary>
    /// Gets or sets additional files that should also be moved
    /// </summary>
    [StringArray(5)]
    public string[] AdditionalFiles { get; set; }

    /// <summary>
    /// Gets or sets original files from the original file location that should also be moved
    /// </summary>
    [Boolean(6)]
    public bool AdditionalFilesFromOriginal { get; set; }

    /// <summary>
    /// Gets or sets if the original files creation and last write time dates should be preserved
    /// </summary>
    [Boolean(7)]
    public bool PreserverOriginalDates { get; set; }

    /// <summary>
    /// Executes the node
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        var dest = GetDestinationPath(args, DestinationPath, DestinationFile, MoveFolder);
        if (dest == null)
            return -1;

        if (args.MoveFile(dest) == false)
            return -1;
        
        if(PreserverOriginalDates && args.Variables.TryGetValue("ORIGINAL_CREATE_UTC", out object oCreateTimeUtc) &&
           args.Variables.TryGetValue("ORIGINAL_LAST_WRITE_UTC", out object oLastWriteUtc) &&
           oCreateTimeUtc is DateTime dtCreateTimeUtc && oLastWriteUtc is DateTime dtLastWriteUtc)
        {
            Helpers.FileHelper.SetLastWriteTime(dest, dtLastWriteUtc);
            Helpers.FileHelper.SetCreationTime(dest, dtCreateTimeUtc);
        }

        if(AdditionalFiles?.Any() == true)
        {
            try
            {
                string destDir = new FileInfo(args.MapPath(dest)).DirectoryName;
                args.CreateDirectoryIfNotExists(destDir ?? String.Empty);
                var srcDir = AdditionalFilesFromOriginal ? new FileInfo(args.FileName).DirectoryName : new FileInfo(args.WorkingFile).DirectoryName;

                var diSrc = new DirectoryInfo(srcDir);
                foreach (var additional in AdditionalFiles)
                {
                    foreach(var addFile in diSrc.GetFiles(additional))
                    {
                        try
                        {
                            string addFileDest = Path.Combine(destDir, addFile.Name);
                            System.IO.File.Move(addFile.FullName, addFileDest, true);
                            args.Logger?.ILog("Moved file: \"" + addFile.FullName + "\" to \"" + addFileDest + "\"");
                        }
                        catch(Exception ex)
                        {
                            args.Logger?.ILog("Failed moving file: \"" + addFile.FullName + "\": " + ex.Message);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                args.Logger.WLog("Error moving additional files: " + ex.Message);
            }
        }


        if (DeleteOriginal && args.FileName != args.WorkingFile)
        {
            args.Logger?.ILog("Deleting original file: " + args.FileName);
            try
            {
                System.IO.File.Delete(args.FileName);
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
        return result.Path + result.Separator + result.Filename;
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
        string destFolder = args.ReplaceVariables(destinationPath, true);
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
                destFolder = destFolder + separator + relative.Substring(0, relative.LastIndexOf(separator, StringComparison.Ordinal));
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
            destFilename = args.ReplaceVariables(destinationFile);
        }
        
        string destExtension = new FileInfo(destFilename).Extension;
        string workingExtension = new FileInfo(args.WorkingFile).Extension;

        if (string.IsNullOrEmpty(destExtension) == false && destExtension != workingExtension)
        {
            destFilename = destFilename.Substring(0, destFilename.LastIndexOf(".", StringComparison.Ordinal)) + workingExtension;
        }

        args.Logger?.ILog("Final destination path: " + destFolder);
        args.Logger?.ILog("Final destination filename: " + destFilename);
        
        return (destFolder, destFilename, separator);
    }
}