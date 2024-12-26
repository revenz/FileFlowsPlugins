using FileFlows.Plugin.Helpers;

namespace FileFlows.ResellerPlugin.FlowElements;

/// <summary>
/// Moves a file to a new location
/// </summary>
public class MoveToUserFolder: Node
{
    /// <summary>
    /// Gets the number of inputs
    /// </summary>
    public override int Inputs => 1;
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
    public override string HelpUrl => "https://fileflows.com/docs/plugins/reseller/move-to-user-folder";

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        if (args.Variables.TryGetValue("ResellerUserOutputDir", out var oOutputDir) == false || string.IsNullOrWhiteSpace(oOutputDir as string))
            return args.Fail("No user output directory in variables");

        var outputDir = (string)oOutputDir;

        string filename = FileHelper.GetShortFileName(args.WorkingFile);
        var noExtension = FileHelper.GetShortFileNameWithoutExtension(args.WorkingFile);
        if(Guid.TryParse(noExtension, out _))
            filename = FileHelper.GetShortFileNameWithoutExtension(args.LibraryFileName) + FileHelper.GetExtension(args.WorkingFile);
        
        string fullPath = Path.Combine(outputDir, filename);
        args.Logger?.ILog("Full Output Path: " + fullPath);

        var result = args.MoveFile(fullPath);
        if (result.Failed(out var error))
            return args.Fail(error);
        
        return 1;
    }
}