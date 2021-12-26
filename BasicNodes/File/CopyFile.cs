namespace FileFlows.BasicNodes.File
{
    using System.ComponentModel.DataAnnotations;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

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
        [Required]
        [TextVariable(2)]
        public string DestinationFile
        {
            get => _DestinationFile;
            set { _DestinationFile = value ?? ""; }
        }

        [Boolean(3)]
        public bool CopyFolder { get; set; }

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

            var destDir = new FileInfo(dest).DirectoryName;
            args.CreateDirectoryIfNotExists(destDir ?? String.Empty);

            if(string.IsNullOrEmpty(DestinationFile) == false)
            {
                string destFile = args.ReplaceVariables(DestinationFile);
                dest = Path.Combine(destDir!, destFile);
            }

            // have to use file streams so we can report progress
            int bufferSize = 1024 * 1024;

            using (FileStream fsOut = new FileStream(dest, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
            {
                using (FileStream fsIn = new FileStream(args.WorkingFile, FileMode.Open, FileAccess.Read))
                {
                    try
                    {
                        long fileSize = fsIn.Length;
                        fsOut.SetLength(fileSize);
                        int bytesRead = -1;
                        byte[] bytes = new byte[bufferSize];

                        while ((bytesRead = fsIn.Read(bytes, 0, bufferSize)) > 0 && Canceled == false)
                        {
                            fsOut.Write(bytes, 0, bytesRead);
                            float percent = fsOut.Position / fileSize * 100;
                            if (percent > 100)
                                percent = 100;
                            if(args?.PartPercentageUpdate != null)
                                args.PartPercentageUpdate(percent);
                        }
                        if (Canceled == false && args?.PartPercentageUpdate != null)
                            args.PartPercentageUpdate(100);
                    }
                    catch (Exception ex)
                    {
                        args.Logger?.ELog("Failed to move file: " + ex.Message + Environment.NewLine + ex.StackTrace);
                        return -1;
                    }
                }
            }

            if (Canceled)
            {
                try
                {
                    System.IO.File.Delete(dest);
                }
                catch (Exception) { }
                args?.Logger?.ELog("Action was canceled.");
                return -1;
            }
            else
            {
                args?.SetWorkingFile(dest);

                return base.Execute(args!);
            }
        }
    }
}