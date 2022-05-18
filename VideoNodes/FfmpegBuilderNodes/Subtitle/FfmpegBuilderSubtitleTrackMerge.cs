using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderSubtitleTrackMerge : FfmpegBuilderNode
{
    public override string HelpUrl => "https://github.com/revenz/FileFlows/wiki/FFMPEG-Builder:-Subtitle-Track-Merge";
    
    public override string Icon => "fas fa-comment-medical";

    public override int Outputs => 2;

    [Checklist(nameof(Options), 1)]
    [Required]
    public List<string> Subtitles { get; set; }

    private static List<ListOption> _Options;
    public static List<ListOption> Options
    {
        get
        {
            if (_Options == null)
            {
                _Options = new List<ListOption>
                {
                    new ListOption { Value = "ass", Label = "ass: Advanced SubStation Alpha"},
                    new ListOption { Value = "idx", Label = "idx: IDX"},
                    new ListOption { Value = "srt", Label = "srt: SubRip subtitle"},
                    new ListOption { Value = "ssa", Label = "ssa: SubStation Alpha"},
                    new ListOption { Value = "sub", Label = "sub: SubStation Alpha"},
                    new ListOption { Value = "sup", Label = "sup: SubPicture"},
                    new ListOption { Value = "txt", Label = "txt: Raw text subtitle"}                        
                };
            }
            return _Options;
        }
    }

    [Boolean(2)]
    [DefaultValue(true)]
    public bool UseSourceDirectory { get; set; } = true;
    
    public override int Execute(NodeParameters args)
    {
        var dir = new FileInfo(UseSourceDirectory ? args.FileName : args.WorkingFile).Directory;
        if (dir.Exists == false)
        {
            args.Logger?.ILog("Directory does not exist: " + dir.FullName);
            return 2;
        }
        bool added = false;
        foreach (var file in dir.GetFiles())
        {
            string ext = file.Extension;
            if (string.IsNullOrEmpty(ext) || ext.Length < 2)
                continue;
            ext = ext.Substring(1).ToLower();// remove .
            if (Subtitles.Contains(ext))
                continue;
            
            this.Model.InputFiles.Add(file.FullName);
            added = true;
        }
        return added ? 1 : 2;
    }
}
