using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FileFlows.BasicNodes.Functions;

/// <summary>
/// A flow element that matches all conditions
/// </summary>
public class MatchesAll : Node
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
    public override bool FailureNode => true;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/matches-all";

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

        foreach (var match in MatchConditions)
        {
            try
            {
                object value;
                if (Regex.IsMatch(match.Key, @"\{[\w\d\.-]+\}") &&
                    args.Variables.TryGetValue(match.Key[1..^1], out var varValue))
                    value = varValue;
                else
                    value = args.ReplaceVariables(match.Key, stripMissing: true);
                
                if (args.MathHelper.IsMathOperation(match.Value))
                {
                    string strValue = value?.ToString()?.Trim() ?? string.Empty;
                    if (args.MathHelper.IsFalse(match.Value, strValue))
                    {
                        args.Logger?.ILog($"Did not match found '{match.Value}' = {strValue}");
                        return 2;
                    }
                    args.Logger?.ILog($"Did match found '{match.Value}' = {strValue}");
                    continue;
                }

                if (args.StringHelper.Matches(match.Value, value) == false)
                {
                    args.Logger?.ILog($"Did not match found '{match.Value}' = {value}");
                    return 2;
                }
                
                args.Logger?.ILog($"Did match found '{match.Value}' = {value}");
            }
            catch (Exception ex)
            {
                // Ignored
                args.Logger?.ILog($"Error matching match found '{match.Value}': {ex.Message}");
                return 2;
            }
        }

        args.Logger?.ILog("All matches passed");
        return 1;
    }
}
