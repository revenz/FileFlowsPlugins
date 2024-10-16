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
    [Select("NODE_LIST_ANY", 1)]
    public ObjectReference? Node { get; set; }
    
    /// <summary>
    /// Gets or sets the number of minutes to hold the file for reprocessing
    /// </summary>
    [NumberInt(1)]
    public int? HoldMinutes { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        bool holding = HoldMinutes is > 0;
        bool onNode = Node is not null && Node.Uid != Guid.Empty && Node.Uid != args.Node.Uid;
        if (holding == false && onNode == false)
        {
            args.FailureReason = "Must select at least one of Hold Minutes or Reprocess Node.";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        if (holding)
        {
            args.Logger?.ILog($"Holding for {HoldMinutes} minutes");
            args.Reprocess.HoldForMinutes = HoldMinutes;
        }

        if (onNode)
        {
            args.Logger?.ILog($"Reprocessing on node '{Node.Name}'");
            args.ReprocessNode = Node;
        }

        return 1;
    }
}
