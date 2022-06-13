using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderAudioSetLanguage : FfmpegBuilderNode
{
    public override string HelpUrl => "https://docs.fileflows.com/plugins/video-nodes/ffmpeg-builder/set-language";

    public override int Outputs => 2;

    public override string Icon => "fas fa-comment-dots";


    [Select(nameof(StreamTypeOptions), 1)]
    public string StreamType { get; set; }

    [Required]
    [Text(2)]
    public string Language { get; set; }

    private static List<ListOption> _StreamTypeOptions;
    public static List<ListOption> StreamTypeOptions
    {
        get
        {
            if (_StreamTypeOptions == null)
            {
                _StreamTypeOptions = new List<ListOption>
                {
                    new ListOption { Label = "Audio", Value = "Audio" },
                    new ListOption { Label = "Subtitle", Value = "Subtitle" },
                    new ListOption { Label = "Both", Value = "Both" },
                };
            }
            return _StreamTypeOptions;
        }
    }

    public override int Execute(NodeParameters args)
    {
        bool changes = false;
        if (StreamType == "Subtitle" || StreamType == "Both")
        {

            foreach (var at in Model.AudioStreams)
            {
                if (string.IsNullOrEmpty(at.Language))
                {
                    at.Language = Language.ToLower();
                    at.ForcedChange = true; // this will ensure the language is set even if there are no changes anywhere else
                    changes = true;
                }
            }
        }
        
        if(string.IsNullOrEmpty(StreamType) || StreamType == "Both" || StreamType == "Audio")
        {
            foreach (var at in Model.AudioStreams)
            {
                if (string.IsNullOrEmpty(at.Language))
                {
                    at.Language = Language.ToLower();
                    at.ForcedChange = true; // this will ensure the language is set even if there are no changes anywhere else
                    changes = true;
                }
            }
        }
        return changes ? 1 : 2;
    }
}
