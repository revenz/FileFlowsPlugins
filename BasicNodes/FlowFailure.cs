using FileFlows.Plugin;
using System.Text.RegularExpressions;

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
            { "fail.LogNoDates", "A short tail of the log" },
            { "fail.Message", "A formatted message containing the failure details" },
            { "fail.MessageNoDates", "A formatted message containing the failure details" },
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

            var rgxDateTime = new Regex(@"^[\d]{4}-[\d]{2}\-[\d]{2} [\d]{2}:[\d]{2}:[\d]{2}\.[\d]+ - ");
            string logNoDates = string.Join(Environment.NewLine, log.Replace("\r\n", "\n").Split('\n').Select(x => rgxDateTime.Replace(x, "")));

            string message = @$"Failed processing file {args.FileName}
Flow: {flowName}
Node: {failedNode}
{log}";
            string messageNoDates = @$"Failed processing file {args.FileName}
Flow: {flowName}
Node: {failedNode}
{logNoDates}";

            Variables["fail.Node"] = failedNode;
            Variables["fail.Flow"] = flowName;
            Variables["fail.Message"] = message;
            Variables["fail.MessageNoDates"] = messageNoDates;
            Variables["fail.Log"] = log;
            Variables["fail.LogNoDates"] = logNoDates;
            args.UpdateVariables(Variables);
            return 1;
        }
        catch (Exception)
        {
            return 0; // special case, we never want to report an error here
        }
    }
}
