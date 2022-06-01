namespace FileFlows.BasicNodes.Functions;

using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Node that logs a message to the Flow logger
/// </summary>
public class Log : Node
{
    public override int Inputs => 1;
    public override int Outputs => 1;
    public override FlowElementType Type => FlowElementType.Logic;
    public override string Icon => "far fa-file-alt";

    [Enum(1, LogType.Info, LogType.Debug, LogType.Warning, LogType.Error)]
    public LogType LogType { get; set; }

    [TextArea(2)]
    [Required]
    public string Message { get; set; }
    public override int Execute(NodeParameters args)
    {
        switch (LogType)
        {
            case LogType.Error: args.Logger.ELog(Message); break;
            case LogType.Warning: args.Logger.WLog(Message); break;
            case LogType.Debug: args.Logger.DLog(Message); break;
            case LogType.Info: args.Logger.ILog(Message); break;
        }

        return base.Execute(args);
    }
}
