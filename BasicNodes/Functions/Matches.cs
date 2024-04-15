using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using FileFlows.BasicNodes.Helpers;

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
                var value = args.ReplaceVariables(match.Key, stripMissing: true);
                if (GeneralHelper.IsRegex(match.Value))
                {
                    if (Regex.IsMatch(value, match.Value, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                    {
                        args.Logger?.ILog($"Match found '{match.Value}' = {value}");
                        return output;
                    }
                }

                if (match.Value == value)
                {
                    args.Logger?.ILog($"Match found '{match.Value}' = {value}");
                    return output;
                }

                if (MathHelper.IsMathOperation(match.Value))
                {
                    if (MathHelper.IsTrue(value, match.Value))
                    {
                        args.Logger?.ILog($"Match found '{match.Value}' = {value}");
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
