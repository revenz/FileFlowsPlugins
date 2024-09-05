using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.Functions;

/// <summary>
/// Flow element that matches the working file or the original file against a regular expression pattern 
/// </summary>
public class PatternMatch : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string Icon => "fas fa-equals";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/pattern-match";
    /// <inheritdoc />
    public override bool FailureNode => true;

    private Dictionary<string, object> _Variables;
    /// <inheritdoc />
    public override Dictionary<string, object> Variables => _Variables;
    
    /// <summary>
    /// Constructs a new instance of the Pattern Match object
    /// </summary>
    public PatternMatch()
    {
        _Variables = new Dictionary<string, object>()
        {
            { "PatternMatch", "match from pattern" }
        };
    }

    /// <summary>
    /// Gets or sets the pattern to match against
    /// </summary>
    [DefaultValue("")]
    [Text(1)]
    [Required]
    public string Pattern { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        if (string.IsNullOrEmpty(Pattern))
            return 1; // no pattern, matches everything

        try
        {
            var rgx = new Regex(Pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            if (rgx.IsMatch(args.WorkingFile))
            {
                args.Variables["PatternMatch"] = rgx.Match(args.WorkingFile).Value;
                return 1;
            }
            if (rgx.IsMatch(args.FileName))
            {
                args.Variables["PatternMatch"] = rgx.Match(args.FileName).Value;
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
