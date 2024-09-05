using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.File;

public class Touch : Node
{
    public override int Inputs => 1;
    public override int Outputs => 1;
    public override FlowElementType Type => FlowElementType.Process;
    public override string Icon => "fas fa-hand-point-right";
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/touch"; 

    [TextVariable(1)]
    public string FileName { get; set; }

    public override int Execute(NodeParameters args)
    {
        string filename = args.ReplaceVariables(this.FileName ?? string.Empty, stripMissing: true);
        if (string.IsNullOrEmpty(filename))
            filename = args.WorkingFile;

        var result = args.FileService.Touch(filename);
        if (result.IsFailed)
        {
            args.Logger?.ELog("Failed to touch: " + result.Error);
            return -1;
        }

        return 1;
    }
}
