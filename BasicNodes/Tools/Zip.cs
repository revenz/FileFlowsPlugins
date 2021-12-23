namespace FileFlows.BasicNodes.File
{
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.IO.Compression;


    public class Zip : Node
    {
        public override int Inputs => 1;
        public override int Outputs => 1;
        public override FlowElementType Type => FlowElementType.Process;
        public override string Icon => "fas fa-file-archive";
        private string _DestinationPath = string.Empty;
        private string _DestinationFile = string.Empty;

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

        public override int Execute(NodeParameters args)
        {
            bool isDir = false;

            try
            {
                if (System.IO.Directory.Exists(args.WorkingFile))
                {
                    isDir = true;
                }
                else if (System.IO.File.Exists(args.WorkingFile) == false)
                {
                    args.Logger?.ELog("File or folder does not exist: " + args.WorkingFile);
                    return -1;
                }

                string destDir = DestinationPath;
                if (string.IsNullOrEmpty(destDir))
                {
                    if (isDir)
                        destDir = new DirectoryInfo(args.LibraryPath).FullName;
                    else
                        destDir = new FileInfo(args.FileName)?.DirectoryName ?? String.Empty;
                    if (string.IsNullOrEmpty(destDir))
                    {
                        args.Logger?.ELog("Failed to get destination directory");
                        return -1;
                    }
                }
                else
                {
                    // incase they set a linux path on windows or vice versa
                    destDir = destDir.Replace('\\', Path.DirectorySeparatorChar);
                    destDir = destDir.Replace('/', Path.DirectorySeparatorChar);

                    destDir = args.ReplaceVariables(destDir, stripMissing: true);

                    // this converts it to the actual OS path
                    destDir = new FileInfo(destDir).DirectoryName!;
                    args.CreateDirectoryIfNotExists(destDir);
                }

                string destFile = args.ReplaceVariables(DestinationFile ?? string.Empty, true);
                if (string.IsNullOrEmpty(destFile))
                {
                    if (isDir)
                        destFile = new DirectoryInfo(args.FileName).Name + ".zip";
                    else
                        destFile = new FileInfo(args.FileName).Name + ".zip";
                }
                if (destFile.ToLower().EndsWith(".zip") == false)
                    destFile += ".zip";
                destFile = Path.Combine(destDir, destFile);

                args.Logger?.ILog($"Compressing '{args.WorkingFile}' to '{destFile}'");
                if (isDir)
                {
                    var dir = new DirectoryInfo(args.WorkingFile);
                    var files = dir.GetFiles("*.*", SearchOption.AllDirectories);
                    using (FileStream fs = new FileStream(destFile, FileMode.Create))
                    {
                        using (ZipArchive arch = new ZipArchive(fs, ZipArchiveMode.Create))
                        {
                            args?.PartPercentageUpdate(0);
                            float current= 0;
                            float count = files.Length;
                            foreach(var file in files)
                            {
                                ++count;
                                if (file.FullName.ToLower() == destFile.ToLower())
                                    continue; // cant zip the zip we are creating
                                string relative = file.FullName.Substring(dir.FullName.Length + 1);
                                try
                                {
                                    arch.CreateEntryFromFile(file.FullName, relative, CompressionLevel.SmallestSize);
                                }
                                catch (Exception ex)
                                {
                                    args.Logger?.WLog("Failed to add file to zip: " + file.FullName + " => " + ex.Message);
                                }
                                args?.PartPercentageUpdate((current / count) * 100);
                            }
                            args?.PartPercentageUpdate(100);
                        }
                    }
                }
                else
                {
                    using (FileStream fs = new FileStream(destFile, FileMode.Create))
                    {
                        using (ZipArchive arch = new ZipArchive(fs, ZipArchiveMode.Create))
                        {
                            arch.CreateEntryFromFile(args.WorkingFile, new FileInfo(args.FileName).Name);
                        }
                    }
                }

                if (System.IO.File.Exists(destFile))
                {
                    args.SetWorkingFile(destFile);
                    args.Logger?.ILog("Zip created at: " + destFile);
                    return 1;
                }

                args.Logger?.ELog("Failed to create zip: " + destFile);
                return -1;
            }
            catch (Exception ex)
            {
                args.Logger?.ELog("Failed creating zip: " + ex.Message + Environment.NewLine + ex.StackTrace);
                return -1;
            }
        }
    }
}
