namespace FileFlows.VideoNodes
{
    using FileFlows.Plugin;

    public abstract class VideoNode : Node
    {
        public override string Icon => "fas fa-video";

        protected string GetFFMpegExe(NodeParameters args)
        {
            string ffmpeg = args.GetToolPath("FFMpeg");
            if (string.IsNullOrEmpty(ffmpeg))
            {
                args.Logger.ELog("FFMpeg tool not found.");
                return "";
            }
            var fileInfo = new FileInfo(ffmpeg);
            if (fileInfo.Exists == false)
            {
                args.Logger.ELog("FFMpeg tool configured by ffmpeg.exe file does not exist.");
                return "";
            }
            return fileInfo.FullName;
        }
        protected string GetFFMpegPath(NodeParameters args)
        {
            string ffmpeg = args.GetToolPath("FFMpeg");
            if (string.IsNullOrEmpty(ffmpeg))
            {
                args.Logger.ELog("FFMpeg tool not found.");
                return "";
            }
            var fileInfo = new FileInfo(ffmpeg);
            if (fileInfo.Exists == false)
            {
                args.Logger.ELog("FFMpeg tool configured by ffmpeg.exe file does not exist.");
                return "";
            }
            return fileInfo.DirectoryName;
        }
        protected string GetFFPlayExe(NodeParameters args)
        {
            string ffmpeg = args.GetToolPath("FFMpeg");
            if (string.IsNullOrEmpty(ffmpeg))
            {
                args.Logger.ELog("FFMpeg tool not found.");
                return "";
            }

            var fileInfo = new FileInfo(ffmpeg);
            if (fileInfo.Exists == false)
            {
                args.Logger.ELog("FFMpeg tool configured by ffmpeg file does not exist.");
                return "";
            }

            var ffplay = Path.Combine(fileInfo.DirectoryName, "ffplay" + fileInfo.Extension);
            if (File.Exists(ffplay) == false)
            {
                args.Logger.ELog("FFMpeg tool configured by ffplay file does not exist.");
                return "";
            }
            return ffplay;
        }

        private const string VIDEO_INFO = "VideoInfo";
        protected void SetVideoInfo(NodeParameters args, VideoInfo videoInfo, Dictionary<string, object> variables)
        {
            if (videoInfo.VideoStreams?.Any() == false)
                return;

            if (args.Parameters.ContainsKey(VIDEO_INFO))
                args.Parameters[VIDEO_INFO] = videoInfo;
            else
                args.Parameters.Add(VIDEO_INFO, videoInfo);

            variables.AddOrUpdate("vi.VideoInfo", videoInfo);
            variables.AddOrUpdate("vi.Width", videoInfo.VideoStreams[0].Width);
            variables.AddOrUpdate("vi.Height", videoInfo.VideoStreams[0].Height);
            variables.AddOrUpdate("vi.Duration", videoInfo.VideoStreams[0].Duration.TotalSeconds);
            variables.AddOrUpdate("vi.Video.Codec", videoInfo.VideoStreams[0].Codec);
            if (videoInfo.AudioStreams?.Any() == true)
            {
                ;
                if (string.IsNullOrEmpty(videoInfo.AudioStreams[0].Codec))
                    Variables.AddOrUpdate("vi.Audio.Codec", videoInfo.AudioStreams[0].Codec);
                if (string.IsNullOrEmpty(videoInfo.AudioStreams[0].Codec))
                    Variables.AddOrUpdate("vi.Audio.Channels", videoInfo.AudioStreams[0].Channels);
                if (string.IsNullOrEmpty(videoInfo.AudioStreams[0].Language))
                    Variables.AddOrUpdate("vi.Audio.Language", videoInfo.AudioStreams[0].Language);
                Variables.AddOrUpdate("vi.Audio.Codecs", string.Join(", ", videoInfo.AudioStreams.Select(x => x.Codec).Where(x => string.IsNullOrEmpty(x) == false)));
                Variables.AddOrUpdate("vi.Audio.Languages", string.Join(", ", videoInfo.AudioStreams.Select(x => x.Language).Where(x => string.IsNullOrEmpty(x) == false)));
            }

            if (videoInfo.VideoStreams[0].Width == 1920)
                Variables.AddOrUpdate("vi.Resolution", "1080");
            else if (videoInfo.VideoStreams[0].Width == 3840)
                Variables.AddOrUpdate("vi.Resolution", "4l");
            else if (videoInfo.VideoStreams[0].Width == 1280)
                Variables.AddOrUpdate("vi.Resolution", "720p");
            else if (videoInfo.VideoStreams[0].Width < 1280)
                Variables.AddOrUpdate("vi.Resolution", "SD");
            else
                Variables.AddOrUpdate("vi.Resolution", videoInfo.VideoStreams[0].Width + "x" + videoInfo.VideoStreams[0].Height);

            args.UpdateVariables(variables);
        }

        protected VideoInfo GetVideoInfo(NodeParameters args)
        {
            if (args.Parameters.ContainsKey(VIDEO_INFO) == false)
            {
                args.Logger.WLog("No codec information loaded, use a 'VideoFile' node first");
                return null;
            }
            var result = args.Parameters[VIDEO_INFO] as VideoInfo;
            if (result == null)
            {
                args.Logger.WLog("VideoInfo not found for file");
                return null;
            }
            return result;
        }
    }
}