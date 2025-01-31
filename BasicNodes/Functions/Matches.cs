using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FileFlows.BasicNodes.Functions;

/// <summary>
/// A flow element that matches different conditions
/// </summary>
public class Matches : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string Icon => "fas fa-equals";
    /// <inheritdoc />
    public override bool FailureNode => true;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/matches";

    /// <summary>
    /// Gets or sets replacements to replace
    /// </summary>
    [KeyValue(1, showVariables: true, allowDuplicates: true)]
    [Required]
    public List<KeyValuePair<string, string>> MatchConditions { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        if (MatchConditions?.Any() != true)
        {
            args.FailureReason = "No matches defined";
            args.Logger.ELog(args.FailureReason);
            return -1;
        }

        int output = 0;
        foreach (var match in MatchConditions)
        {
            output++;
            try
            {
                // the value is what we will test
                object value;
                // first try to see if the key is a Variable, and if it is get the variables value
                if (Regex.IsMatch(match.Key, @"\{[\w\d\.-]+\}") &&
                    args.Variables.TryGetValue(match.Key[1..^1], out var varValue))
                    value = varValue;
                else
                {
                    // else, its not a variable, but it may contain a variable
                    value = args.ReplaceVariables(match.Key, stripMissing: true);
                }
                
                if (args.MathHelper.IsMathOperation(match.Value))
                {
                    string strValue = value?.ToString()?.Trim() ?? string.Empty;
                    if (args.MathHelper.IsTrue(match.Value, strValue))
                    {
                        args.Logger?.ILog($"Match found '{match.Value}' = {strValue}");
                        return output;
                    }
                }
                
                if (args.StringHelper.Matches(match.Value, value))
                    return output;
            }
            catch (Exception)
            {
                // Ignored
            }
        }

        args.Logger?.ILog("No matches found");
        return MatchConditions.Count + 1;
    }
}
