using System.ComponentModel.DataAnnotations;
using FileFlows.BasicNodes;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace BasicNodes.File;

/// <summary>
/// A flow element that writes text to a file
/// </summary>
public class WriteText : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;

    /// <inheritdoc />
    public override int Outputs => 1;

    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;

    /// <inheritdoc />
    public override string Icon => "fas fa-file-signature";

    /// <inheritdoc />
    public override string Group => "File";

    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/write-text";

    /// <summary>
    /// Gets or sets the output text location
    /// </summary>
    [TextVariable(1)]
    [Required]
    public string File { get; set; }

    /// <summary>
    /// Gets or sets the text to write, if blank writes the current file
    /// </summary>
    [TextVariable(2)]
    public string Text { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        string file = args.ReplaceVariables(File, stripMissing: true);
        if (string.IsNullOrWhiteSpace(file))
        {
            args.FailureReason = "No file set";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        string text = GetText(args, Text, file);

        args.Logger?.ILog($"Text: {text}");
        args.Logger?.ILog($"File: {file}");

        var result = args.FileService.FileAppendAllText(file, text + Environment.NewLine);
        if (result.Failed(out var error))
        {
            args.FailureReason = "File writing file: " + error;
            args.Logger.ELog(args.FailureReason);
            return -1;
        }

        args.Logger?.ILog($"Text written to file");
        return 1;
    }

    /// <summary>
    /// Gets the text to write
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <param name="text">the original text</param>
    /// <param name="outputFile">the file to write to</param>
    /// <returns>the text to write</returns>
    public static string GetText(NodeParameters args, string text, string outputFile)
    {
        if (outputFile?.ToLowerInvariant().EndsWith(".csv") == true)
        {
            var parts = text?.Split([";", ","], StringSplitOptions.RemoveEmptyEntries) ?? [];
            if (parts.Length == 0)
            {
                return CsvEncode(args.WorkingFile);
            }

            return string.Join(",", parts.Select(x => CsvEncode(args.ReplaceVariables(x, stripMissing: true))));
        }

        return args.ReplaceVariables(text, stripMissing: true)?.EmptyAsNull() ?? args.WorkingFile;
    }

    /// <summary>
    /// CSV encodes a string
    /// </summary>
    /// <param name="text">the text to encode</param>
    /// <returns>the CSV encoded string</returns>
    private static string CsvEncode(string text)
        => $"\"{text.Replace("\"", "\"\"")}\"";
}