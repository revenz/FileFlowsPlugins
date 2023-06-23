namespace FileFlows.BasicNodes.File
{
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public class Delete : Node
    {
        public override int Inputs => 1;
        public override int Outputs => 1;
        public override FlowElementType Type => FlowElementType.Process;
        public override string Icon => "far fa-trash-alt";
        public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/delete";

        [TextVariable(1)]
        public string FileName { get; set; }

        public override int Execute(NodeParameters args)
        {
            string filename = args.ReplaceVariables(this.FileName ?? string.Empty, stripMissing: true);
            if (string.IsNullOrEmpty(filename))
                filename = args.WorkingFile;

            if (args.IsDirectory)
            {
                try
                {
                    args.Logger?.ILog("Deleting directory: " + filename);
                    Directory.Delete(filename, true);
                    args.Logger?.ILog("Deleted directory: " + filename);
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
                    args.Logger?.ILog("Deleting file: " + filename);
                    System.IO.File.Delete(filename);
                    args.Logger?.ILog("Deleted file: " + filename);
                    return 1;
                }
                catch (Exception ex)
                {
                    args.Logger?.ELog($"Failed to delete file: '{filename}' => {ex.Message}");
                    return -1;
                }
            }
        }
    }
}