using FileFlows.Plugin;

namespace FileFlows.BasicNodes.SystemElements;

/// <summary>
/// A flow to determine if this is running on Docker or not
/// </summary>
public class IsDocker : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string Group => "System";
    /// <inheritdoc />
    public override string Icon => "fab fa-docker";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/system/is-docker";
    /// <inheritdoc />
    public override string CustomColor => "#2481e4";

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
        => args.IsDocker ? 1 : 2;
}