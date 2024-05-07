namespace FileFlows.ComicNodes.Comics;

/// <summary>
/// Extracts a comic 
/// </summary>
public class ComicExtractor : Node
{ 
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 1;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc />
    public override string Icon => "fas fa-file-pdf";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/comic-nodes/comic-extractor";

    CancellationTokenSource cancellation = new CancellationTokenSource();

    /// <summary>
    /// Gets or sets the destination to extract the comic into
    /// </summary>
    [Required]
    [Folder(1)]
    public string DestinationPath { get; set; } = string.Empty;

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        string dest = args.ReplaceVariables(DestinationPath, true);
        dest = dest.Replace("\\", Path.DirectorySeparatorChar.ToString());
        dest = dest.Replace("/", Path.DirectorySeparatorChar.ToString());
        if (string.IsNullOrEmpty(dest))
        {
            args.FailureReason = "No destination specified"; 
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        if (Helpers.ComicExtractor
            .Extract(args, args.WorkingFile, dest, halfProgress: false, cancellation: cancellation.Token)
            .Failed(out string error))
        {
            args.FailureReason = "Failed to extract comic: " + error;
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }

        var metadata = new Dictionary<string, object>();
        metadata.Add("Format", args.WorkingFile[(args.WorkingFile.LastIndexOf(".", StringComparison.Ordinal) + 1)..].ToUpper());
        var rgxImages = new Regex(@"\.(jpeg|jpg|jpe|png|bmp|tiff|webp|gif)$");
        metadata.Add("Pages", Directory.GetFiles(dest, "*.*", SearchOption.AllDirectories).Count(x => rgxImages.IsMatch(x)));
        args.SetMetadata(metadata);

        return 1;
    }
    
    /// <summary>
    /// Cancels the extraction
    /// </summary>
    /// <returns>the task to await</returns>
    public override Task Cancel()
    {
        cancellation.Cancel();
        return base.Cancel();
    }
}
