using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using FileHelper = FileFlows.Plugin.Helpers.FileHelper;

namespace FileFlows.BasicNodes.File;

/// <summary>
/// Flow element to delete the source directory
/// </summary>
public class DeleteSourceDirectory : Node
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
    /// Gets the flow element type
    /// </summary>
    public override FlowElementType Type => FlowElementType.Process;
    /// <summary>
    /// Gets the icon
    /// </summary>
    public override string Icon => "far fa-trash-alt";
    /// <summary>
    /// Gets the Help URL
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/delete-source-folder";

    /// <summary>
    /// Gets or sets if the directory should only be deleted if empty
    /// </summary>
    [Boolean(1)] public bool IfEmpty { get; set; }

    /// <summary>
    /// Gets or sets an optional list of file patterns to include in the If empty check.
    /// eg if [mkv, mp4, mov, etc] is the list, if any video file is found the directory will not be deleted.
    /// </summary>
    [StringArray(2)] public string[] IncludePatterns { get; set; }
    
    /// <summary>
    /// Gets or sets if only the top most directory should be deleted, and all parent directories should be left alone
    /// </summary>
    [Boolean(3)] public bool TopMostOnly { get; set; }

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output to call next, -1 to abort flow, 0 to end flow</returns>
    public override int Execute(NodeParameters args)
        => TopMostOnly ? DeleteTopMostOnly(args) : DeleteFull(args);

    /// <summary>
    /// Deletes only the top most directory in the library
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output to call next, -1 to abort flow, 0 to end flow</returns>
    public int DeleteTopMostOnly(NodeParameters args)
    {
        string libraryPath = args.LibraryFileName[..^args.RelativeFile.Length]
            .TrimEnd('/')
            .TrimEnd('\\');
        string dir = args.OriginalIsDirectory ? args.LibraryFileName : FileHelper.GetDirectory(args.LibraryFileName);
        if (string.Equals(dir, libraryPath, StringComparison.InvariantCultureIgnoreCase))
        {
            args.Logger?.WLog("Cannot delete library root: " + dir);
            return 2;
        }

        args.Logger?.ILog("Deleting top level directory only: " + dir);
        var existsResult = args.FileService.DirectoryExists(dir);
        if (existsResult.Failed(out var error))
        {
            args.Logger?.WLog("Failed determining if directory exists: " + error);
            return 2;
        }

        if (existsResult.Value == false)
        {
            args.Logger?.ILog($"Directory '{dir}' no longer exists.");
            return 2;
        }

        if (IfEmpty)
        {
            args.Logger?.ILog($"Checking if directory is empty: {dir}");
            var emptyResult = args.FileService.DirectoryEmpty(dir, IncludePatterns);
            if (emptyResult.Failed(out error))
            {
                args.Logger?.WLog(error);
                return 2;
            }

            if (emptyResult.Value == false)
            {
                args.Logger?.ILog("Directory is not empty");
                return 2;
            }
            args.Logger?.ILog("Directory is considered empty");
        }

        args.Logger?.ILog("About to delete directory: " + dir);
        var deleteResult = args.FileService.DirectoryDelete(dir, true);
        if (deleteResult.Failed(out error))
        {
            args.Logger?.WLog("Failed to deleted directory: " + error);
            return 2;
        }

        args.Logger?.ILog("Directory deleted: " + dir);
        return 1;
    }

    /// <summary>
    /// Deletes the full library folder
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output to call next, -1 to abort flow, 0 to end flow</returns>
    private int DeleteFull(NodeParameters args)
    {
        string libraryPath = args.LibraryFileName[..^args.RelativeFile.Length]
            .TrimEnd('/')
            .TrimEnd('\\');

        string topdir;
        if (args.FileService.DirectoryExists(args.LibraryFileName))
        {
            args.Logger?.ILog("Library Folder Name: " + args.LibraryFileName);
            args.Logger?.ILog("Library Path: " + libraryPath);
            args.Logger?.ILog("Relative Folder: " + args.RelativeFile);
            topdir = args.RelativeFile;
        }
        else
        {
            args.Logger?.ILog("Library File Name: " + args.LibraryFileName);
            args.Logger?.ILog("Library Path: " + libraryPath);
            args.Logger?.ILog("Relative File: " + args.RelativeFile);

            int pathIndex = args.RelativeFile.IndexOfAny(['\\', '/']);
            if (pathIndex < 0)
            {
                args.Logger?.ILog("File is in library root, will not delete");
                return 2;
            }

            topdir = args.RelativeFile[..pathIndex];
        }

        string pathSeparator = args.FileService.PathSeparator.ToString();

        args.Logger?.ILog("Path Separator: " + pathSeparator);
        string pathToDelete = libraryPath.EndsWith(pathSeparator) ? libraryPath + topdir : libraryPath + pathSeparator + topdir;
        args.Logger?.ILog("Path To Delete: " + pathToDelete);

        if (IfEmpty)
        {
            string libFileDirectory = args.IsDirectory ? args.FileName : FileHelper.GetDirectory(args.LibraryFileName);
            args.Logger?.ILog("libFileDirectory: " + libFileDirectory);
            return RecursiveDelete(args, libraryPath, libFileDirectory, true);
        }

        args.Logger?.ILog("Deleting directory: " + pathToDelete);
        var result = args.FileService.DirectoryDelete(pathToDelete, true);
        if (result.IsFailed)
        {
            args.Logger?.WLog("Failed to deleted directory: " + result.Error);
            return 2;
        }

        return 1;
    }

    private int RecursiveDelete(NodeParameters args, string root, string path, bool deleteSubFolders)
    {
        if (string.IsNullOrWhiteSpace(path))
            return 1;
        
        args.Logger?.ILog("Checking directory to delete: " + path);
        //DirectoryInfo dir = new DirectoryInfo(path);
        if (string.Equals(path, root, StringComparison.CurrentCultureIgnoreCase))
        {
            args.Logger?.ILog("At root, stopping deleting: " + root);
            return 1;
        }

        if (path.Length <= root.Length)
        {
            args.Logger?.ILog("At root2, stopping deleting: " + root);
            return 1;
        }

        if (deleteSubFolders == false)
        {
            var dirResult = args.FileService.GetDirectories(path);
            if (dirResult.Failed(out var derror))
            {
                args.Logger.WLog("Failed to get directories: " + derror);
                return 2;
            }

            if (dirResult.Value == null)
            {
                args.Logger.WLog("Directory result was null, assuming an error");
                return 2;
            }
            
            if (dirResult.Value.Length > 0)
            {
                args.Logger?.ILog("Directory contains subfolders, cannot delete: " + path);
                return 2;
            }
        }

        var result = args.FileService.GetFiles(path, "", true);
        if (result.Failed(out var error))
        {
            args.Logger.WLog("Failed to get files: " + error);
            return 2;
        }

        if (result.Value == null)
        {
            args.Logger.WLog("File result was null, assuming an error");
            return 2;
        }

        var files = result.Value;
        args.Logger?.ILog($"Directory contains {files.Length} file{(files.Length == 1 ? "" : "s")}");
        
        foreach (var file in files)
        {
            args.Logger?.ILog("File: " + file);
        }
        
        if(files.Any(x => x.ToLowerInvariant().EndsWith(".fftemp")))
        {
            args.Logger?.ILog("Temporary FF file found, cannot delete.");
            return 2;
        }

        if (IncludePatterns?.Any() == true)
        {
            var includeFiles = files.Where(x =>
            {
                foreach (var pattern in IncludePatterns)
                {
                    if (x.Contains(pattern))
                        return true;
                    try
                    {
                        if (System.Text.RegularExpressions.Regex.IsMatch(x, pattern.Trim(),
                                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                            return true;
                    }
                    catch (Exception)
                    {
                    }
                }

                return false;
            }).ToList();
            if (includeFiles.Any())
            {
                args.Logger?.ILog("Directory is not empty, cannot delete: " + path + Environment.NewLine +
                                  string.Join(Environment.NewLine, includeFiles.Select(x => " - " + x)));
                return 2;
            }
        }
        else if (files.Any())
        {
            args.Logger?.ILog("Directory is not empty, cannot delete: " + path + Environment.NewLine +
                              string.Join(Environment.NewLine, files.Select(x => " - " + x)));
            return 2;
        }

        args.Logger?.ILog("Deleting directory: " + path);
        try
        {
            args.FileService.DirectoryDelete(path, true);
        }
        catch (Exception ex)
        {
            args.Logger?.WLog("Failed to delete directory: " + ex.Message);
            return args.FileService.DirectoryExists(path).Is(true) ? 2 : 1; // silently fail
        }

        string parent = FileHelper.GetDirectory(path);
        return RecursiveDelete(args, root, parent, false);
    }
}