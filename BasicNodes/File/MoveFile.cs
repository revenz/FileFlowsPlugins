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

        [TextVariable(2)]
        public string DestinationFile{ get; set; }

        [Boolean(3)]
        public bool MoveFolder { get; set; }

        [Boolean(4)]
        public bool DeleteOriginal { get; set; }

        [StringArray(5)]
        public string[] AdditionalFiles { get; set; }

        [Boolean(6)]
        public bool AdditionalFilesFromOriginal { get; set; }

        public override int Execute(NodeParameters args)
        {
            string dest = args.ReplaceVariables(DestinationPath, true);
            dest = dest.Replace("\\", Path.DirectorySeparatorChar.ToString());
            dest = dest.Replace("/", Path.DirectorySeparatorChar.ToString());
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
            var fiWorking = new FileInfo(args.WorkingFile);
            if (string.IsNullOrEmpty(fiDest.Extension) == false && fiDest.Extension != fiWorking.Extension)
            {
                dest = dest.Substring(0, dest.LastIndexOf(".")) + fiWorking.Extension;
            }

            if (string.IsNullOrEmpty(DestinationFile) == false)
            {
                string destFile = args.ReplaceVariables(DestinationFile);
                dest = Path.Combine(new FileInfo(dest).DirectoryName!, destFile);
            }

            fiDest = new FileInfo(dest);
            var fiWorkingFile = new FileInfo(args.WorkingFile);
            if (fiDest.Extension != fiWorkingFile.Extension)
            {
                dest = dest.Replace(fiDest.Extension, fiWorkingFile.Extension);
                fiDest = new FileInfo(dest);
            }

            var destDir = fiDest.DirectoryName;
            args.CreateDirectoryIfNotExists(destDir ?? String.Empty);

            var srcDir = AdditionalFilesFromOriginal ? new FileInfo(args.FileName).DirectoryName : new FileInfo(args.WorkingFile).DirectoryName;

            if (args.MoveFile(dest) == false)
                return -1;

            if(AdditionalFiles?.Any() == true)
            {
                try
                {
                    var diSrc = new DirectoryInfo(srcDir);
                    foreach (var additional in AdditionalFiles)
                    {
                        foreach(var addFile in diSrc.GetFiles(additional))
                        {
                            try
                            {
                                string addFileDest = Path.Combine(destDir, addFile.Name);
                                System.IO.File.Move(addFile.FullName, addFileDest, true);
                                args.Logger?.ILog("Moved file: \"" + addFile.FullName + "\" to \"" + addFileDest + "\"");
                            }
                            catch(Exception ex)
                            {
                                args.Logger?.ILog("Failed moving file: \"" + addFile.FullName + "\": " + ex.Message);
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    args.Logger.WLog("Error moving additinoal files: " + ex.Message);
                }
            }


            if (DeleteOriginal && args.FileName != args.WorkingFile)
            {
                args.Logger?.ILog("Deleting orginal file: " + args.FileName);
                try
                {
                    System.IO.File.Delete(args.FileName);
                }
                catch(Exception ex)
                {
                    args.Logger?.WLog("Failed to delete original file: " + ex.Message);
                }
            }
            return 1;
        }
    }
}