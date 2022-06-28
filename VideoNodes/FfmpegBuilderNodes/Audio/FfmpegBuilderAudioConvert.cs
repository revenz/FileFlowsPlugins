using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderAudioConverter : FfmpegBuilderNode
{
    public override string HelpUrl => "https://docs.fileflows.com/plugins/video-nodes/ffmpeg-builder/audio-converter";

    public override string Icon => "fas fa-comments";

    public override int Outputs => 2;



    [DefaultValue("aac")]
    [Select(nameof(CodecOptions), 1)]
    public string Codec { get; set; }

    private static List<ListOption> _CodecOptions;
    public static List<ListOption> CodecOptions
    {
        get
        {
            if (_CodecOptions == null)
            {
                _CodecOptions = new List<ListOption>
                {
                    new ListOption { Label = "AAC", Value = "aac"},
                    new ListOption { Label = "AC3", Value = "ac3"},
                    new ListOption { Label = "EAC3", Value = "eac3" },
                    new ListOption { Label = "MP3", Value = "mp3"},
                };
            }
            return _CodecOptions;
        }
    }

    [DefaultValue(0)]
    [Select(nameof(ChannelsOptions), 2)]
    public float Channels { get; set; }

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

    [Select(nameof(BitrateOptions), 3)]
    public int Bitrate { get; set; }

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


    [TextVariable(4)]
    public string Pattern { get; set; }

    [Boolean(5)]
    public bool NotMatching { get; set; }

    [Boolean(6)]
    public bool UseLanguageCode { get; set; }

    public override int Execute(NodeParameters args)
    {
        bool converting = false;
        Regex? regex = null;
        foreach (var track in Model.AudioStreams)
        {
            if (track.Deleted)
                continue;

            if (string.IsNullOrEmpty(this.Pattern))
            {
                converting |= ConvertTrack(track);
                continue;
            }

            if (regex == null)
                regex = new Regex(this.Pattern, RegexOptions.IgnoreCase);

            string str = UseLanguageCode ? track.Stream.Language : track.Stream.Title;
            
            if (string.IsNullOrEmpty(str) == false) // if empty we always use this since we have no info to go on
            {
                bool matches = regex.IsMatch(str);
                if (NotMatching)
                    matches = !matches;
                if (matches)
                {
                    converting |= ConvertTrack(track);
                }
            }
        }
        return converting ? 1: 2;
    }

    private bool ConvertTrack(FfmpegAudioStream stream)
    {
        bool codecSame = stream.Stream.Codec?.ToLower() == Codec?.ToLower();
        bool channelsSame = Channels == 0 || Channels == stream.Stream.Channels;
        bool bitrateSame = stream.Stream.Bitrate == 0 || stream.Stream.Bitrate == Bitrate;

        if (codecSame && channelsSame && bitrateSame)
            return false;

        stream.EncodingParameters.AddRange(FfmpegBuilderAudioAddTrack.GetNewAudioTrackParameters("0:a:" + (stream.Stream.TypeIndex), Codec, Channels, Bitrate));
        return true;
    }
}
