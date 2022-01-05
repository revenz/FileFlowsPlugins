namespace FileFlows.VideoNodes
{
    using System.ComponentModel;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public class VideoFile : VideoNode
    {
        public override int Outputs => 1;
        public override FlowElementType Type => FlowElementType.Input;

        private Dictionary<string, object> _Variables;
        public override Dictionary<string, object> Variables => _Variables;
        public VideoFile()
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
                { "vi.VideoInfo", new VideoInfo() },
                { "vi.Width", 1920 },
                { "vi.Height", 1080 },
            };
        }

        public override int Execute(NodeParameters args)
        {
            string ffmpegExe = GetFFMpegExe(args);
            if (string.IsNullOrEmpty(ffmpegExe))
                return -1;

            try
            {

                var videoInfo = new VideoInfoHelper(ffmpegExe, args.Logger).Read(args.WorkingFile);
                if (videoInfo.VideoStreams.Any() == false)
                {
                    args.Logger.ILog("No video streams detected.");
                    return 0;
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
                args.Logger.ELog("Failed processing VideoFile: " + ex.Message);
                return -1;
            }
        }
    }


}