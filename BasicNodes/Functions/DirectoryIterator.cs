using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using System.ComponentModel.DataAnnotations;

namespace FileFlows.BasicNodes.Functions;

/// <summary>
/// A special flow element that iterates all files in a directory and process them through a sub flow
/// </summary>
public class DirectoryIterator : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 1;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/directory-iterator";
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
    /// Gets or sets the directory to iterate
    /// </summary>
    [Required]
    [Folder(2)]
    public string? Directory { get; set; }
    
    /// <summary>
    /// Gets or set the pattern to iterate over
    /// </summary>
    [Text(3)]
    public string? Pattern { get; set; }

    /// <summary>
    /// Gets or sets if all files or just the top directory files should be iterated over
    /// </summary>
    [Boolean(4)]
    public bool Recursive { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        args.Logger?.ELog("Should not be called here, but directly from the runner.");
        return -1;
    }
}