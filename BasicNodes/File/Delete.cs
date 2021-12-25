namespace FileFlows.BasicNodes.File
{
    using FileFlows.Plugin;

    public class Delete : Node
    {
        public override int Inputs => 1;
        public override int Outputs => 1;
        public override FlowElementType Type => FlowElementType.Process;
        public override string Icon => "far fa-trash-alt";

        public override int Execute(NodeParameters args)
        {
            if (args.IsDirectory)
            {
                try
                {
                    args.Logger?.ILog("Deleting directory: " + args.WorkingFile);
                    Directory.Delete(args.WorkingFile, true);
                    args.Logger?.ILog("Deleted directory: " + args.WorkingFile);
                    return 1;
                }
                catch (Exception ex)
                {
                    args.Logger?.ELog("Failed to delete directory: " + ex.Message);
                    return -1;
                }
            }
            else
            {
                try
                {
                    args.Logger?.ILog("Deleting file: " + args.WorkingFile);
                    System.IO.File.Delete(args.WorkingFile);
                    args.Logger?.ILog("Deleted file: " + args.WorkingFile);
                    return 1;
                }
                catch (Exception ex)
                {
                    args.Logger?.ELog($"Failed to delete file: '{args.WorkingFile}' => {ex.Message}");
                    return -1;
                }
            }
        }
    }
}