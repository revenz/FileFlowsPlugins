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
                { "vi.VideoCodec", string.Empty },
                { "vi.AudioCodec", string.Empty },
                { "vi.AudioCodecs", string.Empty },
                { "vi.AudioLanguage", string.Empty },
                { "vi.AudioLanguages", string.Empty },
                { "vi.Resolution", string.Empty },
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

                Variables["vi.VideoCodec"] = videoInfo.VideoStreams[0].Codec;
                if (videoInfo.AudioStreams?.Any() == true)
                {
                    ;
                    if (string.IsNullOrEmpty(videoInfo.AudioStreams[0].Codec))
                        Variables["vi.AudioCodec"] = videoInfo.AudioStreams[0].Codec;
                    if(string.IsNullOrEmpty(videoInfo.AudioStreams[0].Language))
                        Variables["vi.AudioLanguage"] = videoInfo.AudioStreams[0].Language;
                    Variables["vi.AudioCodecs"] = string.Join(", ", videoInfo.AudioStreams.Select(x => x.Codec).Where(x => string.IsNullOrEmpty(x) == false));
                    Variables["vi.AudioLanguages"] = string.Join(", ", videoInfo.AudioStreams.Select(x => x.Language).Where(x => string.IsNullOrEmpty(x) == false));
                }

                if (videoInfo.VideoStreams[0].Width == 1920)
                    Variables["vi.Resolution"] = "1080P";
                else if (videoInfo.VideoStreams[0].Width == 3840)
                    Variables["vi.Resolution"] = "4k";
                else if (videoInfo.VideoStreams[0].Width == 1280)
                    Variables["vi.Resolution"] = "720p";
                else if (videoInfo.VideoStreams[0].Width < 1280)
                    Variables["vi.Resolution"] = "SD";
                else
                    Variables["vi.Resolution"] = videoInfo.VideoStreams[0].Width + "x" + videoInfo.VideoStreams[0].Height;

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