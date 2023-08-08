using System.ComponentModel.DataAnnotations;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.Conditions;

/// <summary>
/// Base class for the If flow elements
/// </summary>
public abstract class IfBase : Node
{
    /// <summary>
    /// Gets or sets the number of inputs
    /// </summary>
    public override int Inputs => 1;
    /// <summary>
    /// Gets or sets the number of outputs
    /// </summary>
    public override int Outputs => 2;
    /// <summary>
    /// Gets or sets the flow element type
    /// </summary>
    public override FlowElementType Type => FlowElementType.Logic;
    /// <summary>
    /// Gets or sets the icon
    /// </summary>
    public override string Icon => "fas fa-question";
    
    /// <summary>
    /// Gets or sets the variable name
    /// </summary>
    [Text(1)]
    [Required]
    public string Variable { get; set; }

    /// <summary>
    /// Test the variable value matches
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <param name="value">the variable value</param>
    /// <returns>true if matches, otherwise false</returns>
    protected abstract bool DoCheck(NodeParameters args, object value);

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output node</returns>
    public override int Execute(NodeParameters args)
    {
        if (string.IsNullOrWhiteSpace(Variable))
        {
            args.Logger?.WLog("Variable not set");
            return 2;
        }

        var value = args.GetVariable(Variable);
        if (value == null)
        {
            args.Logger?.WLog($"Variable '{Variable}' was null.");
            return 2;
        }

        bool matches = DoCheck(args, value);
        return matches ? 1 : 2;
    }
}