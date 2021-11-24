namespace FileFlows.BasicNodes.File
{
    using System.ComponentModel.DataAnnotations;
    using System.Text.RegularExpressions;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public class Renamer : Node
    {
        public override int Inputs => 1;
        public override int Outputs => 1;
        public override string Icon => "fas fa-font";

        public string _Pattern = string.Empty;

        public override FlowElementType Type => FlowElementType.Process;

        [Required]
        [TextVariable(1)]
        public string? Pattern
        {
            get => _Pattern;
            set { _Pattern = value ?? ""; }
        }

        private string _DestinationPath = string.Empty;

        [Folder(2)]
        public string DestinationPath
        {
            get => _DestinationPath;
            set { _DestinationPath = value ?? ""; }
        }

        [Boolean(3)]
        public bool LogOnly { get; set; }

        public override int Execute(NodeParameters args)
        {
            if(string.IsNullOrEmpty(Pattern))
            {
                args.Logger?.ELog("No pattern specified");
                return -1;
            }

            string newFile = Pattern;
            // incase they set a linux path on windows or vice versa
            newFile = newFile.Replace('\\', Path.DirectorySeparatorChar);
            newFile = newFile.Replace('/', Path.DirectorySeparatorChar);

            if (args.Variables?.Any() == true)
            {
                {
                    foreach (string variable in args.Variables.Keys)
                    {
                        string strValue = args.Variables[variable]?.ToString() ?? "";
                        newFile = ReplaceVariable(newFile, variable, strValue);
                    }
                }
            }

            string destFolder = DestinationPath;
            if (string.IsNullOrEmpty(destFolder))
                destFolder = new FileInfo(args.WorkingFile).Directory?.FullName ?? "";

            var dest = new FileInfo(Path.Combine(destFolder, newFile));
            
            args.Logger?.ILog("Renaming file to: " + (string.IsNullOrEmpty(DestinationPath) ? "" : DestinationPath + Path.DirectorySeparatorChar) + newFile);


            if (LogOnly)
                return 1;

            return args.MoveFile(dest.FullName) ? 1 : -1;
        }

        private string ReplaceVariable(string input, string variable, string value)
        {
            return Regex.Replace(input, @"{" + Regex.Escape(variable) + @"}", value, RegexOptions.IgnoreCase);
        }
    }
}
