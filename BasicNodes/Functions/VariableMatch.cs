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

    [Required]
    [Select("VARIABLE_LIST", 1)]
    public ObjectReference Variable { get; set; }

    [Required]
    [TextVariable(2)]
    public string Input { get; set; }



    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        string test= args.ReplaceVariables(Input, stripMissing: true);
        bool matches = args.MatchesVariable(Variable.Name, test);
        return matches ? 1 : 2;
    }
}
