using FileFlows.Plugin.Helpers;

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

    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/filename-pattern-replacer";

    internal bool UnitTest = false;

    [KeyValue(1, null)]
    [Required]
    public List<KeyValuePair<string, string>> Replacements{ get; set; }

    [Boolean(2)]
    public bool UseWorkingFileName { get; set; }

    public override int Execute(NodeParameters args)
    {
        if (Replacements?.Any() != true)
            return 2; // no replacements

        try
        {
            //string filename = new FileInfo(UseWorkingFileName ? args.WorkingFile : args.FileName).Name;
            string filename = FileHelper.GetShortFileName(UseWorkingFileName ? args.WorkingFile : args.FileName);
            string updated = RunReplacements(args, filename);
            
            if (updated == filename)
            {
                args.Logger?.ILog("No replacements found in file: " + filename);
                return 2; // did not match
            }

            args.Logger?.ILog($"Pattern replacement: '{filename}' to '{updated}'");

            string directory = FileHelper.GetDirectory(args.WorkingFile);
            string dest = directory + args.FileService.PathSeparator + updated;
            args.Logger?.ILog($"New filename: " + dest);
            if (UnitTest == false)
            {
                var result = args.FileService.FileMove(args.WorkingFile, dest, true);
                if(result.IsFailed)
                {
                    args.Logger?.ELog("Failed to move file: " + result.Error);
                    return -1;
                }
            }
            args.SetWorkingFile(dest);
            return 1;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Pattern error: " + ex.Message);
            return -1;
        }
    }

    /// <summary>
    /// Run replacements on a filename
    /// </summary>
    /// <param name="args">The node parameters</param>
    /// <param name="filename">the filename to replacement</param>
    /// <returns>the replaced files</returns>
    internal string RunReplacements(NodeParameters args, string filename)
    {
        string updated = filename;
        foreach(var replacement in Replacements)
        {
            var value = replacement.Value ?? string.Empty;
            if (value == "EMPTY")
            {
                args?.Logger?.ILog("Using an EMPTY replacement");
                value = string.Empty;
            }
            else
            {
                
                args?.Logger?.ILog("Using replacement value: \"" + value + "\"");
            }
            try
            {
                // this might not be a regex, but try it first
                updated = Regex.Replace(updated, replacement.Key, value, RegexOptions.IgnoreCase);
            }
            catch (Exception ex) { }

            updated = updated.Replace(replacement.Key, value);
        }

        return updated;
    }
}
