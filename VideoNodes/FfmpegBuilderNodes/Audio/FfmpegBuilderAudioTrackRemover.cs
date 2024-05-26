using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderAudioTrackRemover: FfmpegBuilderNode
{
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/track-remover";

    public override string Icon => "fas fa-eraser";

    public override int Outputs => 2;


    [Select(nameof(StreamTypeOptions), 1)]
    [ChangeValue(nameof(RemoveAll), false, "Video")]
    public string StreamType { get; set; }

    [Boolean(2)]
    [ConditionEquals(nameof(StreamType), "Video", inverse: true)]
    public bool RemoveAll { get; set; }

    [NumberInt(3)]
    public int RemoveIndex { get; set; }

    /// <summary>
    /// Gets or sets the type to match against
    /// </summary>
    [Required]
    [Select(nameof(MatchTypes), 4)]
    [DefaultValue(MatchTypeOption.Title)]
    [ConditionEquals(nameof(RemoveAll), false)]
    public MatchTypeOption MatchType { get; set; }

    private static List<ListOption>? _MatchTypes;
    /// <summary>
    /// Gets the match types
    /// </summary>
    public static List<ListOption> MatchTypes
    {
        get
        {
            if (_MatchTypes == null)
            {
                _MatchTypes = new List<ListOption>
                {
                    new () { Label = "Title", Value = MatchTypeOption.Title },
                    new () { Label = "Language", Value = MatchTypeOption.Language },
                    new () { Label = "Codec", Value = MatchTypeOption.Codec }
                };
            }
            return _MatchTypes;
        }
    }

    [TextVariable(5)]
    [ConditionEquals(nameof(RemoveAll), false)]
    public string Pattern { get; set; }

    [Boolean(6)]
    [ConditionEquals(nameof(RemoveAll), false)]
    public bool NotMatching { get; set; }


    /// <summary>
    /// Left in for legacy reasons, will be removed later
    /// </summary>
    [Obsolete]
    public bool? UseLanguageCode
    {
        get => false;
        set
        {
            if ((int)this.MatchType > 0)
                return; // we can now ignore this value

            if (value == true)
                this.MatchType = MatchTypeOption.Language;
            else if(value == false)
                this.MatchType = MatchTypeOption.Title;
        }
    }

    private static List<ListOption> _StreamTypeOptions;
    public static List<ListOption> StreamTypeOptions
    {
        get
        {
            if (_StreamTypeOptions == null)
            {
                _StreamTypeOptions = new List<ListOption>
                {
                    new () { Label = "Video", Value = "Video" },
                    new () { Label = "Audio", Value = "Audio" },
                    new () { Label = "Subtitle", Value = "Subtitle" }
                };
            }
            return _StreamTypeOptions;
        }
    }
    public override int Execute(NodeParameters args)
    {
        string pattern = args.ReplaceVariables(this.Pattern, stripMissing: true);
        if(string.IsNullOrEmpty(StreamType) || StreamType.ToLower() == "audio")
            return RemoveTracks(pattern, Model.AudioStreams) ? 1 : 2;
        if (StreamType.ToLower() == "subtitle")
            return RemoveTracks(pattern, Model.SubtitleStreams) ? 1 : 2;
        if (StreamType.ToLower() == "video")
            return RemoveTracks(pattern, Model.VideoStreams) ? 1 : 2;

        return 2;
    }

    private bool RemoveTracks<T>(string pattern, List<T> tracks) where T: FfmpegStream
    {
        bool removing = false;
        Regex? regex = null;
        int index = -1;
        Args.Logger.ILog("Using match type: " + MatchType);
        foreach (var track in tracks)
        {
            if (track.Deleted == false)
            {
                // only record indexes of tracks that have not been deleted
                ++index;
                if (index < RemoveIndex)
                    continue;
            }

            if (RemoveAll || string.IsNullOrEmpty(pattern))
            {
                track.Deleted = true;
                removing = true;
                continue;
            }

            if (regex == null)
                regex = new Regex(pattern, RegexOptions.IgnoreCase);

            string str = "";
            if(track is FfmpegAudioStream audio)
                str = MatchType == MatchTypeOption.Language ? audio.Stream.Language  :
                      MatchType == MatchTypeOption.Codec ? audio.Stream.Codec :
                      audio.Stream.Title;
            else if (track is FfmpegSubtitleStream subtitle)
                str = MatchType == MatchTypeOption.Language ? subtitle.Stream.Language :
                      MatchType == MatchTypeOption.Codec ? subtitle.Stream.Codec :
                      subtitle.Stream.Title;
            else if (track is FfmpegVideoStream video)
                str = MatchType == MatchTypeOption.Codec ? video.Stream.Codec : video.Stream.Title;

            Args.Logger.ILog("Testing string: " + str);
            if (string.IsNullOrEmpty(str) == false) // if empty we always use this since we have no info to go on
            {
                bool matches = regex.IsMatch(str);
                if (NotMatching)
                    matches = !matches;
                if (matches)
                {
                    track.Deleted = true;
                    removing = true;
                }
            }
        }
        return removing;
    }
}


public enum MatchTypeOption
{
    Title = 1,
    Language = 2,
    Codec = 3
};
