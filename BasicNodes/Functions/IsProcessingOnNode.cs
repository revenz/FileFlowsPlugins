using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.Functions;

/// <summary>
/// Flow element that checks if a flow is being executed on a specific processing node
/// </summary>
public class IsProcessingOnNode : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/is-processing-on-node";
    /// <inheritdoc /> 
    public override string Icon => "fas fa-question";
    
    /// <summary>
    /// Gets or sets the flow to execute
    /// </summary>
    [Select("NODE_LIST", 1)]
    public ObjectReference Node { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        if (Node.Uid == args.Node.Uid)
        {
            args.Logger?.ILog("Is processing on node: " + args.Node.Name);
            return 1;
        }

        args.Logger?.ILog("Is not processing on node: " + Node.Name);
        return 2;
    }
}
