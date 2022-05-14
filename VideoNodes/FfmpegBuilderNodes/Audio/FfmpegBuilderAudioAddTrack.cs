using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderAudioAddTrack : FfmpegBuilderNode
{
    public override string Icon => "fas fa-volume-off";

    public override string HelpUrl => "https://github.com/revenz/FileFlows/wiki/FFMPEG-Builder:-Audio-Add-Track";

    [NumberInt(1)]
    [Range(0, 100)]
    [DefaultValue(1)]
    public int Index { get; set; }


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

    [DefaultValue(2f)]
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
                    new ListOption { Label = "64 Kbps", Value = 64},
                    new ListOption { Label = "96 Kbps", Value = 96},
                    new ListOption { Label = "128 Kbps", Value = 128},
                    new ListOption { Label = "160 Kbps", Value = 160},
                    new ListOption { Label = "192 Kbps", Value = 192},
                    new ListOption { Label = "224 Kbps", Value = 224},
                    new ListOption { Label = "256 Kbps", Value = 256},
                    new ListOption { Label = "288 Kbps", Value = 288},
                    new ListOption { Label = "320 Kbps", Value = 320},
                };
            }
            return _BitrateOptions;
        }
    }

    [DefaultValue("eng")]
    [TextVariable(4)]
    public string Language { get; set; }

    public override int Execute(NodeParameters args)
    {
        base.Init(args);

        var audio = new FfmpegAudioStream();

        var bestAudio = GetBestAudioTrack(args, Model.AudioStreams.Select(x => x.Stream));
        if (bestAudio == null)
        {
            args.Logger.WLog("No source audio track found");
            return -1;
        }

        audio.Stream = bestAudio;

        bool directCopy = false;
        if(bestAudio.Codec.ToLower() == this.Codec.ToLower())
        {
            if(this.Channels == 0 || this.Channels == bestAudio.Channels)
            {
                directCopy = true;
            }
        }

        if (directCopy)
        {
            args.Logger?.ILog($"Source audio is already in appropriate format, just copying that track: {bestAudio.IndexString}, Channels: {bestAudio.Channels}, Codec: {bestAudio.Codec}");
        }
        else
        {
            audio.EncodingParameters.AddRange(GetNewAudioTrackParameters("0:a:" + (bestAudio.TypeIndex)));
        }
        if (Index > Model.AudioStreams.Count - 1)
            Model.AudioStreams.Add(audio);
        else 
            Model.AudioStreams.Insert(Math.Max(0, Index), audio);

        return 1;
    }

    internal AudioStream GetBestAudioTrack(NodeParameters args, IEnumerable<AudioStream> streams)
    {
        Regex? rgxLanguage = null;
        try
        {
            rgxLanguage = new Regex(this.Language, RegexOptions.IgnoreCase);
        }
        catch (Exception) { }
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
        var bestAudio = streams.Where(x => System.Text.Json.JsonSerializer.Serialize(x).ToLower().Contains("commentary") == false)
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
        .OrderBy(x =>
        {
            if (Language != string.Empty)
            {
                args.Logger?.ILog("Language: " + x.Language, x);
                if (string.IsNullOrEmpty(x.Language))
                    return 50; // no language specified
                if (rgxLanguage != null && rgxLanguage.IsMatch(x.Language))
                    return 0;
                if (x.Language.ToLower() != Language)
                    return 100; // low priority not the desired language
            }
            return 0;
        })
        .ThenByDescending(x => {
            if(this.Channels == 2)
            {
                if (x.Channels == 2)
                    return 1_000_000_000;
                // compare codecs
                if (x.Codec?.ToLower() == this.Codec?.ToLower())
                    return 1_000_000;
            }
            if(this.Channels == 1)
            {
                if (x.Channels == 1)
                    return 1_000_000_000;
                if (x.Channels <= 2.1f)
                    return 5_000_000;
                if (x.Codec?.ToLower() == this.Codec?.ToLower())
                    return 1_000_000;
            }

            // now we want best channels, but to prefer matching codec
            if (x.Codec?.ToLower() == this.Codec?.ToLower())
            {
                return 1_000 + x.Channels;
            }
            return x.Channels;
        })
        .ThenBy(x => x.Index)
        .FirstOrDefault();
        return bestAudio;
    }


    private string[] GetNewAudioTrackParameters(string source)
    {
        if (Channels == 0)
        {
            // same as source
            if (Bitrate == 0)
            {
                return new[]
                {
                    "-map", source, 
                    "-c:a:{index}",
                    Codec
                };
            }
            return new[]
            {
                "-map", source,
                "-c:a:{index}",
                Codec,
                "-b:a:{index}", Bitrate + "k"
            };
        }
        else
        {
            if (Bitrate == 0)
            {
                return new[]
                {
                    "-map", source,
                    "-c:a:{index}",
                    Codec,
                    "-ac:a:{index}", Channels.ToString()
                };
            }
            return new[]
            {
                "-map", source,
                "-c:a:{index}",
                Codec,
                "-ac:a:{index}", Channels.ToString(),
                "-b:a:{index}", Bitrate + "k"
            };
        }
    }
}
