using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.Functions;

/// <summary>
/// Flow element that moves a file for reprocessing
/// </summary>
public class Reprocess : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 0;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/reprocess";
    /// <inheritdoc /> 
    public override string Icon => "fas fa-redo";
    
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
            args.FailureReason = "Target reprocess node is same as current processing node.";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }
        args.Logger?.ILog("Requesting reprocessing on Node: " + Node.Name);
        args.ReprocessNode = Node;
        
        return 0;
    }
}
