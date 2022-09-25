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
    public override int Inputs => 1;
    public override int Outputs => 2;

    public override FlowElementType Type => FlowElementType.Logic;
    public override string HelpUrl => "https://docs.fileflows.com/plugins/basic-nodes/variable-match";
    public override string Icon => "fas fa-equals";

    [Required]
    [Select("VARIABLE_LIST", 1)]
    public ObjectReference Variable { get; set; }

    [Required]
    [TextVariable(2)]
    public string Input { get; set; }



    public override int Execute(NodeParameters args)
    {
        string test= args.ReplaceVariables(Input, stripMissing: true);
        bool matches = args.MatchesVariable(Variable.Name, test);
        return matches ? 1 : 2;
    }
}
