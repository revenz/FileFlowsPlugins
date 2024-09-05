using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.Functions;


/// <summary>
/// A node that choose a random output
/// </summary>
public class Random : Node
{
    /// <summary>
    /// Gets the number of input nodes
    /// </summary>
    public override int Inputs => 1;

    /// <summary>
    /// Gets or sets the number of outputs
    /// </summary>
    [DefaultValue(3)]
    [NumberInt(1)]
    [Range(2, 10)]
    public new int Outputs { get; set; }
    
    /// <summary>
    /// Gets the type of node
    /// </summary>
    public override FlowElementType Type => FlowElementType.Logic;
    /// <summary>
    /// Gets the icon of the flow
    /// </summary>
    public override string Icon => "fas fa-dice";
    /// <summary>
    /// Gets the URL for the help page
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/random";
    /// <inheritdoc />
    public override bool FailureNode => true;

    /// <summary>
    /// Executes the node
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>-1 to indicate the flow should fail</returns>
    public override int Execute(NodeParameters args)
    {
        var rand = new System.Random(DateTime.UtcNow.Millisecond);
        int output = rand.Next(1, Outputs + 1);
        args.Logger?.ILog("Random output selected: " + output);
        return output;
    }
}