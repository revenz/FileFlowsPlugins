namespace FileFlows.ComicNodes.Comics;

public class ComicExtractor : Node
{ 
    public override int Inputs => 1;
    public override int Outputs => 1;
    public override FlowElementType Type => FlowElementType.Process;
    public override string Icon => "fas fa-file-pdf";

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
        Helpers.ComicExtractor.Extract(args, args.WorkingFile, dest, halfProgress: false);

        return 1;
    }
}
