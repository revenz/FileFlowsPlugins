namespace FileFlows.BasicNodes.File;

using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

public class Touch : Node
{
    public override int Inputs => 1;
    public override int Outputs => 1;
    public override FlowElementType Type => FlowElementType.Process;
    public override string Icon => "fas fa-hand-point-right";
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/touch"; 


    [TextVariable(1)]
    public string FileName { get; set; }

    public override int Execute(NodeParameters args)
    {
        string filename = args.ReplaceVariables(this.FileName ?? string.Empty, stripMissing: true);
        if (string.IsNullOrEmpty(filename))
            filename = args.WorkingFile;

        if (IsDirectory(filename))
        {
            try
            {
                var dir = new DirectoryInfo(filename);
                Directory.SetLastWriteTimeUtc(filename, DateTime.UtcNow);
                return 1;
            }
            catch (Exception ex)
            {
                args.Logger?.ELog("Failed to touch directory: " + ex.Message);
                return -1;
            }
        }
        else
        {
            try
            {
                System.IO.File.SetLastWriteTimeUtc(filename, DateTime.UtcNow);
                return 1;
            }
            catch (Exception ex)
            {
                args.Logger?.ELog($"Failed to touch file: '{filename}' => {ex.Message}");
                return -1;
            }
        }
    }

    private bool IsDirectory(string filename)
    {
        try
        {
            return new DirectoryInfo(filename).Exists;
        }
        catch (Exception) { return false; }
    }
}
