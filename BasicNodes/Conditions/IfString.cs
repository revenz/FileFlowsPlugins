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
        args?.Logger?.ILog("Value is: " + (value == null ? "null" : value.ToString()));
        
        if (Options?.Any() != true)
        {
            args.FailureReason = "No Options configured";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        for(int i=0;i<Options.Length;i++)
        {
            args.Logger?.ILog($"Option[{i}] = {(Options[i] == null ? "null" : Options[i].ToString())}");
        }

        var strValue = value?.ToString() ?? string.Empty;
        
        var index = Options.ToList().FindIndex(x => x?.EmptyAsNull() == strValue?.EmptyAsNull());
        args.Logger?.ILog("Index of option: " + index);
        if (index >= 0)
            return index + 1;
        
        // index is then -1
        args.Logger?.ELog("Failed to locate value in options");
        args.FailureReason = "Failed to locate value in options: " + strValue;
        return index;
    }
}