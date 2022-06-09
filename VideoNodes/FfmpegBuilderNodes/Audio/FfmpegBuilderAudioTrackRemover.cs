namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderAudioTrackRemover: FfmpegBuilderNode
{
    public override string HelpUrl => "https://docs.fileflows.com/plugins/video-nodes/ffmpeg-builder/audio-track-remover";

    public override string Icon => "fas fa-volume-off";

    public override int Outputs => 2; 

    [Boolean(1)]
    public bool RemoveAll { get; set; }
    [NumberInt(2)]
    public int RemoveIndex { get; set; }


    [TextVariable(3)]
    [ConditionEquals(nameof(RemoveAll), false)]
    public string Pattern { get; set; }

    [Boolean(4)]
    [ConditionEquals(nameof(RemoveAll), false)]
    public bool NotMatching { get; set; }

    [Boolean(5)]
    [ConditionEquals(nameof(RemoveAll), false)]
    public bool UseLanguageCode { get; set; }

    public override int Execute(NodeParameters args)
    {
        bool removing = false;
        Regex? regex = null;
        int index = -1;
        foreach(var audio in Model.AudioStreams)
        {
            if (audio.Deleted == false)
            {
                // only record indexes of tracks that have not been deleted
                ++index;
                if (index < RemoveIndex)
                    continue;
            }

            if (RemoveAll)
            {
                audio.Deleted = true;
                removing = true;
                continue;
            }
            if(regex == null)
                regex = new Regex(this.Pattern, RegexOptions.IgnoreCase);
            string str = UseLanguageCode ? audio.Stream.Language : audio.Stream.Title;
            if (string.IsNullOrEmpty(str) == false) // if empty we always use this since we have no info to go on
            {
                bool matches = regex.IsMatch(str);
                if (NotMatching)
                    matches = !matches;
                if (matches)
                {
                    audio.Deleted = true;
                    removing = true;
                }
            }
        }
        return removing ? 1 : 2;
    }
}
