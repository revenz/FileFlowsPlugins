using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderAddInputFile : FfmpegBuilderNode
{
    public override string HelpUrl => "https://github.com/revenz/FileFlows/wiki/FFMPEG-Builder:-Add-Input-File";

    public override string Icon => "fas fa-plus";

    public override int Outputs => 2; 


    [TextVariable(1)]
    [Required]
    public string Pattern { get; set; }

    [Boolean(2)]
    public bool UseSourceDirectory { get; set; }
    
    public override int Execute(NodeParameters args)
    {
        var dir = new FileInfo(UseSourceDirectory ? args.FileName : args.WorkingFile).Directory;
        if (dir.Exists == false)
        {
            args.Logger?.ILog("Directory does not exist: " + dir.FullName);
            return 2;
        }
        var regex = new Regex(this.Pattern, RegexOptions.IgnoreCase);
        bool added = false;
        foreach (var file in dir.GetFiles())
        {
            if (regex.IsMatch(file.Name) == false)
                continue;
            this.Model.InputFiles.Add(file.FullName);
            added = true;
        }
        return added ? 1 : 2;
    }
}
