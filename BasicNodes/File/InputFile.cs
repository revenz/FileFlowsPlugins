using System.Text.RegularExpressions;
using FileFlows.Plugin;

namespace FileFlows.BasicNodes.File;

/// <summary>
/// Basic Input file, the default input node
/// </summary>
public partial class InputFile : Node
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
        if (UrlRegex().IsMatch(args.WorkingFile))
        {
            args.Logger?.ILog("URL Detected: "  + args.WorkingFile);
            return 1;
        }
        try
        {
            if(args.FileService.FileExists(args.WorkingFile).Is(true) == false)
            {
                args.Logger?.ELog("File not found: " + args.WorkingFile);
                return -1;
            }
            
            return 1;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Failed in InputFile: " + ex.Message + Environment.NewLine + ex.StackTrace);
            return -1;
        }
    }

    /// <summary>
    /// Regex to detect a URL
    /// </summary>
    /// <returns>the URL regex</returns>
    [GeneratedRegex("^http(s)://", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex UrlRegex();
}