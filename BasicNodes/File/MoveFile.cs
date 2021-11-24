namespace FileFlows.BasicNodes.File
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public class MoveFile : Node
    {
        public override int Inputs => 1;
        public override int Outputs => 1;
        public override FlowElementType Type => FlowElementType.Process;
        public override string Icon => "fas fa-file-export";

        [Required]
        [Folder(1)]
        public string DestinationPath { get; set; }

        [Boolean(2)]
        public bool MoveFolder { get; set; }

        [Boolean(3)]
        public bool DeleteOriginal { get; set; }

        public override int Execute(NodeParameters args)
        {
            string dest = DestinationPath;
            if (string.IsNullOrEmpty(dest))
            {
                args.Logger?.ELog("No destination specified");
                args.Result = NodeResult.Failure;
                return -1;
            }
            args.Result = NodeResult.Failure;

            if (MoveFolder)
                dest = Path.Combine(dest, args.RelativeFile);
            else
                dest = Path.Combine(dest, new FileInfo(args.FileName).Name);


            var fiDest = new FileInfo(dest);
            var fiWorkingFile = new FileInfo(args.WorkingFile);
            if (fiDest.Extension != fiWorkingFile.Extension)
            {
                dest = dest.Replace(fiDest.Extension, fiWorkingFile.Extension);
                fiDest = new FileInfo(dest);
            }

            var destDir = fiDest.DirectoryName;
            if (string.IsNullOrEmpty(destDir) == false && Directory.Exists(destDir) == false)
                Directory.CreateDirectory(destDir);

            string original = args.WorkingFile;
            if (args.MoveFile(dest) == false)
                return -1;

            if (DeleteOriginal && original != args.FileName)
            {
                try
                {
                    System.IO.File.Delete(original);
                }catch(Exception ex)
                {
                    args.Logger?.WLog("Failed to delete original file: " + ex.Message);
                }
            }
            return 1;
        }
    }
}