using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.Functions;

/// <summary>
/// Flow element that moves process into another flow
/// </summary>
public class GotoFlow : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 0;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/goto-flow";
    /// <inheritdoc /> 
    public override string Icon => "fas fa-sitemap";
    
    /// <summary>
    /// Gets or sets the flow to execute
    /// </summary>
    [Select("FLOW_LIST", 1)]
    public ObjectReference Flow { get; set; }
    
    /// <summary>
    /// Gets or sets if the flow this file is processing with should be updated to this new flow
    /// </summary>
    [Boolean(2)]
    public bool UpdateFlowUsed { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        args.GotoFlow(Flow);
        return 0;
    }
}
