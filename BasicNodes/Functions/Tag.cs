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
    /// Gets or sets the tags
    /// </summary>
    [TagSelection(1)]
    public List<Guid> Tags { get; set; }
    
    /// <summary>
    /// Gets or sets if the tags should replace the existing tags
    /// </summary>
    [Boolean(2)]
    public bool Replace { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        if (Tags == null || Tags.Count == 0)
        {
            args.FailureReason = "No tags selected";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }
        
        // if (Replace)
        //     args.File.Tags = Tags;
        // else
        //     args.File.Tags.AddRange(Tags);
        return 1;
    }
}
