using FileFlows.Plugin;

namespace FileFlows.BasicNodes.Functions;

/// <summary>
/// A flow element that simply completes/finishes a flow
/// </summary>
public class CompleteFlow : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string Icon => "fas fa-check-square";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/complete-flow";

    /// <inheritdoc />
    public override string CustomColor => "var(--success)";

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
        => 0;
}