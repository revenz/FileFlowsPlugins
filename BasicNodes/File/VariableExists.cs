using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.File;

/// <summary>
/// Checks if a file exists
/// </summary>
public class VariableExists: Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string Icon => "fas fa-question";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/variable-exists";
    /// <inheritdoc />
    public override bool NoEditorOnAdd => true;


    /// <summary>
    /// Gets or sets the name of the variable to check
    /// </summary>
    [Text(1)]
    public string Variable { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        string variable = Variable;
        if(string.IsNullOrWhiteSpace(variable))
        {
            args.FailureReason = "Variable not set";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        if (args.Variables.TryGetValue(variable, out var value) == false)
        {
            args.Logger?.ILog("Variable does not exist");
            return 2;
        }
        if (value == null)
        {
            args.Logger?.ILog("Variable exists but is null");
            return 2;
        }
        
        args.Logger?.ILog("Variable exists and is not null");
        return 1;
    }
}