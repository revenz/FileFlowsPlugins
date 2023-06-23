namespace FileFlows.BasicNodes.File
{
    using System.Text;
    using System.Text.RegularExpressions;
    using FileFlows.Plugin;

    public class OriginalFile : Node
    {
        public override int Inputs => 1;
        public override int Outputs => 1;
        public override string Icon => "fas fa-file";
        public override FlowElementType Type => FlowElementType.Logic;
        public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/original-file";

        public override int Execute(NodeParameters args)
        {
            args.SetWorkingFile(args.FileName);
            args.Logger?.ILog("Set working file to: " + args.FileName);
            return 1;
        }
    }
}
