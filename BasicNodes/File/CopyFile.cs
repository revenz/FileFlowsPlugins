namespace FileFlows.BasicNodes.File
{
    using System.ComponentModel.DataAnnotations;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;
    using FileFlows.Plugin.Helpers;

    public class CopyFile : Node
    {
        public override int Inputs => 1;
        public override int Outputs => 1;
        public override FlowElementType Type => FlowElementType.Process;
        public override string Icon => "far fa-copy";

        private string _DestinationPath = string.Empty;
        private string _DestinationFile = string.Empty;

        [Required]
        [Folder(1)]
        public string DestinationPath 
        { 
            get => _DestinationPath;
            set { _DestinationPath = value ?? ""; }
        }
        [TextVariable(2)]
        public string DestinationFile
        {
            get => _DestinationFile;
            set { _DestinationFile = value ?? ""; }
        }

        [Boolean(3)]
        public bool CopyFolder { get; set; }

        [StringArray(4)]
        public string[] AdditionalFiles { get; set; }

        [Boolean(5)]
        public bool AdditionalFilesFromOriginal { get; set; }

        private bool Canceled;

        public override Task Cancel()
        {
            Canceled = true;
            return base.Cancel();
        }


        public override int Execute(NodeParameters args)
        {
            Canceled = false;
            string dest = args.ReplaceVariables(DestinationPath, true);
            dest = dest.Replace("\\", Path.DirectorySeparatorChar.ToString());
            dest = dest.Replace("/", Path.DirectorySeparatorChar.ToString());
            if (string.IsNullOrEmpty(dest))
            {
                args.Logger?.ELog("No destination specified");
                return -1;
            }

            if (CopyFolder)
                dest = Path.Combine(dest, args.RelativeFile);
            else
                dest = Path.Combine(dest, new FileInfo(args.FileName).Name);

            var fiDest = new FileInfo(dest);
            var fiWorking = new FileInfo(args.WorkingFile);
            if (string.IsNullOrEmpty(fiDest.Extension) == false && fiDest.Extension != fiWorking.Extension)
            {
                dest = dest.Substring(0, dest.LastIndexOf(".")) + fiWorking.Extension;
            }

            var destDir = new FileInfo(dest).DirectoryName;
            args.CreateDirectoryIfNotExists(destDir ?? String.Empty);

            if(string.IsNullOrEmpty(DestinationFile) == false)
            {
                string destFile = args.ReplaceVariables(DestinationFile);
                dest = Path.Combine(destDir!, destFile);
            }

            bool copied = args.CopyFile(dest);
            if (!copied)
                return -1;

            var srcDir = AdditionalFilesFromOriginal ? new FileInfo(args.FileName).DirectoryName : new FileInfo(args.WorkingFile).DirectoryName;

            if (AdditionalFiles?.Any() == true)
            {
                try
                {
                    var diSrc = new DirectoryInfo(srcDir);
                    foreach (var additional in AdditionalFiles)
                    {
                        foreach (var addFile in diSrc.GetFiles(additional))
                        {
                            try
                            {
                                string addFileDest = Path.Combine(destDir, addFile.Name);
                                System.IO.File.Copy(addFile.FullName, addFileDest, true);
                                
                                FileHelper.ChangeOwner(args.Logger, addFileDest, file: true);

                                args.Logger?.ILog("Copyied file: \"" + addFile.FullName + "\" to \"" + addFileDest + "\"");
                            }
                            catch (Exception ex)
                            {
                                args.Logger?.ILog("Failed copying file: \"" + addFile.FullName + "\": " + ex.Message);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    args.Logger.WLog("Error copying additinoal files: " + ex.Message);
                }
            }

            // not needed as args.CopyFile does this
            //args?.SetWorkingFile(dest);
            return 1;
        }
    }
}