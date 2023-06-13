using System.ComponentModel.DataAnnotations;
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
    public override string HelpUrl => "https://docs.fileflows.com/plugins/basic-nodes/move-file";

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
        var dest = GetDesitnationPath(args, DestinationPath, DestinationFile, MoveFolder);
        if (dest == null)
            return -1;

        string destDir = new FileInfo(dest).DirectoryName;

        args.CreateDirectoryIfNotExists(destDir ?? String.Empty);

        var srcDir = AdditionalFilesFromOriginal ? new FileInfo(args.FileName).DirectoryName : new FileInfo(args.WorkingFile).DirectoryName;

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

    internal static string  GetDesitnationPath(NodeParameters args, string destinationPath, string destinationFile = null, bool moveFolder = false)
    {
        string dest = args.ReplaceVariables(destinationPath, true);
        dest = dest.Replace("\\", Path.DirectorySeparatorChar.ToString());
        dest = dest.Replace("/", Path.DirectorySeparatorChar.ToString());
        if (string.IsNullOrEmpty(dest))
        {
            args.Logger?.ELog("No destination specified");
            args.Result = NodeResult.Failure;
            return null;
        }
        args.Result = NodeResult.Failure;

        if (moveFolder)
            dest = Path.Combine(dest, args.RelativeFile);
        else
            dest = Path.Combine(dest, new FileInfo(args.FileName).Name);

        var fiDest = new FileInfo(dest);
        var fiWorking = new FileInfo(args.WorkingFile);
        if (string.IsNullOrEmpty(fiDest.Extension) == false && fiDest.Extension != fiWorking.Extension)
        {
            dest = dest.Substring(0, dest.LastIndexOf(".")) + fiWorking.Extension;
        }

        if (string.IsNullOrEmpty(destinationFile) == false)
        {
            // FF-154 - changed file.Name and file.Orig.Filename to be the full short filename including the extension
            destinationFile = destinationFile.Replace("{file.Orig.FileName}{file.Orig.Extension}", "{file.Orig.FileName}");
            destinationFile = destinationFile.Replace("{file.Name}{file.Extension}", "{file.Name}");
            destinationFile = destinationFile.Replace("{file.Name}{ext}", "{file.Name}");
            string destFile = args.ReplaceVariables(destinationFile);
            dest = Path.Combine(new FileInfo(dest).DirectoryName!, destFile);
        }

        fiDest = new FileInfo(dest);
        var fiWorkingFile = new FileInfo(args.WorkingFile);
        if (fiDest.Extension != fiWorkingFile.Extension)
        {
            dest = dest.Replace(fiDest.Extension, fiWorkingFile.Extension);
            fiDest = new FileInfo(dest);
        }

        return dest;
    }
}