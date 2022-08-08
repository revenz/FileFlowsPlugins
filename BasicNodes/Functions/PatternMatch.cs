namespace FileFlows.BasicNodes.Functions;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
public class PatternMatch : Node
{
    public override int Inputs => 1;
    public override int Outputs => 2;
    public override FlowElementType Type => FlowElementType.Logic;
    public override string Icon => "fas fa-equals";
    public override string HelpUrl => "https://docs.fileflows.com/plugins/basic-nodes/pattern-match";

    private Dictionary<string, object> _Variables;
    public override Dictionary<string, object> Variables => _Variables;
    public PatternMatch()
    {
        _Variables = new Dictionary<string, object>()
        {
            { "PatternMatch", "match from pattern" }
        };
    }

    [DefaultValue("")]
    [Text(1)]
    [Required]
    public string Pattern { get; set; }

    public override int Execute(NodeParameters args)
    {
        if (string.IsNullOrEmpty(Pattern))
            return 1; // no pattern, matches everything

        try
        {
            var rgx = new Regex(Pattern);
            if (rgx.IsMatch(args.WorkingFile))
            {
                args.Variables.Add("PatternMatch", rgx.Match(args.WorkingFile).Value);
                return 1;
            }
            if (rgx.IsMatch(args.FileName))
            {
                args.Variables.Add("PatternMatch", rgx.Match(args.FileName).Value);
                return 1;
            }
            return 2;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Pattern error: " + ex.Message);
            return -1;
        }
    }
}
