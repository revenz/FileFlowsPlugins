using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.Functions;

/// <summary>
/// A flow element that simply fails a flow
/// </summary>
public class FailFlow : Node
{
    /// <summary>
    /// Gets the number of input nodes
    /// </summary>
    public override int Inputs => 1;
    /// <summary>
    /// Gets the type of node
    /// </summary>
    public override FlowElementType Type => FlowElementType.Logic;
    /// <summary>
    /// Gets the icon of the flow
    /// </summary>
    public override string Icon => "fas fa-exclamation-triangle";
    /// <summary>
    /// Gets the URL for the help page
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/fail-flow";

    /// <inheritdoc />
    public override string CustomColor => "var(--error)";

    /// <summary>
    /// Gets or sets the reason to fail the flow
    /// </summary>
    [Text(1)]
    public string Reason { get; set; }

    /// <summary>
    /// Executes the node
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>-1 to indicate the flow should fail</returns>
    public override int Execute(NodeParameters args)
    {
        if (string.IsNullOrWhiteSpace(Reason) == false)
        {
            args.Logger.ILog("Failing flow: " + Reason);
            args.FailureReason = Reason;
        }
        else
        {
            args.Logger.ILog("Failing flow");
        }

        return -1;
    }
}