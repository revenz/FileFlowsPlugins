using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.File;

/// <summary>
/// Tests if a filename matches the given vlaue
/// </summary>
public class FileNameMatches: Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string Icon => "fas fa-equals";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/file-name-matches";
    /// <inheritdoc />
    public override bool FailureNode => true;

    /// <summary>
    /// Gets or sets the value to match against
    /// </summary>
    [TextVariable(1)]
    [Required]
    public string Value { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var value = args.ReplaceVariables(Value, stripMissing: true);
        var matches = args.StringHelper.Matches(args.LibraryFileName, Value);
        if (matches)
        {
            args.Logger?.ILog("Matches");
            return 1;
        }

        args.Logger?.ILog("Does not match");
        return 2;
    }
}