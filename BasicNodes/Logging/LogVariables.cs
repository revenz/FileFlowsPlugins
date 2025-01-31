using System.Collections;
using System.Text;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.Logging;

/// <summary>
/// Flow that logs all variables
/// </summary>
public class LogVariables: Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 1;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string Icon => "fas fa-at";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/log-variables"; 
    /// <inheritdoc />
    public override bool FailureNode => true;

    /// <inheritdoc />
    public override string Group => "Logging";
    
    /// <summary>
    /// Gets or sets if the logging will be recursive
    /// </summary>
    [Boolean(1)]
    public bool Recursive { get; set; }
    
    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var log = GetVariablesString(args.Variables, Recursive);
        args.Logger?.ILog("Variables: \n" + log);
        return 1;
    }

    /// <summary>
    /// Generates a string representation of variables to be logged.
    /// </summary>
    /// <param name="variables">The variables to log.</param>
    /// <param name="recursive">Indicates whether to log variables recursively.</param>
    /// <returns>A formatted string of variables.</returns>
    internal static string GetVariablesString(Dictionary<string, object> variables, bool recursive)
    {
        StringBuilder log = new();

        foreach (var variable in variables)
        {
            // Only log the variable's key and value for simple types (non-recursive)
            string valueRepresentation = recursive 
                ? AppendComplexObject(variable.Key, variable.Value) 
                : FormatValue(variable.Value);

            if (recursive)
            {
                log.AppendLine(valueRepresentation);
            }
            else
            {
                // For non-recursive, log only the key and value
                log.AppendLine($"{variable.Key}: {valueRepresentation}");
            }
        }

        return log.ToString();
    }

    /// <summary>
    /// Formats a value as a string.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>A formatted string representation of the value.</returns>
    private static string FormatValue(object value)
    {
        return value switch
        {
            null => "null",
            IEnumerable enumerable when value is not string => $"[{string.Join(", ", enumerable.Cast<object>())}]",
            _ => value.ToString()
        };
    }

    /// <summary>
    /// Recursively formats complex objects into a log-friendly string.
    /// </summary>
    /// <param name="key">The key or name of the variable.</param>
    /// <param name="value">The value of the variable.</param>
    /// <returns>A string representation of the complex object.</returns>
    private static string AppendComplexObject(string key, object value)
    {
        if (value == null)
            return "null";

        StringBuilder log = new();

        // Handle IDictionary (Dictionary)
        if (value is IDictionary dictionary)
        {
            foreach (var dictKey in dictionary.Keys)
            {
                var dictValue = dictionary[dictKey];
                // Recursively call for dictionary items
                log.AppendLine(AppendComplexObject($"{key}.{dictKey}", dictValue));
            }
        }
        // Handle IEnumerable (Collections like List or Array)
        else if (value is IEnumerable enumerable && value is not string)
        {
            // Log list or collection elements
            var values = string.Join(", ", enumerable.Cast<object>());
            log.AppendLine($"{key}: [{values}]");
        }
        // Handle Class types (Non-string objects)
        else if (value.GetType().IsClass && value.GetType() != typeof(string))
        {
            foreach (var property in value.GetType().GetProperties())
            {
                var propertyValue = property.GetValue(value);
                // Recursively call for class properties
                log.AppendLine(AppendComplexObject($"{key}.{property.Name}", propertyValue));
            }
        }
        else
        {
            // Base case: simple value
            log.AppendLine($"{key}: {value}");
        }

        return log.ToString().TrimEnd();
    }

}