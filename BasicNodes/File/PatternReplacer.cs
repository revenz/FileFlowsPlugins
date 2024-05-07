using FileFlows.Plugin.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.Functions;

/// <summary>
/// Replaces matching patterns in a fil
/// </summary>
public class PatternReplacer : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc />
    public override string Icon => "fas fa-exchange-alt";
    /// <inheritdoc />
    public override string Group => "File";

    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/filename-pattern-replacer";

    internal bool UnitTest = false;

    /// <summary>
    /// Gets or sets replacements to replace
    /// </summary>
    [KeyValue(1, null)]
    [Required]
    public List<KeyValuePair<string, string>> Replacements{ get; set; }

    /// <summary>
    /// Gets or sets if the working file should be used
    /// </summary>
    [Boolean(2)]
    public bool UseWorkingFileName { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        if (Replacements?.Any() != true)
            return 2; // no replacements

        try
        {
            string filename = FileHelper.GetShortFileName(UseWorkingFileName ? args.WorkingFile : args.FileName);
            string updated = RunReplacements(args, filename);
            
            if (updated == filename)
            {
                args.Logger?.ILog("No replacements found in file: " + filename);
                return 2; // did not match
            }

            args.Logger?.ILog($"Pattern replacement: '{filename}' to '{updated}'");

            string directory = FileHelper.GetDirectory(args.WorkingFile);
            string dest = FileHelper.Combine(directory, updated);
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
            catch (Exception)
            {
                // Ignored
            }

            updated = updated.Replace(replacement.Key, value);
        }

        return updated;
    }
}
