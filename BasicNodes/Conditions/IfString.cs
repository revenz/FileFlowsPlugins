using System.ComponentModel;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.Conditions;

/// <summary>
/// A flow element that test if a string matches another string
/// </summary>
public class IfString: IfBase
{
    /// <summary>
    /// Gets or sets the URL to the help page
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/conditions/if-string";

    /// <summary>
    /// Gets the number of outputs
    /// </summary>
    [DefaultValue(2)]
    [NumberInt(2)]
    public new int Outputs { get; set; }

    /// <summary>
    /// Gets or sets the options the variable can match
    /// </summary>
    [StringArray(3)]
    public string[] Options { get; set; }

    /// <summary>
    /// Checks the variable value
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <param name="value">the variable value</param>
    /// <returns>true if matches, otherwise false</returns>
    protected override int DoCheck(NodeParameters args, object value)
    {
        if (Options?.Any() != true) return -1;
        var index = Options.ToList().FindIndex(x => x == value as string);
        if (index > 0)
            return index + 1;
        return index;
    }
}