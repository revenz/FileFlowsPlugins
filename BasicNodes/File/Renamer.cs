namespace FileFlows.BasicNodes.File
{
    using System.ComponentModel.DataAnnotations;
    using System.Text;
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

        [File(4)]
        public string CsvFile { get; set; }

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

            newFile = args.ReplaceVariables(newFile, stripMissing: true, cleanSpecialCharacters: true);
            newFile = Regex.Replace(newFile, @"\.(\.[\w\d]+)$", "$1");
            // remove empty [], (), {}
            newFile = newFile.Replace("()", "").Replace("{}", "").Replace("[]", "");
            // remove double space that may have been introduced by empty [], () removals
            while (newFile.IndexOf("  ") >= 0)
                newFile = newFile.Replace("  ", " ");
            newFile = Regex.Replace(newFile, @"\s(\.[\w\d]+)$", "$1");
            newFile = newFile.Replace(" \\", "\\");

            
            string destFolder = args.ReplaceVariables(DestinationPath ?? string.Empty, stripMissing: true, cleanSpecialCharacters: true);
            if (string.IsNullOrEmpty(destFolder))
                destFolder = new FileInfo(args.WorkingFile).Directory?.FullName ?? "";

            var dest = args.GetSafeName(Path.Combine(destFolder, newFile));

            if (string.IsNullOrEmpty(dest.Extension) == false)
            {
                // just ensures extensions are lowercased
                dest = new FileInfo(dest.FullName.Substring(0, dest.FullName.LastIndexOf(dest.Extension)) + dest.Extension.ToLower());
            }



            args.Logger?.ILog("Renaming file to: " + dest.FullName);

            if (string.IsNullOrEmpty(CsvFile) == false)
            {
                try
                {
                    System.IO.File.AppendAllText(CsvFile, EscapeForCsv(args.FileName) + "," + EscapeForCsv(dest.FullName) + Environment.NewLine);
                }
                catch (Exception ex)  
                {
                    args.Logger?.ELog("Failed to append to CSV file: " + ex.Message);
                }
            }

            if (LogOnly)
                return 1;

            return args.MoveFile(dest.FullName) ? 1 : -1;
        }

        private string EscapeForCsv(string input)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('"');
            foreach(char c in input)
            {
                sb.Append(c);
                if(c == '"')
                    sb.Append('"');
            }
            sb.Append('"');
            return sb.ToString();
        }
    }
}
