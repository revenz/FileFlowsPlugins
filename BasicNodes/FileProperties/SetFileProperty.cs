using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.FileProperties;

/// <summary>
/// Flow element that sets a file property
/// </summary>
public class SetFileProperty : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 1;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string Icon => "fas fa-file-signature";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/set-file-property"; 

    /// <summary>
    /// Gets or sets the property name
    /// </summary>
    [TextVariable(1)]
    public string Property { get; set; }
    
    /// <summary>
    /// Gets or sets the property value
    /// </summary>
    [TextVariable(2)]
    public string Value { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        string property = args.ReplaceVariables(this.Property ?? string.Empty, stripMissing: true);
        if (string.IsNullOrEmpty(property))
            return args.Fail("No property set");

        string value = args.ReplaceVariables(this.Value ?? string.Empty, stripMissing: true)?.EmptyAsNull();

        args.SetProperty(property, value);
        
        return 1;
    }
}
