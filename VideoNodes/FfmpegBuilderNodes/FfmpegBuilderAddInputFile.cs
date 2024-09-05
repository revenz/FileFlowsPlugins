using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderAddInputFile : FfmpegBuilderNode
{
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/add-input-file";

    public override string Icon => "fas fa-plus";

    public override int Outputs => 2; 


    [TextVariable(1)]
    [Required]
    public string Pattern { get; set; }

    [Boolean(2)]
    public bool UseSourceDirectory { get; set; }
    
    public override int Execute(NodeParameters args)
    {
        var dir = FileHelper.GetDirectory(UseSourceDirectory ? args.LibraryFileName : args.WorkingFile);
        if (args.FileService.DirectoryExists(dir).Is(true) == false)
        {
            args.Logger?.ILog("Directory does not exist: " + dir);
            return 2;
        }
        var regex = new Regex(this.Pattern, RegexOptions.IgnoreCase);
        bool added = false;
        var files = args.FileService.GetFiles(dir);
        if(files.IsFailed)
        {
            args.Logger?.ILog("Failed to get files: " + files.Error);
            return 2;
        }
        foreach (var file in files.Value)
        {
            if (regex.IsMatch(file) == false)
                continue;
            var result = args.FileService.GetLocalPath(file);
            if (result.IsFailed)
            {
                args.Logger?.WLog($"Failed to get local file '{file}': {result.Error}");
                continue;
            }
            this.Model.InputFiles.Add(new InputFile(result.ValueOrDefault));
            added = true;
        }
        return added ? 1 : 2;
    }
}
