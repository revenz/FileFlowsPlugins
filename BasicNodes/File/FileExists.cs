using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.File;

/// <summary>
/// Checks if a file exists
/// </summary>
public class FileExists: Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string Icon => "fas fa-question-circle";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/file-exists";
    /// <inheritdoc />
    public override bool NoEditorOnAdd => true;


    /// <summary>
    /// Gets or sets the name of the file to check
    /// Leave blank to test the working file
    /// </summary>
    [TextVariable(1)]
    public string FileName { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        string file = args.ReplaceVariables(FileName ?? string.Empty, true)?.EmptyAsNull() ?? args.WorkingFile;
        if(string.IsNullOrWhiteSpace(file))
        {
            args.FailureReason = "FileName not set";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }
        try
        {
            var result = args.FileService.FileExists(file);
            if (result.Is(true))
            {
                args.Logger?.ILog("File does exist: " + file);
                return 1;
            }
            args.Logger?.ILog("File does NOT exist: " + file);
            return 2;
        }
        catch (Exception ex)
        {
            args.FailureReason = $"Failed testing if file '{file}' exists: " + ex.Message;
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }
    }
}