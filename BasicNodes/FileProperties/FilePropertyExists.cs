using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.FileProperties;

/// <summary>
/// Flow element that tests if a file property exists
/// </summary>
public class FilePropertyExists : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc />
    public override string Icon => "fas fa-question";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/file-property-exists"; 

    /// <summary>
    /// Gets or sets the property name
    /// </summary>
    [TextVariable(1)]
    public string Property { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        string property = args.ReplaceVariables(this.Property ?? string.Empty, stripMissing: true);
        if (string.IsNullOrEmpty(property))
            return args.Fail("No property set");
        
        string actualValue = args.GetProperty(property);
        bool exists = string.IsNullOrWhiteSpace(actualValue) == false;
        args.Logger?.ILog(exists ? "Property exists" : "Property does not exist");
        return exists ? 1 : 2;
    }
}
