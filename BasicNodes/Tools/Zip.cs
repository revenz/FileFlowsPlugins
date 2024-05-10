using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using FileFlows.Plugin.Helpers;

namespace FileFlows.BasicNodes.File;

/// <summary>
/// A flow element that zips files or directories.
/// </summary>
public class Zip : Node
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
    /// Gets the element type
    /// </summary>
    public override FlowElementType Type => FlowElementType.Process;
    /// <summary>
    /// Gets the icon
    /// </summary>
    public override string Icon => "fas fa-file-archive";
    /// <summary>
    /// Gets the help URL
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/zip";
    
    private string _DestinationPath = string.Empty;
    private string _DestinationFile = string.Empty;

    /// <summary>
    /// Gets or sets the path to zip
    /// </summary>
    [Folder(1)]
    public string Path { get; set; }

    /// <summary>
    /// Gets or sets the destination path for zipping.
    /// </summary>
    [Folder(2)]
    public string DestinationPath
    {
        get => _DestinationPath;
        set => _DestinationPath = value ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets the destination file name for zipping.
    /// </summary>
    [TextVariable(3)]
    public string DestinationFile
    {
        get => _DestinationFile;
        set => _DestinationFile = value ?? string.Empty;
    }
    
    /// <summary>
    /// Gets or sets if the working file should be updated to the zipped file
    /// </summary>
    [Boolean(4)]
    public bool SetWorkingFile { get; set; }

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        bool isDir = false;

        try
        {
            var path = string.IsNullOrWhiteSpace(Path)
                ? args.WorkingFile
                : args.ReplaceVariables(Path, stripMissing: true);
            
            if (args.FileService.DirectoryExists(path).Is(true))
            {
                isDir = true;
            }
            else if (args.FileService.FileExists(path).Is(true) == false)
            {
                args.FailureReason = "File or folder does not exist: " + path;
                args.Logger?.ELog(args.FailureReason);
                return -1;
            }

            string destDir = DestinationPath;
            if (string.IsNullOrEmpty(destDir))
            {
                if (isDir)
                    destDir = FileHelper.GetDirectory(args.LibraryPath);
                else
                    destDir = FileHelper.GetDirectory(args.FileName);
                if (string.IsNullOrEmpty(destDir))
                {
                    args.FailureReason ="Failed to get destination directory";
                    args.Logger?.ELog(args.FailureReason);
                    return -1;
                }
            }
            else
            {
                // in case they set a linux path on windows or vice versa
                destDir = destDir.Replace('\\', args.FileService.PathSeparator);
                destDir = destDir.Replace('/', args.FileService.PathSeparator);

                destDir = args.ReplaceVariables(destDir, stripMissing: true);

                // this converts it to the actual OS path
                destDir = FileHelper.GetDirectory(destDir);
                args.FileService.DirectoryCreate(destDir);
            }

            string destFile = args.ReplaceVariables(DestinationFile ?? string.Empty, true);
            if (string.IsNullOrEmpty(destFile))
            {
                destFile = FileHelper.GetShortFileName(args.FileName) + ".zip";
            }
            if (destFile.ToLower().EndsWith(".zip") == false)
                destFile += ".zip";
            destFile = FileHelper.Combine(destDir, destFile);

            string tempZip = FileHelper.Combine(args.TempPath, Guid.NewGuid() + ".zip");

            args.Logger?.ILog($"Compressing '{path}' to '{destFile}'");
            if (isDir)
            {
                if (args.FileService.FileIsLocal(path) == false)
                {
                    args.FailureReason = "Cannot zip remote directories";
                    args.Logger?.ELog(args.FailureReason);
                    return -1;
                }
                args.ArchiveHelper.Compress(path, tempZip, allDirectories: true, percentCallback:(percent) =>
                {
                    args.PartPercentageUpdate(percent);
                });
                args?.PartPercentageUpdate(100);
            }
            else
            {
                string localFile = args.FileService.GetLocalPath(path);
                args.ArchiveHelper.Compress(localFile, tempZip);
            }

            if (System.IO.File.Exists(tempZip) == false)
            {
                args.FailureReason = "Failed to create zip: " + destFile;
                args.Logger?.ELog(args.FailureReason);
                return -1;
            }

            if (args.FileService.FileMove(tempZip, destFile, true).Failed(out string error))
            {
                args.FailureReason = "Failed to move zip: " + error;
                args.Logger?.ELog(args.FailureReason);
                return -1;
            }
            
            if(SetWorkingFile)
                args.SetWorkingFile(destFile);
            
            args.Logger?.ILog("Zip created at: " + destFile);
            return 1;
        }
        catch (Exception ex)
        {
            args.FailureReason = "Failed creating zip: " + ex.Message;
            args.Logger?.ELog(args.FailureReason + ex.Message + Environment.NewLine + ex.StackTrace);
            return -1;
        }
    }
}
