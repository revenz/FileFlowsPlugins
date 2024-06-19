using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using FileFlows.BasicNodes.Helpers;
using FileFlows.Plugin.Helpers;

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
                object value;
                if (Regex.IsMatch(match.Key, @"\{[\w\d\.-]+\}") &&
                    args.Variables.TryGetValue(match.Key[1..^1], out var varValue))
                    value = varValue;
                else
                    value = args.ReplaceVariables(match.Key, stripMissing: true);
                string strValue = value?.ToString() ?? string.Empty;
                
                if (GeneralHelper.IsRegex(match.Value))
                {
                    if (Regex.IsMatch(strValue, match.Value, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                    {
                        args.Logger?.ILog($"Match found '{match.Value}' = {value}");
                        return output;
                    }
                }

                if (Regex.IsMatch(match.Value ??string.Empty, "^(true|1)$", RegexOptions.IgnoreCase) &&
                    Regex.IsMatch(strValue, "^(true|1)$", RegexOptions.IgnoreCase))
                {
                    args.Logger?.ILog($"Match found '{match.Value}' = {strValue}");
                    return output;
                }
                if (Regex.IsMatch(match.Value ??string.Empty, "^(false|0)$", RegexOptions.IgnoreCase) &&
                    Regex.IsMatch(strValue, "^(false|0)$", RegexOptions.IgnoreCase))
                {
                    args.Logger?.ILog($"Match found '{match.Value}' = {strValue}");
                    return output;
                }

                if (match.Value == strValue)
                {
                    args.Logger?.ILog($"Match found '{match.Value}' = {strValue}");
                    return output;
                }

                if (args.MathHelper.IsMathOperation(match.Value))
                {
                    if (args.MathHelper.IsTrue(match.Value, strValue))
                    {
                        args.Logger?.ILog($"Match found '{match.Value}' = {strValue}");
                        return output;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        args.Logger?.ILog("No matches found");
        return MatchConditions.Count + 1;
    }
}
