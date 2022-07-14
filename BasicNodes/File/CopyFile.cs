namespace FileFlows.BasicNodes.File
{
    using System.ComponentModel.DataAnnotations;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;
    using FileFlows.Plugin.Helpers;

    /// <summary>
    /// Node that copies a file
    /// </summary>
    public class CopyFile : Node
    {
        /// <summary>
        /// Gets the number of inputs
        /// </summary>
        public override int Inputs => 1;
        
        /// <summary>
        /// Gets the number of outputs
        /// </summary>
        public override int Outputs => 1;
        
        /// <summary>
        /// Gets the type of node 
        /// </summary>
        public override FlowElementType Type => FlowElementType.Process;
        
        /// <summary>
        /// Gets the icon for this node
        /// </summary>
        public override string Icon => "far fa-copy";

        /// <summary>
        /// Gets the help URL
        /// </summary>
        public override string HelpUrl => "https://docs.fileflows.com/plugins/basic-nodes/copy-file";

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

            args.Logger.ILog($"CopyFile.Dest[0] '{dest}'");
            dest = dest.Replace("\\", Path.DirectorySeparatorChar.ToString());
            dest = dest.Replace("/", Path.DirectorySeparatorChar.ToString());
            args.Logger.ILog($"CopyFile.Dest[1] '{dest}'");
            if (string.IsNullOrEmpty(dest))
            {
                args.Logger?.ELog("No destination specified");
                return -1;
            }

            args.Logger.ILog($"CopyFile.Dest[2] '{dest}'");
            if (CopyFolder)
                dest = Path.Combine(dest, args.RelativeFile);
            else
                dest = Path.Combine(dest, new FileInfo(args.FileName).Name);
            args.Logger.ILog($"CopyFile.Dest[3] '{dest}'");

            var fiDest = new FileInfo(dest);
            var fiWorking = new FileInfo(args.WorkingFile);
            if (string.IsNullOrEmpty(fiDest.Extension) == false && fiDest.Extension != fiWorking.Extension)
            {
                dest = dest.Substring(0, dest.LastIndexOf(".")) + fiWorking.Extension;
            }
            args.Logger.ILog($"CopyFile.Dest[5] '{dest}'");

            // cant use new FileInfo(dest).Directory.Name here since
            // if the folder is a linux folder and this node is running on windows
            // /mnt, etc will be converted to c:\mnt and break the destination
            var destDir = dest.Substring(0, dest.Replace("\\", "/").LastIndexOf("/"));

            if(string.IsNullOrEmpty(DestinationFile) == false)
            {
                string destFile = args.ReplaceVariables(DestinationFile);
                dest = Path.Combine(destDir!, destFile);
            }
            args.Logger.ILog($"CopyFile.Dest[6] '{dest}'");

            args.Logger.ILog($"CopyFileArgs: '{args.WorkingFile}', '{dest}'");
            
            bool copied = args.CopyFile(args.WorkingFile, dest, updateWorkingFile: true);
            if (!copied)
                return -1;

            var srcDir = AdditionalFilesFromOriginal ? new FileInfo(args.MapPath(args.FileName)).DirectoryName : new FileInfo(args.MapPath(args.WorkingFile)).DirectoryName;

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
                                args.CopyFile(addFile.FullName, addFileDest, updateWorkingFile: false);

                                FileHelper.ChangeOwner(args.Logger, addFileDest, file: true);

                                args.Logger?.ILog("Copied file: \"" + addFile.FullName + "\" to \"" + addFileDest + "\"");
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