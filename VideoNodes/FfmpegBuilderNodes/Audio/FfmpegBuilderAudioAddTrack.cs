using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes
{
    public class FfmpegBuilderAudioAddTrack: FfmpegBuilderNode
    {
        public override string Icon => "fas fa-volume-off";

        [NumberInt(1)]
        [Range(1, 100)]
        [DefaultValue(2)]
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
                        new ListOption { Label = "Stereo", Value = 2f}
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
            audio.Stream = Model.AudioStreams[0].Stream;

#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            var bestAudio = Model.AudioStreams.Where(x => System.Text.Json.JsonSerializer.Serialize(x.Stream).ToLower().Contains("commentary") == false)
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            .OrderBy(x =>
            {
                if (Language != string.Empty)
                {
                    args.Logger?.ILog("Language: " + x.Stream.Language, x);
                    if (string.IsNullOrEmpty(x.Stream.Language))
                        return 50; // no language specified
                    if (x.Stream.Language?.ToLower() != Language)
                        return 100; // low priority not the desired language
                }
                return 0;
            })
            .ThenByDescending(x => x.Stream.Channels)
            .ThenBy(x => x.Index)
            .FirstOrDefault();

            audio.EncodingParameters.AddRange(GetNewAudioTrackParameters("0:a:" + (bestAudio.Stream.TypeIndex - 1)));
            if (Index > Model.AudioStreams.Count)
                Model.AudioStreams.Add(audio);
            else 
                Model.AudioStreams.Insert(Math.Max(0, Index - 1), audio);

            return 1;
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
                        "-ac", Channels.ToString()
                    };
                }
                return new[]
                {
                    "-map", source,
                    "-c:a:{index}",
                    Codec,
                    "-ac", Channels.ToString(),
                    "-b:a:{index}", Bitrate + "k"
                };
            }
        }
    }
}
