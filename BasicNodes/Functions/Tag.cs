namespace FileFlows.BasicNodes.Functions;

using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Flow element that Tags a file
/// </summary>
public class Tag : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 1;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string Icon => "fas fa-tag";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/tag"; 
    /// <inheritdoc />
    public override bool FailureNode => true;
    /// <inheritdoc />
    public override LicenseLevel LicenseLevel => LicenseLevel.Basic;
    
    /// <summary>
    /// Gets or sets if the tags should replace the existing tags
    /// </summary>
    [Boolean(1)]
    public bool Replace { get; set; }
    
    /// <summary>
    /// Gets or sets the tags
    /// </summary>
    [TagSelection(2)]
    public List<Guid> Tags { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        args.SetTags(Tags ?? [], Replace);
        return 1;
    }
}
