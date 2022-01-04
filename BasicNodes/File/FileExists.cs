namespace FileFlows.BasicNodes.File
{
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public class FileExists: Node
    {
        public override int Inputs => 1;
        public override int Outputs => 2;
        public override FlowElementType Type => FlowElementType.Logic;
        public override string Icon => "fas fa-question-circle";

        [TextVariable(1)]
        public string FileName { get; set; }

        public override int Execute(NodeParameters args)
        {
            string file = args.ReplaceVariables(FileName ?? string.Empty, true);
            if(string.IsNullOrWhiteSpace(file))
            {
                args.Logger?.ELog("FileName not set");
                return -1;
            }
            try
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.Exists)
                {
                    args.Logger?.ILog("File does exist: " + file);
                    return 1;
                }
                args.Logger?.ILog("File does NOT exist: " + file);
                return 2;
            }
            catch (Exception ex)
            {
                args.Logger?.ELog($"Failed testing if file '{file}' exists: " + ex.Message);
                return -1;
            }
        }
    }
}