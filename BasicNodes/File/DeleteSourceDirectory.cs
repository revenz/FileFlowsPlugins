namespace FileFlows.BasicNodes.File
{
    using System.ComponentModel;
    using System.Threading.Tasks;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public class DeleteSourceDirectory : Node
    {
        public override int Inputs => 1;
        public override int Outputs => 2;
        public override FlowElementType Type => FlowElementType.Process;
        public override string Icon => "far fa-trash-alt";

        [Boolean(1)]
        public bool IfEmpty { get; set; }

        [StringArray(2)]
        public string[] IncludePatterns { get; set; }

        public override int Execute(NodeParameters args)
        {
            string path = args.FileName.Substring(0, args.FileName.Length - args.RelativeFile.Length);

            args.Logger?.ILog("Library path: " + path);
            args.Logger?.ILog("RelativeFile: " + args.RelativeFile);
            int pathIndex = args.RelativeFile.IndexOfAny(new[] { '\\', '/' });
            if (pathIndex < 0)
            {
                args.Logger?.ILog("File is in library root, will not delete");
                return base.Execute(args);
            }

            string topdir = args.RelativeFile.Substring(0, pathIndex);
            string pathToDelete = Path.Combine(path, topdir);
            args.Logger?.ILog("Path To Delete: " + pathToDelete);

            if (IfEmpty)
            {
                string libFilePath = args.IsDirectory ? args.FileName : new FileInfo(args.FileName).DirectoryName;
                return RecursiveDelete(args, path, libFilePath, true);
            }


            args.Logger?.ILog("Deleting directory: " + pathToDelete);
            try
            {
                Directory.Delete(pathToDelete, true);
            }
            catch (Exception ex)
            {
                args.Logger?.WLog("Failed to delete directory: " + ex.Message);
                return Directory.Exists(pathToDelete) ? 2 : 1; // silent fail, another runner may have already deleted it, eg if processing music albums 
            }
            return base.Execute(args);
        }

        private int RecursiveDelete(NodeParameters args, string root, string path, bool deleteSubFolders)
        {
            args.Logger?.ILog("Checking directory to delete: " + path);
            DirectoryInfo dir = new DirectoryInfo(path);
            if (dir.Parent.FullName.ToLower() == root.ToLower())
            {
                args.Logger?.ILog("At root, stopping deleting: " + root);
                return 1;
            }
            if (dir.Parent.FullName.Length <= root.Length)
            {
                args.Logger?.ILog("At root2, stopping deleting: " + root);
                return 1;
            }
            if (deleteSubFolders == false && dir.GetDirectories().Any())
            {
                args.Logger?.ILog("Directory is contains subfolders, cannot delete: " + dir.FullName);
                return 2;
            }

            var files = dir.GetFiles("*.*", SearchOption.AllDirectories);
            if (IncludePatterns?.Any() == true)
            {
                var count = files.Where(x =>
                {
                    foreach (var pattern in IncludePatterns)
                    {
                        if (x.FullName.Contains(pattern))
                            return true;
                        try
                        {
                            if (System.Text.RegularExpressions.Regex.IsMatch(x.FullName, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                                return true;
                        }
                        catch (Exception) { }
                    }
                    return false;
                }).Count();
                if (count > 0)
                {
                    args.Logger?.ILog("Directory is not empty, cannot delete: " + dir.FullName);
                    return 2;
                }
            }
            else if (files.Length == 0)
            {
                args.Logger?.ILog("Directory is not empty, cannot delete: " + dir.FullName);
                return 2;
            }

            args.Logger?.ILog("Deleting directory: " + dir.FullName);
            try
            {
                dir.Delete(true);
            }
            catch (Exception ex)
            {
                args.Logger?.WLog("Failed to delete directory: " + ex.Message);
                return dir.Exists ? 2 : 1; // silenty fail
            }

            return RecursiveDelete(args, root, dir.Parent.FullName, false);
        }
    }
}