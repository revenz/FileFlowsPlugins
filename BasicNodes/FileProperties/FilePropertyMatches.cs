using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.FileProperties;

/// <summary>
/// Flow element that tests if a file property matches a given string
/// </summary>
public class FilePropertyMatches : Node
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
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/file-property-matches"; 

    /// <summary>
    /// Gets or sets the property name
    /// </summary>
    [TextVariable(1)]
    public string Property { get; set; }
    
    /// <summary>
    /// Gets or sets the property value
    /// </summary>
    [TextVariable(2)]
    public string Value { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        string property = args.ReplaceVariables(this.Property ?? string.Empty, stripMissing: true);
        if (string.IsNullOrEmpty(property))
            return args.Fail("No property set");

        string matchExpression = args.ReplaceVariables(this.Value ?? string.Empty, stripMissing: true) ?? string.Empty;
        
        string actualValue = args.GetProperty(property) ?? string.Empty;

        args.Logger?.ILog("Match Expression: " + matchExpression);
        args.Logger?.ILog("Actual Value: " + actualValue);

        bool matches = args.StringHelper.Matches(matchExpression, actualValue);

        args.Logger?.ILog(matches ? "Value matches" : "Value does not match");

        return matches ? 1 : 2;
    }
}
