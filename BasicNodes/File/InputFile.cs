using FileFlows.Plugin;

namespace FileFlows.BasicNodes.File;

/// <summary>
/// Basic Input file, the default input node
/// </summary>
public class InputFile : Node
{
    /// <summary>
    /// Gets the number of outputs
    /// </summary>
    public override int Outputs => 1;
    /// <summary>
    /// Gets the element type
    /// </summary>
    public override FlowElementType Type => FlowElementType.Input;
    /// <summary>
    /// Gets the icon
    /// </summary>
    public override string Icon => "far fa-file";
    /// <summary>
    /// Gets the help URL
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/input-file";

    /// <summary>
    /// Executes the node
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        try
        {
            if(args.FileService.FileExists(args.WorkingFile).Is(true) == false)
            {
                args.Logger?.ELog("File not found: " + args.WorkingFile);
                return -1;
            }
            
            args.Variables["ORIGINAL_CREATE_UTC"] = args.FileService.FileCreationTimeUtc(args.WorkingFile).ValueOrDefault;
            args.Variables["ORIGINAL_LAST_WRITE_UTC"] = args.FileService.FileLastWriteTimeUtc(args.WorkingFile).ValueOrDefault;
            return 1;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Failed in InputFile: " + ex.Message + Environment.NewLine + ex.StackTrace);
            return -1;
        }
    }
}