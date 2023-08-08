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
    protected override bool DoCheck(NodeParameters args, object value)
    {
        if (value == null) return false;
        if (value is bool bValue) return bValue;
        if (value is string sValue) return sValue.ToLowerInvariant() == "true" || sValue.ToLowerInvariant() == "1";
        if (value is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.False) return false;
            if (je.ValueKind == JsonValueKind.True) return true;
            if (je.ValueKind == JsonValueKind.Null) return false;
            if (je.ValueKind == JsonValueKind.Number) return je.GetInt32() > 0;
            if (je.ValueKind == JsonValueKind.String) return je.GetString().ToLowerInvariant() == "true";
        }
        args?.Logger?.ILog("Value was an unexpected type: " + value.GetType().FullName);
        return false;
    }
}