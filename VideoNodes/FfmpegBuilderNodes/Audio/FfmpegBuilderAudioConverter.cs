using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using FileFlows.VideoNodes.Helpers;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderAudioConverter : FfmpegBuilderNode
{
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/audio-converter";

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
                    new () { Label = "AAC", Value = "aac"},
                    new () { Label = "AC3", Value = "ac3"},
                    new () { Label = "DTS", Value = "dts" },
                    new () { Label = "EAC3", Value = "eac3" },
                    new () { Label = "FLAC", Value ="flac" },
                    new () { Label = "MP3", Value = "mp3"},
                    new () { Label = "PCM", Value ="pcm" },
                    new () { Label = "OPUS", Value = "opus"},
                    new () { Label = "Vorbis", Value ="libvorbis" },
                };
            }
            return _CodecOptions;
        }
    }
    
    [DefaultValue("pcm_s16le")]
    [Select(nameof(PcmFormats), 2)]
    [ConditionEquals(nameof(Codec), "pcm")]
    public string PcmFormat { get; set; }

    private static List<ListOption> _PcmFormats;
    public static List<ListOption> PcmFormats
    {
        get
        {
            if (_PcmFormats == null)
            {
                _PcmFormats = new List<ListOption>
                {
                    new () { Label = "Common", Value = "###GROUP###" },
                    new () { Label = "Signed 16-bit Little Endian", Value = "pcm_s16le" },
                    new () { Label = "Signed 24-bit Little Endian", Value = "pcm_s24le"},
                    
                    new () { Label = "Signed", Value = "###GROUP###" },
                    new () { Label = "8-bit", Value = "pcm_s8"},
                    new () { Label = "16-bit Little Endian", Value = "pcm_s16le" },
                    new () { Label = "16-bit Big Endian", Value = "pcm_s16be"},
                    new () { Label = "24-bit Little Endian", Value = "pcm_s24le"},
                    new () { Label = "24-bit Big Endian", Value = "pcm_s24be"},
                    new () { Label = "32-bit Little Endian", Value = "pcm_s32le"},
                    new () { Label = "32-bit Big Endian", Value = "pcm_s32be"},
                    new () { Label = "64-bit Little Endian", Value = "pcm_s64le"},
                    new () { Label = "64-bit Big Endian", Value = "pcm_s64be"},
                    new () { Label = "Floating-point", Value = "###GROUP###" },
                    new () { Label = "32-bit Little Endian", Value = "pcm_f32le"},
                    new () { Label = "32-bit Big Endian", Value = "pcm_f32be"},
                    new () { Label = "16-bit Little Endian", Value = "pcm_f16le"},
                    new () { Label = "24-bit Little Endian", Value = "pcm_f24le"},
                    new () { Label = "64-bit Little Endian", Value = "pcm_f64le"},
                    new () { Label = "64-bit Big Endian", Value = "pcm_f64be"},
                };

            }
            return _PcmFormats;
        }
    }


    [DefaultValue(0)]
    [Select(nameof(ChannelsOptions), 3)]
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
                    new () { Label = "Same as source", Value = 0},
                    new () { Label = "Mono", Value = 1f},
                    new () { Label = "Stereo", Value = 2f},
                    new () { Label = "5.1", Value = 6},
                    new () { Label = "7.1", Value = 8}
                };
            }
            return _ChannelsOptions;
        }
    }

    [Select(nameof(BitrateOptions), 4)]
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
                    new () { Label = "Automatic", Value = 0},
                    new () { Label = "Same as source", Value = 1},
                };
                for (int i = 64; i <= 2048; i += 32)
                {
                    _BitrateOptions.Add(new ListOption { Label = i + " Kbps", Value = i });
                }
            }
            return _BitrateOptions;
        }
    }

    [DefaultValue("")]
    [Select(nameof(FieldOptions), 5)]
    public string Field { get; set; }

    private static List<ListOption> _FieldOptions;

    internal const string FIELD_TITLE = "Title";
    internal const string FIELD_LANGUAGE = "Language";
    internal const string FIELD_CODEC = "Codec";

    public static List<ListOption> FieldOptions
    {
        get
        {
            if (_FieldOptions == null)
            {
                _FieldOptions = new List<ListOption>
                {
                    new() { Label = "Convert All", Value = "" },
                    new() { Label = "Title", Value = FIELD_TITLE },
                    new() { Label = "Language", Value = FIELD_LANGUAGE },
                    new() { Label = "Codec", Value = FIELD_CODEC },
                };
            }

            return _FieldOptions;
        }
    }

    [TextVariable(6)]
    [ConditionEquals(nameof(Field), "", true)]
    public string Pattern { get; set; }

    [Boolean(7)]
    [ConditionEquals(nameof(Field), "", true)]
    public bool NotMatching { get; set; }

    public override int Execute(NodeParameters args)
    {
        bool converting = false;
        Regex? regex = null;
        
        
        foreach (var track in Model.AudioStreams)
        {
            if (track.Deleted)
            {
                args.Logger?.ILog($"Stream {track} is deleted, skipping");
                continue;
            }

            bool convert = false;
            if (string.IsNullOrEmpty(this.Field))
            {
                convert = true;
            }
            else
            {
                string testValue = Field switch
                {
                    FIELD_LANGUAGE => track.Language?.EmptyAsNull() ?? track.Stream?.Language ?? string.Empty,
                    FIELD_TITLE => track.Title?.EmptyAsNull() ?? track.Stream?.Title ?? string.Empty,
                    FIELD_CODEC => track.Codec?.EmptyAsNull() ?? track.Stream?.Codec ?? string.Empty,
                    _ => null
                };
                if (testValue == null)
                {
                    args.Logger?.ILog("Failed to load test value for stream: " + track);
                    continue;
                }

                string pattern = this.Pattern ?? string.Empty;

                if (GeneralHelper.IsRegex(pattern))
                {
                    try
                    {
                        convert =
                            new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase).IsMatch(
                                testValue);
                        if (NotMatching)
                            convert = !convert;
                    }
                    catch (Exception ex)
                    {
                        args.Logger?.WLog("Invalid pattern: " + ex.Message);
                        continue;
                    }
                }
                else if (Field == FIELD_LANGUAGE)
                {
                    args.Logger?.ILog("Matching language using language helper.");
                    convert = LanguageHelper.AreSame(pattern, testValue);
                    if (NotMatching)
                        convert = !convert;
                }
                else
                {
                    convert = string.Equals(pattern, testValue, StringComparison.InvariantCultureIgnoreCase);
                    if (NotMatching)
                        convert = !convert;
                }
            }

            if (convert == false)
            {
                args.Logger?.ILog("Stream does not match conditions: " + track);
                continue;
            }

            bool convertResult = ConvertTrack(args, track);
            if (convertResult)
            {
                args.Logger?.ILog($"Stream {track} matches pattern and will be converted");
                converting = true;
            }
            else
            {
                args.Logger?.ILog($"Stream {track} matches pattern but will not be converted");
            }
        }
        return converting ? 1 : 2;
    }
    
    /// <summary>
    /// Converts and audio track
    /// </summary>
    /// <param name="args">the node arguments</param>
    /// <param name="stream">teh stream to convert</param>
    /// <returns>if the stream had to be converted or not</returns>
    private bool ConvertTrack(NodeParameters args, FfmpegAudioStream stream)
    {
        string codec = Codec?.ToLowerInvariant() ?? string.Empty;
        if (codec == "pcm")
            codec = PcmFormat;
        
        bool codecSame = stream.Stream.Codec?.ToLowerInvariant() == codec;
        bool channelsSame = Channels == 0 || Math.Abs(Channels - stream.Stream.Channels) < 0.05f;
        bool bitrateSame = Bitrate < 2 || stream.Stream.Bitrate == 0 ||
                           Math.Abs(stream.Stream.Bitrate - Bitrate) < 0.05f;

        if (codecSame && channelsSame && bitrateSame)
        {
            args.Logger.ILog($"Stream {stream} matches codec, channels, and bitrate, skipping conversion");
            return false;
        }

        stream.Codec = Codec.ToLowerInvariant();

        stream.EncodingParameters.AddRange(FfmpegBuilderAudioAddTrack.GetNewAudioTrackParameters(args, stream, codec, Channels, Bitrate, 0));
        return true;
    }
}
