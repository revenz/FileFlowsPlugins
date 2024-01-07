using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.File;

/// <summary>
/// A flow element that deletes a single item (file or directory)
/// </summary>
public class Delete : Node
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
    /// Gets the type of flow element
    /// </summary>
    public override FlowElementType Type => FlowElementType.Process;
    /// <summary>
    /// Gets the icon for the flow element
    /// </summary>
    public override string Icon => "far fa-trash-alt";
    /// <summary>
    /// Gets the help URL for the flow element
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/delete";

    /// <summary>
    /// Gets or sets the FileName/path to delete
    /// </summary>
    [TextVariable(1)] public string FileName { get; set; }

    /// <summary>
    /// Tests if a path is a directory
    /// </summary>
    /// <param name="path">the path to test</param>
    /// <returns>true if a directory, otherwise false</returns>
    private bool IsDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            return Directory.Exists(path);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the next output to call</returns>
    public override int Execute(NodeParameters args)
    {
        string path = args.ReplaceVariables(this.FileName ?? string.Empty, stripMissing: true);
        if (string.IsNullOrEmpty(path))
            path = args.WorkingFile;

        bool originalFile = path == args.FileName;
        if (originalFile && args.IsRemote)
        {
            args.Logger?.ILog("Deleting original file remotely: " + args.LibraryFileName);
            var result = args.DeleteRemote(args.LibraryFileName, false, null);
            return 1;
        }
        
        if (IsDirectory(path))
        {
            try
            {
                args.Logger?.ILog("Deleting directory: " + path);
                Directory.Delete(path, true);
                args.Logger?.ILog("Deleted directory: " + path);
                return 1;
            }
            catch (Exception ex)
            {
                args.Logger?.ELog("Failed to delete directory: " + ex.Message);
                return -1;
            }
        }
        else
        {
            try
            {
                args.Logger?.ILog("Deleting file: " + path);
                System.IO.File.Delete(path);
                args.Logger?.ILog("Deleted file: " + path);
                return 1;
            }
            catch (Exception ex)
            {
                args.Logger?.ELog($"Failed to delete file: '{path}' => {ex.Message}");
                return -1;
            }
        }
    }
}