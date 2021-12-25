namespace FileFlows.BasicNodes.File
{
    using System.ComponentModel;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public class InputFolder: Node
    {
        public override int Outputs => 1;
        public override FlowElementType Type => FlowElementType.Input;
        public override string Icon => "far fa-folder";
        public override int Execute(NodeParameters args)
        {
            try
            {
                var info = new DirectoryInfo(args.WorkingFile);
                if (info.Exists == false)
                {
                    args.Logger?.ELog("Directory not found: " + args.WorkingFile);
                    return -1;
                }
                return 1;
            }
            catch (Exception ex)
            {
                args.Logger?.ELog("Failed in InputDirectory: " + ex.Message + Environment.NewLine + ex.StackTrace);
                return -1;
            }
        }
    }
}