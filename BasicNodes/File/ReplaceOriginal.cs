namespace FileFlows.BasicNodes.File
{
    using System.Text;
    using System.Text.RegularExpressions;
    using FileFlows.Plugin;

    public class ReplaceOriginal : Node
    {
        public override int Inputs => 1;
        public override int Outputs => 1;
        public override string Icon => "fas fa-file";

        public string _Pattern = string.Empty;

        public override FlowElementType Type => FlowElementType.Process;

        public override int Execute(NodeParameters args)
        {
            if (args.FileName == args.WorkingFile)
            {
                args.Logger?.ILog("Working file is same as original, nothing to do.");
                return 1;
            }
            var wf = new FileInfo(args.WorkingFile);
            if (args.FileName.ToLower().EndsWith(wf.Extension.ToLower()))
            {
                // easy replace
                bool moved = args.MoveFile(args.FileName);
                if(moved == false)
                {
                    args.Logger?.ELog("Failed to move file to: "+ args.FileName);
                    return -11;
                }
            }
            else
            {
                // different extension, we will move the file, but then delete the original                
                string dest = Path.ChangeExtension(args.FileName, wf.Extension);
                if(args.MoveFile(dest) == false)
                {
                    args.Logger?.ELog("Failed to move file to: " + dest);
                    return 1;
                }
                if (dest.ToLower() != args.FileName.ToLower())
                {
                    try
                    {
                        System.IO.File.Delete(args.FileName);
                    }
                    catch (Exception ex)
                    {
                        args.Logger?.ELog("Failed to delete orginal (with different extension): " + ex.Message);
                        return -1;
                    }
                }
            }

            return 1;
        }
    }
}
