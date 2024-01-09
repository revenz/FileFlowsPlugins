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
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/delete-source-directory";

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
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output to call next, -1 to abort flow, 0 to end flow</returns>
    public override int Execute(NodeParameters args)
    {
        string path = args.LibraryFileName.Substring(0, args.LibraryFileName.Length - args.RelativeFile.Length);

        args.Logger?.ILog("Library File Name: " + args.LibraryFileName);
        args.Logger?.ILog("Library Path: " + path);
        args.Logger?.ILog("Relative File: " + args.RelativeFile);
        //args.Logger?.ILog("IsRemote: " + args.IsRemote);

        int pathIndex = args.RelativeFile.IndexOfAny(new[] { '\\', '/' });
        if (pathIndex < 0)
        {
            args.Logger?.ILog("File is in library root, will not delete");
            return base.Execute(args);
        }

        string topdir = args.RelativeFile.Substring(0, pathIndex);

        string pathSeparator = args.FileService.PathSeparator.ToString();

        args.Logger?.ILog("Path Separator: " + pathSeparator);
        string pathToDelete = path.EndsWith(pathSeparator) ? path + topdir : path + pathSeparator + topdir;
        args.Logger?.ILog("Path To Delete: " + pathToDelete);

        // if (args.IsRemote)
        // {
        //     if (args.LibraryFileName.StartsWith(pathToDelete))
        //     {
        //         args.Logger?.ILog("Deleting original file remotely: " + args.LibraryFileName);
        //         args.DeleteRemote(args.LibraryFileName, false,
        //             null); // first delete the source file since its in this directory we are deleting
        //     }
        //     
        //     args.Logger?.ILog("Sending request to delete remote path: " + pathToDelete);
        //
        //     bool deleted = args.DeleteRemote(pathToDelete, IfEmpty, IncludePatterns);
        //     if (deleted)
        //         args.Logger?.ILog("Successfully deleted remote path: " + pathToDelete);
        //     else
        //         args.Logger?.WLog("Failed to delete remote path: " + pathToDelete);
        //     return deleted ? 1 : 2;
        // }

        if (IfEmpty)
        {
            string libFilePath = args.IsDirectory ? args.FileName : FileHelper.GetDirectory(args.FileName);
            args.Logger?.ILog("libFilePath: " + libFilePath);
            return RecursiveDelete(args, path, libFilePath, true);
        }

        args.Logger?.ILog("Deleting directory: " + pathToDelete);
        var result = args.FileService.DirectoryDelete(pathToDelete, false);
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
        var dirFullName = FileHelper.GetDirectory(path);
        if (string.Equals(dirFullName, root, StringComparison.CurrentCultureIgnoreCase))
        {
            args.Logger?.ILog("At root, stopping deleting: " + root);
            return 1;
        }

        if (dirFullName.Length <= root.Length)
        {
            args.Logger?.ILog("At root2, stopping deleting: " + root);
            return 1;
        }

        if (deleteSubFolders == false)
        {
            if (args.FileService.GetDirectories(path).ValueOrDefault?.Any() == true)
            {
                args.Logger?.ILog("Directory contains subfolders, cannot delete: " + path);
                return 2;
            }
        }

        var files = args.FileService.GetFiles(path, "*.*", true).ValueOrDefault ?? new string [] {};
        if (IncludePatterns?.Any() == true)
        {
            var count = files.Where(x =>
            {
                foreach (var pattern in IncludePatterns)
                {
                    if (x.Contains(pattern))
                        return true;
                    try
                    {
                        if (System.Text.RegularExpressions.Regex.IsMatch(x, pattern,
                                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                            return true;
                    }
                    catch (Exception)
                    {
                    }
                }

                return false;
            }).Count();
            if (count > 0)
            {
                args.Logger?.ILog("Directory is not empty, cannot delete: " + path);
                return 2;
            }
        }
        else if (files.Length == 0)
        {
            args.Logger?.ILog("Directory is not empty, cannot delete: " + path);
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