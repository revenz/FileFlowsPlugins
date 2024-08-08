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
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the next output to call</returns>
    public override int Execute(NodeParameters args)
    {
        string path = args.ReplaceVariables(this.FileName ?? string.Empty, stripMissing: true);
        if (string.IsNullOrEmpty(path))
            path = args.WorkingFile;

        if (args.FileService.DirectoryExists(path).Is(true))
        {
            try
            {
                args.Logger?.ILog("Deleting directory: " + path);
                var result = args.FileService.DirectoryDelete(path, true);
                if (result.IsFailed)
                    args.Logger?.ELog("Failed deleting directory: "+ result.Error);
                else
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
                var result = args.FileService.FileDelete(path);
                if(result.IsFailed)
                    args.Logger?.ELog("Failed to Deleted file: " + result.Error);
                else
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