namespace FileFlows.Pdf.FlowElements;

/// <summary>
/// A flow element that extracts the text of a PDF to a file
/// </summary>
public class PdfToTextFile : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc />
    public override string Icon => "fas fa-file-pdf";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/pdf/pdf-to-text-file";
    /// <inheritdoc />
    public override string Group => "PDF";

    /// <summary>
    /// Gets or sets the output text location
    /// </summary>
    [TextVariable(1)]
    [Required]
    public string File { get; set; } = null!;

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var fileResult = args.FileService.GetLocalPath(args.WorkingFile);
        if (fileResult.Failed(out var error))
            return args.Fail("Failed to get local file: " + error);
        
        var file = fileResult.Value;
        var output = args.ReplaceVariables(File);
        args.Logger?.ILog("Extracting PDF to file: " + output);
        var result = args.PdfHelper.ExtractText(file);
        if (result.Failed(out error))
            return args.Fail("Failed to extract text: " + error);

        var text = result.Value;
        if (string.IsNullOrWhiteSpace(text))
        {
            args.Logger?.ILog("No text found in PDF file");
            return 2;
        }
        
        args.FileService.FileDelete(file);
        var writeResult = args.FileService.FileAppendAllText(file, text);
        if (writeResult.Failed(out error))
            return args.Fail("Failed to write text to file: " + error);
        
        args.Logger?.ILog("Saved PDF text to file: " + file);
        args.SetWorkingFile(file);
        return 1;
    }
}