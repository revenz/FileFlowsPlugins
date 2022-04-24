using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderAudioSetLanguage : FfmpegBuilderNode
{
    public override string HelpUrl => "https://github.com/revenz/FileFlows/wiki/FFMPEG-Builder:-Audio-Set-Language";

    public override int Outputs => 2;

    public override string Icon => "fas fa-comment-dots";

    [Required]
    [Text(1)]
    public string Language { get; set; }

    public override int Execute(NodeParameters args)
    {
        base.Init(args);

        bool changes = false;
        foreach (var at in Model.AudioStreams)
        {
            if (string.IsNullOrEmpty(at.Language))
            {
                at.Language = Language.ToLower();
                at.ForcedChange = true; // this will ensure the language is set even if there are no changes anywhere else
                changes = true;
            }
        }
        return changes ? 1 : 2;
    }
}
