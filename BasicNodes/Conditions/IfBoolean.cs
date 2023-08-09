using System.Text.Json;
using FileFlows.Plugin;

namespace FileFlows.BasicNodes.Conditions;

/// <summary>
/// A flow element that test if a boolean is true
/// </summary>
public class IfBoolean: IfBase
{
    /// <summary>
    /// Gets or sets the URL to the help page
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/conditions/if-boolean";

    /// <summary>
    /// Checks the variable value
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <param name="value">the variable value</param>
    /// <returns>true if matches, otherwise false</returns>
    protected override int DoCheck(NodeParameters args, object value)
    {
        if (value == null) return 2;
        if (value is bool bValue) return bValue ? 1 : 2;
        if (value is string sValue) return sValue.ToLowerInvariant() == "true" || sValue.ToLowerInvariant() == "1" ? 1 : 2;
        if (value is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.False) return 2;
            if (je.ValueKind == JsonValueKind.True) return 1;
            if (je.ValueKind == JsonValueKind.Null) return 2;
            if (je.ValueKind == JsonValueKind.Number) return je.GetInt32() > 0 ? 1 : 2;
            if (je.ValueKind == JsonValueKind.String) return je.GetString().ToLowerInvariant() == "true" ? 1 : 2;
        }
        args?.Logger?.ILog("Value was an unexpected type: " + value.GetType().FullName);
        return 2;
    }
}