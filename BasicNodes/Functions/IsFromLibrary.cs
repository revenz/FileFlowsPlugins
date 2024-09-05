using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.Functions;

/// <summary>
/// Flow element that checks if a file is from the specified library
/// </summary>
public class IsFromLibrary : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/is-from-library";
    /// <inheritdoc /> 
    public override string Icon => "fas fa-question";
    
    /// <summary>
    /// Gets or sets the library to test for
    /// </summary>
    [Select("LIBRARY_LIST", 1)]
    public ObjectReference Library { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        if (args.Library == null)
        {
            args.FailureReason = "Library is null in Flow";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }
        if (Library.Uid == args.Library.Uid)
        {
            args.Logger?.ILog("Is from library: " + args.Library.Name);
            return 1;
        }

        args.Logger?.ILog("Is not from library: " + args.Library.Name);
        return 2;
    }
}
