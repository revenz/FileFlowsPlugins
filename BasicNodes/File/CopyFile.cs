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
        public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/copy-file";

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

        /// <summary>
        /// Gets or sets if the original files creation and last write time dates should be preserved
        /// </summary>
        [Boolean(6)]
        public bool PreserverOriginalDates { get; set; }

        private bool Canceled;

        public override Task Cancel()
        {
            Canceled = true;
            return base.Cancel();
        }


        public override int Execute(NodeParameters args)
        {
            Canceled = false;
            
            var destParts = MoveFile.GetDestinationPathParts(args, DestinationPath, DestinationFile, CopyFolder);
            if (destParts.Filename == null)
                return -1;
            
            // cant use new FileInfo(dest).Directory.Name here since
            // if the folder is a linux folder and this node is running on windows
            // /mnt, etc will be converted to c:\mnt and break the destination
            var destDir = destParts.Path;
            string dest = destParts.Path + destParts.Separator + destParts.Filename;

            //bool copied = args.CopyFile(args.WorkingFile, dest, updateWorkingFile: true);
            //if (!copied)
            //    return -1;
            
            if (args.FileService.FileCopy(args.WorkingFile, dest, true).IsFailed)
                return -1;
            args.SetWorkingFile(dest);

            if (PreserverOriginalDates)
            {
                if (args.Variables.TryGetValue("ORIGINAL_CREATE_UTC", out object oCreateTimeUtc) &&
                    args.Variables.TryGetValue("ORIGINAL_LAST_WRITE_UTC", out object oLastWriteUtc) &&
                    oCreateTimeUtc is DateTime dtCreateTimeUtc && oLastWriteUtc is DateTime dtLastWriteUtc)
                {
                    args.Logger?.ILog("Preserving dates");
                    Helpers.FileHelper.SetLastWriteTime(dest, dtLastWriteUtc);
                    Helpers.FileHelper.SetCreationTime(dest, dtCreateTimeUtc);
                }
                else
                {
                    args.Logger?.WLog("Preserve dates is on but failed to get original dates from variables");
                }
            }

            var srcDir = FileHelper.GetDirectory(AdditionalFilesFromOriginal
                ? args.FileName
                : args.WorkingFile);

            if (AdditionalFiles?.Any() == true)
            {
                try
                {
                    foreach (var additional in AdditionalFiles)
                    {
                        foreach(var addFile in args.FileService.GetFiles(srcDir, additional).ValueOrDefault ?? new string[] {})
                        {
                            try
                            {
                                string shortName = FileHelper.GetShortFileName(addFile);
                                
                                string addFileDest = destDir + args.FileService.PathSeparator + shortName;
                                args.FileService.FileCopy(addFile, addFileDest, true);
                                //args.CopyFile(addFile, addFileDest, updateWorkingFile: false);

                                //FileHelper.ChangeOwner(args.Logger, addFileDest, file: true);

                                args.Logger?.ILog("Copied file: \"" + addFile + "\" to \"" + addFileDest + "\"");
                            }
                            catch (Exception ex)
                            {
                                args.Logger?.ILog("Failed copying file: \"" + addFile + "\": " + ex.Message);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    args.Logger.WLog("Error copying additional files: " + ex.Message);
                }
            }

            // not needed as args.CopyFile does this
            //args?.SetWorkingFile(dest);
            return 1;
        }
    }
}