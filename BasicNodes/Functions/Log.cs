namespace FileFlows.BasicNodes.Functions;

using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Node that logs a message to the Flow logger
/// </summary>
public class Log : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 1;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string Icon => "far fa-file-alt";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/log"; 
    
    /// <summary>
    /// Gets or sets teh log type
    /// </summary>
    [Enum(1, LogType.Info, LogType.Debug, LogType.Warning, LogType.Error)]
    public LogType LogType { get; set; }
    
    /// <summary>
    /// Gets the message to log
    /// </summary>
    [TextArea(2, variables: true)]
    [Required]
    public string Message { get; set; }
    
    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var message = args.ReplaceVariables(Message ?? string.Empty, stripMissing: true);
        switch (LogType)
        {
            case LogType.Error: args.Logger.ELog(message); break;
            case LogType.Warning: args.Logger.WLog(message); break;
            case LogType.Debug: args.Logger.DLog(message); break;
            case LogType.Info: args.Logger.ILog(message); break;
        }

        return 1;
    }
}
