using FileFlows.Plugin;

namespace FileFlows.BasicNodes;

public class FlowFailure: Node
{
    public override int Outputs => 1;
    public override FlowElementType Type => FlowElementType.Failure;
    public override string Icon => "fas fa-exclamation-triangle";
    public override bool FailureNode => true;

    private Dictionary<string, object> _Variables;
    public override Dictionary<string, object> Variables => _Variables;
    public FlowFailure()
    {
        _Variables = new Dictionary<string, object>()
        {
            { "fail.Flow", "The Flow Name" },
            { "fail.Log", "A short tail of the log" },
            { "fail.Message", "A formatted message containing the failure details" },
            { "fail.Node", "FailedNodeName" },
        };
    }
    public override int Execute(NodeParameters args)
    {
        try
        {
            string log = args.Logger?.GetTail(10)?.EmptyAsNull() ?? "No log available";
            string failedNode = args.GetVariable("FailedNode") as string ?? "Unknown";
            string flowName = args.GetVariable("FlowName") as string ?? "Unknown";

            string message = @$"Failed processing file {args.FileName}
Flow: {flowName}
Node: {failedNode}
{log}";

            Variables.AddOrUpdate("fail.Node", failedNode);
            Variables.AddOrUpdate("fail.Flow", flowName);
            Variables.AddOrUpdate("fail.Message", message);
            Variables.AddOrUpdate("fail.Log", log);
            args.UpdateVariables(Variables);
            return 1;
        }
        catch (Exception)
        {
            return 0; // special case, we never want to report an error here
        }
    }
}
