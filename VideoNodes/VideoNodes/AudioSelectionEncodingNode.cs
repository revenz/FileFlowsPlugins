namespace FileFlows.VideoNodes;

public abstract class AudioSelectionEncodingNode : EncodingNode, ITrackAudioSelectionNode
{

    [TextVariable(12)]
    public string Title { get; set; }

    [TextVariable(13)]
    public string Codec { get; set; }

    [TextVariable(14)]
    public string Language { get; set; }

    [NumberFloat(15)]
    public virtual float Channels { get; set; }

    [Boolean(18)]
    public bool NotMatching { get; set; }

    protected AudioStream GetTrack(VideoInfo videoInfo)
    {
        var trackSelector = new TrackSelector();
        trackSelector.Title = Title;
        trackSelector.Channels = Channels;
        trackSelector.Codec= Codec;
        trackSelector.Language = Language;
        trackSelector.NotMatching = NotMatching;
        return trackSelector.FindAudioStream(videoInfo);
    }


    private static List<ListOption> _ChannelsOptions;
    public static List<ListOption> ChannelsOptions
    {
        get
        {
            if (_ChannelsOptions == null)
            {
                _ChannelsOptions = new List<ListOption>
                {
                    new ListOption { Label = "Same as source", Value = 0},
                    new ListOption { Label = "Mono", Value = 1f},
                    new ListOption { Label = "Stereo", Value = 2f},
                    new ListOption { Label = "5.1", Value = 6},
                    new ListOption { Label = "7.1", Value = 8}
                };
            }
            return _ChannelsOptions;
        }
    }


    private static List<ListOption> _BitrateOptions;
    public static List<ListOption> BitrateOptions
    {
        get
        {
            if (_BitrateOptions == null)
            {
                _BitrateOptions = new List<ListOption>
                {
                    new ListOption { Label = "Automatic", Value = 0},
                };
                for (int i = 64; i <= 2048; i += 32)
                {
                    _BitrateOptions.Add(new ListOption { Label = i + " Kbps", Value = i });
                }
            }
            return _BitrateOptions;
        }
    }
}
