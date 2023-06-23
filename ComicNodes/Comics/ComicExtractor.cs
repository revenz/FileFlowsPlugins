using System.Text.RegularExpressions;

namespace FileFlows.ComicNodes.Comics;

public class ComicExtractor : Node
{ 
    public override int Inputs => 1;
    public override int Outputs => 1;
    public override FlowElementType Type => FlowElementType.Process;

    public override string Icon => "fas fa-file-pdf";
    public override string HelpUrl => "https://fileflows.com/docs/plugins/comic-nodes/comic-extractor";

    CancellationTokenSource cancellation = new CancellationTokenSource();

    [Required]
    [Folder(1)]
    public string DestinationPath { get; set; }

    public override int Execute(NodeParameters args)
    {
        string dest = args.ReplaceVariables(DestinationPath, true);
        dest = dest.Replace("\\", Path.DirectorySeparatorChar.ToString());
        dest = dest.Replace("/", Path.DirectorySeparatorChar.ToString());
        if (string.IsNullOrEmpty(dest))
        {
            args.Logger?.ELog("No destination specified");
            args.Result = NodeResult.Failure;
            return -1;
        }
        Helpers.ComicExtractor.Extract(args, args.WorkingFile, dest, halfProgress: false, cancellation: cancellation.Token);

        var metadata = new Dictionary<string, object>();
        metadata.Add("Format", args.WorkingFile.Substring(args.WorkingFile.LastIndexOf(".") + 1).ToUpper());
        var rgxImages = new Regex(@"\.(jpeg|jpg|jpe|png|bmp|tiff|webp|gif)$");
        metadata.Add("Pages", Directory.GetFiles(dest, "*.*", SearchOption.AllDirectories).Where(x => rgxImages.IsMatch(x)).Count());
        args.SetMetadata(metadata);

        return 1;
    }
    public override Task Cancel()
    {
        cancellation.Cancel();
        return base.Cancel();
    }
}
