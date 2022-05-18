namespace FileFlows.VideoNodes
{
    using FileFlows.Plugin;

    public class ReadVideoInfo: EncodingNode
    {
        public override string Icon => "fas fa-video";
        public override int Outputs => 2;

        private Dictionary<string, object> _Variables;
        public override Dictionary<string, object> Variables => _Variables;
        public ReadVideoInfo()
        {
            _Variables = new Dictionary<string, object>()
            {
                { "vi.Video.Codec", "hevc" },
                { "vi.Audio.Codec", "ac3" },
                { "vi.Audio.Codecs", "ac3,aac"},
                { "vi.Audio.Language", "eng" },
                { "vi.Audio.Languages", "eng, mao" },
                { "vi.Resolution", "1080p" },
                { "vi.Duration", 1800 },
                { "vi.VideoInfo", new VideoInfo()
                    {
                        Bitrate = 10_000_000,
                        VideoStreams = new List<VideoStream> {
                            new VideoStream { }
                        },
                        AudioStreams = new List<AudioStream> {
                            new AudioStream { }
                        },
                        SubtitleStreams = new List<SubtitleStream>
                        {
                            new SubtitleStream { }
                        }
                    }
                },
                { "vi.Width", 1920 },
                { "vi.Height", 1080 },
            };
        }

        public override int Execute(NodeParameters args)
        {
            try
            {

                var videoInfo = new VideoInfoHelper(FFMPEG, args.Logger).Read(args.WorkingFile);
                if (videoInfo.VideoStreams.Any() == false)
                {
                    args.Logger.ILog("No video streams detected.");
                    return 2;
                }
                foreach (var vs in videoInfo.VideoStreams)
                {
                    args.Logger.ILog($"Video stream '{vs.Codec}' '{vs.Index}'");
                }


                foreach (var vs in videoInfo.AudioStreams)
                {
                    args.Logger.ILog($"Audio stream '{vs.Codec}' '{vs.Index}' '{vs.Language}");
                }

                SetVideoInfo(args, videoInfo, Variables);

                return 1;
            }
            catch (Exception ex)
            {
                args.Logger.WLog("Failed processing VideoFile: " + ex.Message);
                return 2;
            }
        }
    }
}