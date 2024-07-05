using FileFlows.Plugin;

namespace FileFlows.BasicNodes.SystemElements;

/// <summary>
/// A flow to determine if this is running on MacOS  or not
/// </summary>
public class IsMacOS : Node
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
    public override string Icon => "fab fa-apple";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/system/is-mac-os";
    /// <inheritdoc />
    public override string CustomColor => "#2481e4";

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
        => OperatingSystem.IsMacOS() ? 1 : 2;
}