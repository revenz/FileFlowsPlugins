namespace FileFlows.BasicNodes.Functions;

using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Node that sleeps for a given time
/// </summary>
public class Sleep : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 1;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string Icon => "fas fa-clock";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/sleep";
    /// <inheritdoc />
    public override bool FailureNode => true;


    [NumberInt(1)]
    [Range(1, 3_600_000)]
    [DefaultValue(1000)]
    public int Milliseconds{ get; set; }

    public override int Execute(NodeParameters args)
    {
        if (Milliseconds < 1 || Milliseconds > 3_600_000)
        {
            args.Logger.ELog("Milliseconds must be between 1 and 3,600,000");
            return -1;
        }
        Thread.Sleep(Milliseconds);
        return 1;
    }
}
