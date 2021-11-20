namespace FileFlows.BasicNodes.File
{
    using System.ComponentModel;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public class CopyFile : Node
    {
        public override int Inputs => 1;
        public override int Outputs => 1;
        public override FlowElementType Type => FlowElementType.Process;
        public override string Icon => "far fa-copy";

        private string _DestinationPath = string.Empty;
        
        [Folder(1)]
        public string DestinationPath 
        { 
            get => _DestinationPath;
            set { _DestinationPath = value ?? ""; }
        }

        [Boolean(2)]
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
            string dest = DestinationPath;
            if (string.IsNullOrEmpty(dest))
            {
                args.Logger.ELog("No destination specified");
                args.Result = NodeResult.Failure;
                return -1;
            }
            args.Result = NodeResult.Failure;

            if (CopyFolder)
                dest = Path.Combine(dest, args.RelativeFile);
            else
                dest = Path.Combine(dest, new FileInfo(args.FileName).Name);

            var destDir = new FileInfo(dest).DirectoryName;
            if (Directory.Exists(destDir) == false)
                Directory.CreateDirectory(destDir);

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
                            args.PartPercentageUpdate(percent);
                        }
                        if (Canceled == false)
                            args.PartPercentageUpdate(100);
                    }
                    catch (Exception ex)
                    {
                        args.Logger.ELog("Failed to move file: " + ex.Message + Environment.NewLine + ex.StackTrace);
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
                args.Logger.ELog("Action was canceled.");
                return -1;
            }
            else
            {
                args.SetWorkingFile(dest);

                return base.Execute(args);
            }
        }
    }
}