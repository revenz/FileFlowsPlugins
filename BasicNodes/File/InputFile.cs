namespace FileFlows.BasicNodes.File
{
    using System.ComponentModel;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public class InputFile : Node
    {
        public override int Outputs => 1;
        public override FlowElementType Type => FlowElementType.Input;
        public override string Icon => "far fa-file";

        public override string HelpUrl => "https://docs.fileflows.com/plugins/basic-nodes/input-file";

        public override int Execute(NodeParameters args)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(args.WorkingFile);
                if (fileInfo.Exists == false)
                {
                    args.Logger?.ELog("File not found: " + args.WorkingFile);
                    return -1;
                }
                return 1;
            }
            catch (Exception ex)
            {
                args.Logger?.ELog("Failed in InputFile: " + ex.Message + Environment.NewLine + ex.StackTrace);
                return -1;
            }
        }
    }
}