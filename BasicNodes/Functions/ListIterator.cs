using System.ComponentModel;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using System.ComponentModel.DataAnnotations;

namespace FileFlows.BasicNodes.Functions;

/// <summary>
/// A special flow element that iterates all strings in a list and process them through a sub flow
/// </summary>
public class ListIterator : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 1;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/list-iterator";
    /// <inheritdoc /> 
    public override string Icon => "fas fa-sitemap";
    /// <inheritdoc /> 
    public override string CustomColor => "var(--flow-subflow)";

    /// <summary>
    /// Gets or sets the flow to execute
    /// </summary>
    [Select("SUB_FLOW_LIST", 1)]
    public ObjectReference? Flow { get; set; }
    
    /// <summary>
    /// Gets or sets the list to iterate
    /// </summary>
    [Required]
    [TextVariable(2)]
    [DefaultValue("{CurrentList}")]
    public string? List { get; set; }
    
    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        args.Logger?.ELog("Should not be called here, but directly from the runner.");
        return -1;
    }
}