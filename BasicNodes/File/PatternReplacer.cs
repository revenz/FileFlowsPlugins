namespace FileFlows.BasicNodes.Functions;

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

public class PatternReplacer : Node
{
    public override int Inputs => 1;
    public override int Outputs => 2;
    public override FlowElementType Type => FlowElementType.Process;
    public override string Icon => "fas fa-exchange-alt";

    public string Group => "File";

    public override string HelpUrl => "https://docs.fileflows.com/plugins/basic-nodes/filename-pattern-replacer";

    internal bool UnitTest = false;

    [KeyValue(1)]
    [Required]
    public List<KeyValuePair<string, string>> Replacements{ get; set; }

    [Boolean(2)]
    public bool UseWorkingFileName { get; set; }

    public override int Execute(NodeParameters args)
    {
        if (Replacements?.Any() != true)
            return 2; // no replacements

        string filename = new FileInfo(UseWorkingFileName ? args.WorkingFile : args.FileName).Name;
        string updated = filename;
        try
        {
            foreach(var replacement in Replacements)
            {
                var value = replacement.Value ?? string.Empty;
                if (value == "EMPTY")
                    value = string.Empty;
                try
                {
                    // this might not be a regex, but try it first
                    updated = Regex.Replace(updated, replacement.Key, value, RegexOptions.IgnoreCase);
                }
                catch (Exception ex) { }

                updated = updated.Replace(replacement.Key, value);
            }

            if (updated == filename)
            {
                args.Logger?.ILog("No replacements found in file: " + filename);
                return 2; // did not match
            }

            args.Logger?.ILog($"Pattern replacement: '{filename}' to '{updated}'");

            string dest = Path.Combine(new FileInfo(args.WorkingFile)?.DirectoryName ?? string.Empty, updated);
            args.Logger?.ILog($"New filename: " + dest);
            if (UnitTest == false)
            {
                if (args.MoveFile(dest) == false)
                {
                    args.Logger?.ELog("Failed to move file");
                    return -1;
                }
            }
            else
            {
                args.SetWorkingFile(dest);
            }
            return 1;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Pattern error: " + ex.Message);
            return -1;
        }
    }
}
