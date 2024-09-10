using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileFlows.BasicNodes.Functions;

/// <summary>
/// Tests if an input value is matched by a variable
/// </summary>
public class VariableMatch : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/variable-match";
    /// <inheritdoc />
    public override string Icon => "fas fa-equals";
    /// <inheritdoc />
    public override bool FailureNode => true;

    /// <summary>
    /// Gets or sets the variable to match
    /// </summary>
    public ObjectReference Variable { get; set; }
    
    /// <summary>
    /// Gets or sets the variable to match
    /// </summary>
    [Required]
    [Combobox("VARIABLE_LIST", 1)]
    public string VariableName { get; set; }

    /// <summary>
    /// Gets or sets the value to match
    /// </summary>
    [Required]
    [TextVariable(2)]
    public string Input { get; set; }


    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        string variableName = VariableName?.EmptyAsNull() ?? Variable?.Name?.EmptyAsNull() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(variableName))
        {
            args.FailureReason = "No variable defined to match against";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }
        string test = args.ReplaceVariables(Input, stripMissing: true);

        args.Logger.ILog("Variable: " + variableName);
        args.Logger.ILog("Test Value: " + test);

        if (args.Variables.TryGetValue(variableName, out object variable) == false)
        {
            args.Logger?.ILog("Variable not found");
            return 2;
        }
        
        var variableString = variable?.ToString();

        if (string.IsNullOrWhiteSpace(variableString))
        {
            args.Logger?.ILog("Variable had no appropriate string value");
            return 2;
        }
        
        if (args.MathHelper.IsMathOperation(test) && double.TryParse(variableString, out var dbl))
        {
            args.Logger?.ILog("Testing math operation: " + test);
            
            bool mathMatches = args.MathHelper.IsTrue(test, dbl);
            return mathMatches ? 1 : 2;
        }
        
        bool matches = args.StringHelper.Matches(test, variableString);
        return matches ? 1 : 2;
    }
}
