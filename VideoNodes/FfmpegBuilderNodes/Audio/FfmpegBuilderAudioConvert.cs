using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using FileFlows.VideoNodes.Helpers;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// FFmpeg Builder flow element that converts audio
/// </summary>
public class FfmpegBuilderAudioConvert : TrackSelectorFlowElement<FfmpegBuilderAudioConvert>
{
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/audio-convert";
    /// <inheritdoc />
    public override string Icon => "fas fa-comments";
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    protected override string AutomaticLabel => "All Audio";

    /// <summary>
    /// Gets or sets the codec to use
    /// </summary>
    [DefaultValue("aac")]
    [Select(nameof(CodecOptions), 11)]
    public string Codec { get; set; }

    /// <summary>
    /// The available codec options
    /// </summary>
    private static List<ListOption> _CodecOptions;
    /// <summary>
    /// Gets the available codec options
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets the PCM format
    /// </summary>
    [DefaultValue("pcm_s16le")]
    [Select(nameof(PcmFormats), 12)]
    [ConditionEquals(nameof(Codec), "pcm")]
    public string PcmFormat { get; set; }

    /// <summary>
    /// The PCM options
    /// </summary>
    private static List<ListOption> _PcmFormats;
    /// <summary>
    /// Gets the PCM options
    /// </summary>
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

    /// <summary>
    /// Gets or sets the number of channels for the converted audio
    /// </summary>
    [DefaultValue(0)]
    [Select(nameof(ChannelsOptions), 13)]
    public float Channels { get; set; }
    /// <summary>
    /// The channel options
    /// </summary>
    private static List<ListOption> _ChannelsOptions;
    /// <summary>
    /// Gets the channel options
    /// </summary>
    public static List<ListOption> ChannelsOptions
    {
        get
        {
            if (_ChannelsOptions == null)
            {
                _ChannelsOptions = new List<ListOption>
                {
                    new () { Label = "Automatic", Value = 1000f},
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

    /// <summary>
    /// Gets or sets the bitrate
    /// </summary>
    [Select(nameof(BitrateOptions), 14)]
    public int Bitrate { get; set; }
    /// <summary>
    /// The bitrate options
    /// </summary>
    private static List<ListOption> _BitrateOptions;
    /// <summary>
    /// Gets the bitrate options
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets if the bitrate specified should be per channel
    /// </summary>
    [Boolean(15)]
    public bool BitratePerChannel { get; set; }
        
    /// <summary>
    /// Gets or sets the sample rate
    /// </summary>
    [Select(nameof(SampleRateOptions), 16)]
    public int SampleRate { get; set; }

    private static List<ListOption> _SampleRateOptions;
    /// <summary>
    /// Gets the sample rate options
    /// </summary>
    public static List<ListOption> SampleRateOptions
    {
        get
        {
            if (_SampleRateOptions == null)
            {
                _SampleRateOptions = new List<ListOption>
                {
                    new () { Label = "Automatic", Value = 0},
                    new () { Label = "Same as source", Value = 1},
                    new () { Label = "44100", Value = 44100 },
                    new () { Label = "48000", Value = 48000 },
                    new () { Label = "88200", Value = 88200 },
                    new () { Label = "96000", Value = 96000 },
                    new () { Label = "176400", Value = 176400 },
                    new () { Label = "192000", Value = 192000 }
                };
            }
            return _SampleRateOptions;
        }
    }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        bool converting = false;
        
        foreach (var track in Model.AudioStreams)
        {
            if (track.Deleted)
            {
                args.Logger?.ILog($"Stream {track} is deleted, skipping");
                continue;
            }

            if (CustomTrackSelection && StreamMatches(track) == false)
            {
                args.Logger?.ILog("Stream does not match conditions: " + track);
                continue;
            }

            bool convertResult = ConvertTrack(args, track);
            if (convertResult)
            {
                args.Logger?.ILog($"Stream {track} matches and will be converted");
                converting = true;
            }
            else
            {
                args.Logger?.ILog($"Stream {track} matches but will not be converted");
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
        
        
        int originalChannels = FfmpegBuilderAudioAddTrack.GetAudioBitrateChannels(args.Logger, stream.Channels, string.Empty);

        var channels = this.Channels > 999f ? 0 : this.Channels == 0 ? originalChannels : this.Channels; 
        
        int totalChannels = FfmpegBuilderAudioAddTrack.GetAudioBitrateChannels(args.Logger, channels, codec);
        
        bool channelsSame = originalChannels == totalChannels;
        
        int bitrate = Bitrate;
        if (BitratePerChannel && bitrate > 0)
        {
            args.Logger?.ILog("Total channels: " + totalChannels);
            args.Logger?.ILog("Bitrate Per Channel: " + bitrate);

            if (bitrate == 1)
            {
                // same as source
                // so we need to get the bitrate of the original, and divide that by the number of channels
                // then times it by the number of channels we are now using
                bitrate = (int)(totalChannels * ((stream.Stream.Bitrate / 1000) / stream.Stream.Channels));
            }
            else
            {
                bitrate = totalChannels * bitrate;
            }

            args.Logger?.ILog("Total Bitrate: " + bitrate);
        }

        int sampleRate = SampleRate switch
        {
            0 => 0,
            1 => stream.Stream.SampleRate,
            _ => SampleRate
        };
        
        bool bitrateSame = Bitrate < 2 || stream.Stream.Bitrate == 0 ||
                           Math.Abs(stream.Stream.Bitrate - Bitrate) < 0.05f;
        
        bool sampleRateSame = SampleRate <= 1 || SampleRate == stream.Stream.SampleRate;

        if (codecSame && channelsSame && bitrateSame && sampleRateSame)
        {
            args.Logger.ILog($"Stream {stream} matches codec, channels, bitrate and sample rate, skipping conversion");
            return false;
        }

        stream.Codec = Codec.ToLowerInvariant();

        stream.EncodingParameters.AddRange(
            FfmpegBuilderAudioAddTrack.GetNewAudioTrackParameters(args, stream, codec, channels, bitrate, sampleRate));
        return true;
    }
}
