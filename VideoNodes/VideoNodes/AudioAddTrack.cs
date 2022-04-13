namespace FileFlows.VideoNodes
{
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    public class AudioAddTrack: EncodingNode
    {
        public override int Outputs => 1;

        public override string Icon => "fas fa-volume-down";

        [NumberInt(1)]
        [Range(0, 100)]
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

        public override int Execute(NodeParameters args)
        {
            try
            {
                VideoInfo videoInfo = GetVideoInfo(args);
                if (videoInfo == null)
                    return -1;

                string ffmpegExe = GetFFMpegExe(args);
                if (string.IsNullOrEmpty(ffmpegExe))
                    return -1;

                List<string> ffArgs = new List<string>
                {
                    "-c", "copy",
                    "-map", "0:v",
                };

                bool added = false;
                int audioIndex = 0;
                for(int i = 0; i < videoInfo.AudioStreams.Count; i++)
                {
                    if(i == Index)
                    {
                        ffArgs.AddRange(GetNewAudioTrackParameters(videoInfo, audioIndex));
                        added = true;
                        ++audioIndex;
                    }
                    ffArgs.AddRange(new[]
                    {
                        "-map", videoInfo.AudioStreams[i].IndexString,
                        "-c:a:" + audioIndex, "copy"
                    });
                    ++audioIndex;
                }

                if(added == false) // incase the index is greater than the number of tracks this file has
                    ffArgs.AddRange(GetNewAudioTrackParameters(videoInfo, audioIndex));

                if (videoInfo.SubtitleStreams?.Any() == true)
                    ffArgs.AddRange(new[] { "-map", "0:s" });

                if (Index < 2)
                {
                    // this makes the first audio track now the default track
                    ffArgs.AddRange(new[] { "-disposition:a:0", "default" });
                }

                string extension = new FileInfo(args.WorkingFile).Extension;
                if(extension.StartsWith("."))
                    extension = extension.Substring(1); 

                if (Encode(args, ffmpegExe, ffArgs, extension) == false)
                    return -1;

                return 1;
            }
            catch (Exception ex)
            {
                args.Logger?.ELog("Failed processing VideoFile: " + ex.Message);
                return -1;
            }        
        }

        private string[] GetNewAudioTrackParameters(VideoInfo videoInfo, int index)
        {
            if (Channels == 0)
            {
                // same as source
                if(Bitrate == 0)
                {
                    return new[]
                    {
                        "-map", videoInfo.AudioStreams[0].IndexString,
                        "-c:a:" + index, Codec
                    };
                }
                return new[]
                {
                    "-map", videoInfo.AudioStreams[0].IndexString,
                    "-c:a:" + index, Codec,
                    "-b:a:" + index, Bitrate + "k"
                };
            }
            else
            {
                if (Bitrate == 0)
                {
                    return new[]
                    {
                        "-map", videoInfo.AudioStreams[0].IndexString,
                        "-c:a:" + index, Codec,
                        "-ac", Channels.ToString()
                    };
                }
                return new[]
                {
                    "-map", videoInfo.AudioStreams[0].IndexString,
                    "-c:a:" + index, Codec,
                    "-ac", Channels.ToString(),
                    "-b:a:" + index, Bitrate + "k"
                };
            }
        }
    }
}
