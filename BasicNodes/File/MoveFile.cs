namespace FileFlows.BasicNodes.File
{
    using System.ComponentModel;
    using System.Threading.Tasks;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public class MoveFile : Node
    {
        public override int Inputs => 1;
        public override int Outputs => 1;
        public override FlowElementType Type => FlowElementType.Process;
        public override string Icon => "fas fa-file-export";

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
                args.Logger.ELog("No destination specified");
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
            if (Directory.Exists(destDir) == false)
                Directory.CreateDirectory(destDir);

            long fileSize = new FileInfo(args.WorkingFile).Length;

            bool moved = false;
            Task task = Task.Run(() =>
            {
                try
                {
                    if (System.IO.File.Exists(dest))
                        System.IO.File.Delete(dest);
                    args.Logger.ILog($"Moving file: \"{args.WorkingFile}\" to \"{dest}\"");
                    System.IO.File.Move(args.WorkingFile, dest, true);

                    if (DeleteOriginal && args.WorkingFile != args.FileName)
                    {
                        System.IO.File.Delete(args.FileName);
                    }
                    args.SetWorkingFile(dest);

                    moved = true;
                }
                catch (Exception ex)
                {
                    args.Logger.ELog("Failed to move file: " + ex.Message);
                }
            });

            while (task.IsCompleted == false)
            {
                long currentSize = 0;
                var destFileInfo = new FileInfo(dest);
                if (destFileInfo.Exists)
                    currentSize = destFileInfo.Length;

                args.PartPercentageUpdate(currentSize / fileSize * 100);
                System.Threading.Thread.Sleep(50);
            }

            if (moved == false)
                return -1;

            args.PartPercentageUpdate(100);

            return base.Execute(args);
        }
    }
}