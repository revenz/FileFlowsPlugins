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
            args.FailureReason = "No varaible defined to match against";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        string test=  args.ReplaceVariables(Input, stripMissing: true);
        bool matches = args.MatchesVariable(variableName, test);
        return matches ? 1 : 2;
    }
}
