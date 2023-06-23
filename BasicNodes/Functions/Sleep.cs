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
    public override int Inputs => 1;
    public override int Outputs => 1;
    public override FlowElementType Type => FlowElementType.Logic;
    public override string Icon => "fas fa-clock";
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/sleep";


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
