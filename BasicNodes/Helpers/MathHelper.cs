using System.Globalization;

namespace FileFlows.BasicNodes.Helpers;

/// <summary>
/// Helper for math operations
/// </summary>
public class MathHelper
{
    /// <summary>
    /// Checks if the comparison string represents a mathematical operation.
    /// </summary>
    /// <param name="comparison">The comparison string to check.</param>
    /// <returns>True if the comparison is a mathematical operation, otherwise false.</returns>
    public static bool IsMathOperation(string comparison)
    {
        // Check if the comparison string starts with <=, <, >, >=, ==, or =
        return new[] { "<=", "<", ">", ">=", "==", "=" }.Any(comparison.StartsWith);
    }
    
    
    /// <summary>
    /// Tests if a math operation is true
    /// </summary>
    /// <param name="value">The value to apply the operation to.</param>
    /// <param name="operation">The operation string representing the mathematical operation.</param>
    /// <returns>True if the mathematical operation is successful, otherwise false.</returns>
    public static bool IsTrue(string value, string operation)
    {
        // This is a basic example; you may need to handle different operators
        switch (operation[..2])
        {
            case "<=":
                return Convert.ToDouble(value) <= Convert.ToDouble(AdjustComparisonValue(operation[2..].Trim()));
            case ">=":
                return Convert.ToDouble(value) >= Convert.ToDouble(AdjustComparisonValue(operation[2..].Trim()));
            case "==":
                return Math.Abs(Convert.ToDouble(value) - Convert.ToDouble(AdjustComparisonValue(operation[2..].Trim()))) < 0.05f;
            case "!=":
            case "<>":
                return Math.Abs(Convert.ToDouble(value) - Convert.ToDouble(AdjustComparisonValue(operation[2..].Trim()))) > 0.05f;
        }

        switch (operation[..1])
        {
            case "<":
                return Convert.ToDouble(value) < Convert.ToDouble(AdjustComparisonValue(operation[1..].Trim()));
            case ">":
                return Convert.ToDouble(value) > Convert.ToDouble(AdjustComparisonValue(operation[1..].Trim()));
            case "=":
                return Math.Abs(Convert.ToDouble(value) - Convert.ToDouble(AdjustComparisonValue(operation[1..].Trim()))) < 0.05f;
        }

        return false;
    }
    
    /// <summary>
    /// Adjusts the comparison string by handling common mistakes in units and converting them into full numbers.
    /// </summary>
    /// <param name="comparisonValue">The original comparison string to be adjusted.</param>
    /// <returns>The adjusted comparison string with corrected units or the original comparison if no adjustments are made.</returns>
    private static string AdjustComparisonValue(string comparisonValue)
    {
        if (string.IsNullOrWhiteSpace(comparisonValue))
            return string.Empty;
        
        string adjustedComparison = comparisonValue.ToLower().Trim();

        // Handle common mistakes in units
        if (adjustedComparison.EndsWith("mbps"))
        {
            // Make an educated guess for Mbps to kbps conversion
            return adjustedComparison[..^4] switch
            {
                { } value when double.TryParse(value, out var numericValue) => (numericValue * 1_000_000)
                    .ToString(CultureInfo.InvariantCulture),
                _ => comparisonValue
            };
        }
        if (adjustedComparison.EndsWith("kbps"))
        {
            // Make an educated guess for kbps to bps conversion
            return adjustedComparison[..^4] switch
            {
                { } value when double.TryParse(value, out var numericValue) => (numericValue * 1_000)
                    .ToString(CultureInfo.InvariantCulture),
                _ => comparisonValue
            };
        }
        if (adjustedComparison.EndsWith("kb"))
        {
            return adjustedComparison[..^2] switch
            {
                { } value when double.TryParse(value, out var numericValue) => (numericValue * 1_000 )
                    .ToString(CultureInfo.InvariantCulture),
                _ => comparisonValue
            };
        }
        if (adjustedComparison.EndsWith("mb"))
        {
            return adjustedComparison[..^2] switch
            {
                { } value when double.TryParse(value, out var numericValue) => (numericValue * 1_000_000 )
                    .ToString(CultureInfo.InvariantCulture),
                _ => comparisonValue
            };
        }
        if (adjustedComparison.EndsWith("gb"))
        {
            return adjustedComparison[..^2] switch
            {
                { } value when double.TryParse(value, out var numericValue) => (numericValue * 1_000_000_000 )
                    .ToString(CultureInfo.InvariantCulture),
                _ => comparisonValue
            };
        }
        if (adjustedComparison.EndsWith("tb"))
        {
            return adjustedComparison[..^2] switch
            {
                { } value when double.TryParse(value, out var numericValue) => (numericValue * 1_000_000_000_000)
                    .ToString(CultureInfo.InvariantCulture),
                _ => comparisonValue
            };
        }

        if (adjustedComparison.EndsWith("kib"))
        {
            return adjustedComparison[..^3] switch
            {
                { } value when double.TryParse(value, out var numericValue) => (numericValue * 1_024 )
                    .ToString(CultureInfo.InvariantCulture),
                _ => comparisonValue
            };
        }
        if (adjustedComparison.EndsWith("mib"))
        {
            return adjustedComparison[..^3] switch
            {
                { } value when double.TryParse(value, out var numericValue) => (numericValue * 1_048_576 )
                    .ToString(CultureInfo.InvariantCulture),
                _ => comparisonValue
            };
        }
        if (adjustedComparison.EndsWith("gib"))
        {
            return adjustedComparison[..^3] switch
            {
                { } value when double.TryParse(value, out var numericValue) => (numericValue * 1_099_511_627_776 )
                    .ToString(CultureInfo.InvariantCulture),
                _ => comparisonValue
            };
        }
        if (adjustedComparison.EndsWith("tib"))
        {
            return adjustedComparison[..^3] switch
            {
                { } value when double.TryParse(value, out var numericValue) => (numericValue * 1_000_000_000_000)
                    .ToString(CultureInfo.InvariantCulture),
                _ => comparisonValue
            };
        }
        return comparisonValue;
    }

}