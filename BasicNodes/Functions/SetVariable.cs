using System.Text.RegularExpressions;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.Functions;

/// <summary>
/// A flow element that sets a variable in the flow
/// </summary>
public class SetVariable : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 1;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string Icon => "fas fa-at";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/set-variable";

    /// <summary>
    /// Gets or sets the name of the variable to set
    /// </summary>
    [TextVariable(1)]
    public string Variable { get; set; }
    /// <summary>
    /// Gets or sets the value to set
    /// </summary>
    [TextVariable(2)]
    public string Value { get; set; }
    /// <inheritdoc />
    public override bool FailureNode => true;

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        string variable = args.ReplaceVariables(Variable, stripMissing: true);
        if (string.IsNullOrWhiteSpace(variable))
        {
            args.FailureReason = "No variable name defined";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        if (Regex.IsMatch(variable, "^[a-zA-Z_][a-zA-Z0-9_]*$") == false)
        {
            args.FailureReason = "Invalid variable name: " + variable;
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        string value = args.ReplaceVariables(Value, stripMissing: true);
        object valueToUse;
        string lowerValue = value?.ToLowerInvariant()?.Trim() ?? string.Empty;
        if (lowerValue == "true")
        {
            args.Logger?.ILog($"Boolean detected, setting variable '{variable}' to: True");
            valueToUse = (object)true;
        }
        else if (lowerValue == "false")
        {
            args.Logger?.ILog($"Boolean detected, setting variable '{variable}' to: False");
            valueToUse = (object)false;
        }
        else if (Regex.IsMatch(lowerValue, "^[\\d]+$") && int.TryParse(lowerValue, out int iValue))
        {
            args.Logger?.ILog($"Integer detected, setting variable '{variable}' to: " + iValue);
            valueToUse = (object)iValue;
        }
        else if (Regex.IsMatch(lowerValue, "^[\\d]+\\.[\\d]+$") && float.TryParse(lowerValue, out float fValue))
        {
            args.Logger?.ILog($"Float detected, setting variable '{variable}' to: " + fValue);
            valueToUse = (object)fValue;
        }
        else if (Value == null || value == null)
        {
            args.Logger?.ILog($"Null detected, setting variable '{variable}' to: null");
            valueToUse = null;
        }
        else
        {
            args.Logger?.ILog($"String detected, setting variable '{variable}' to: " + Value);
            valueToUse = value;
        }

        args.Variables[variable] = valueToUse;
        
        return 1;
    }
}