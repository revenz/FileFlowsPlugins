namespace FileFlows.Pdf.FlowElements;

/// <summary>
/// A flow element that tests if a PDF contains the specified text
/// </summary>
public class PdfContainsText : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string Icon => "fas fa-file-pdf";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/pdf/pdf-contains-text";
    /// <inheritdoc />
    public override string Group => "PDF";

    /// <summary>
    /// Gets or sets the text to check for
    /// </summary>
    [Required]
    [TextVariable(1)]
    public string Text { get; set; } = null!;

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var fileResult = args.FileService.GetLocalPath(args.WorkingFile);
        if (fileResult.Failed(out var error))
            return args.Fail("Failed to get local file: " + error);
        
        var file = fileResult.Value;
        var text = args.ReplaceVariables(Text);
        args.Logger?.ILog("Checking if PDF contains text: " + text);
        var containsResult = args.PdfHelper.ContainsText(file, text);
        if(containsResult.Failed(out error))
            return args.Fail("Failed to get contains text: " + error);
        
        if (containsResult.Value == false)
        {
            args.Logger?.ILog("PDF does not contain the text: " + text);
            return 2;
        }

        args.Logger?.ILog("PDF contains the text: " + text);
        return 1;
    }
}