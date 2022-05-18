namespace FileFlows.VideoNodes
{
    using FileFlows.Plugin;

    public abstract class VideoNode : Node
    {
        /// <summary>
        /// Gets the Node Parameters
        /// </summary>
        protected NodeParameters Args { get; private set; }
        


#if (DEBUG)
        /// <summary>
        /// Used for unit tests
        /// </summary>
        /// <param name="args">the args</param>
        public void SetArgs(NodeParameters args)
        {
            this.Args = args;
        }
#endif
        
        /// <summary>
        /// Gets the FFMPEG executable location
        /// </summary>
        protected string FFMPEG { get; private set; }
        public override string Icon => "fas fa-video";

        /// <summary>
        /// Executed before execute, sets ffmpegexe etc
        /// </summary>
        /// <param name="args">the node parametes</param>
        /// <returns>true if successfully</returns>
        public override bool PreExecute(NodeParameters args)
        {
            this.Args = args;
            this.FFMPEG = GetFFMpegExe();
            return string.IsNullOrEmpty(this.FFMPEG) == false;
        }

        private string GetFFMpegExe()
        {
            string ffmpeg = Args.GetToolPath("FFMpeg");
            if (string.IsNullOrEmpty(ffmpeg))
            {
                Args.Logger.ELog("FFMpeg tool not found.");
                return "";
            }
            var fileInfo = new FileInfo(ffmpeg);
            if (fileInfo.Exists == false)
            {
                Args.Logger.ELog("FFMpeg tool configured by ffmpeg.exe file does not exist.");
                return "";
            }
            return fileInfo.FullName;
        }
        // protected string GetFFMpegPath(NodeParameters args)
        // {
        //     string ffmpeg = args.GetToolPath("FFMpeg");
        //     if (string.IsNullOrEmpty(ffmpeg))
        //     {
        //         args.Logger.ELog("FFMpeg tool not found.");
        //         return "";
        //     }
        //     var fileInfo = new FileInfo(ffmpeg);
        //     if (fileInfo.Exists == false)
        //     {
        //         args.Logger.ELog("FFMpeg tool configured by ffmpeg.exe file does not exist.");
        //         return "";
        //     }
        //     return fileInfo.DirectoryName;
        // }

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
                variables.AddOrUpdate("vi.Audio.Codec", videoInfo.AudioStreams[0].Codec?.EmptyAsNull());
                variables.AddOrUpdate("vi.Audio.Channels", videoInfo.AudioStreams[0].Channels > 0 ? (object)videoInfo.AudioStreams[0].Channels : null);
                variables.AddOrUpdate("vi.Audio.Language", videoInfo.AudioStreams[0].Language?.EmptyAsNull());
                variables.AddOrUpdate("vi.Audio.Codecs", string.Join(", ", videoInfo.AudioStreams.Select(x => x.Codec).Where(x => string.IsNullOrEmpty(x) == false)));
                variables.AddOrUpdate("vi.Audio.Languages", string.Join(", ", videoInfo.AudioStreams.Select(x => x.Language).Where(x => string.IsNullOrEmpty(x) == false)));
            }
            var resolution = ResolutionHelper.GetResolution(videoInfo.VideoStreams[0].Width, videoInfo.VideoStreams[0].Height);
            if(resolution == ResolutionHelper.Resolution.r1080p)
                variables.AddOrUpdate("vi.Resolution", "1080p");
            else if (resolution == ResolutionHelper.Resolution.r4k)
                variables.AddOrUpdate("vi.Resolution", "4K");
            else if (resolution == ResolutionHelper.Resolution.r720p)
                variables.AddOrUpdate("vi.Resolution", "720p");
            else if (resolution == ResolutionHelper.Resolution.r480p)
                variables.AddOrUpdate("vi.Resolution", "480p");
            else if (videoInfo.VideoStreams[0].Width < 900 && videoInfo.VideoStreams[0].Height < 800)
                variables.AddOrUpdate("vi.Resolution", "SD");
            else
                variables.AddOrUpdate("vi.Resolution", videoInfo.VideoStreams[0].Width + "x" + videoInfo.VideoStreams[0].Height);

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





        private bool? HW_NVIDIA_265;
        /// <summary>
        /// Can process NVIDIA h265 hardware encoding
        /// </summary>
        /// <returns>true if can support NVIDIA h265 hardware encoding</returns>
        protected bool SupportsHardwareNvidia265()
        {
            if (HW_NVIDIA_265 == null)
                HW_NVIDIA_265 = CanProcessEncoder("hevc_nvenc");
            return HW_NVIDIA_265.Value;
        }
        
        private bool? HW_NVIDIA_264;
        /// <summary>
        /// Can process NVIDIA h264 hardware encoding
        /// </summary>
        /// <returns>true if can support NVIDIA h264 hardware encoding</returns>
        protected bool SupportsHardwareNvidia264()
        {
            if (HW_NVIDIA_264 == null)
                HW_NVIDIA_264 = CanProcessEncoder("h264_nvenc");
            return HW_NVIDIA_264.Value;
        }
        
        
        private bool? HW_QSV_265;
        /// <summary>
        /// Can process QSV h265 hardware encoding
        /// </summary>
        /// <returns>true if can support QSV h265 hardware encoding</returns>
        protected bool SupportsHardwareQsv265()
        {
            if (HW_QSV_265 == null)
                HW_QSV_265 = CanProcessEncoder("hevc_qsv");
            return HW_QSV_265.Value;
        }
        private bool? HW_QSV_264;
        /// <summary>
        /// Can process QSV h264 hardware encoding
        /// </summary>
        /// <returns>true if can support QSV h264 hardware encoding</returns>
        protected bool SupportsHardwareQsv264()
        {
            if (HW_QSV_264 == null)
                HW_QSV_264 = CanProcessEncoder("h264_qsv");
            return HW_QSV_264.Value;
        }
        
        public bool CanProcessEncoder(string encodingParams)
        {
            //ffmpeg -loglevel error -f lavfi -i color=black:s=1080x1080 -vframes 1 -an -c:v hevc_nven2c -preset hq -f null -"

            string cmdArgs = $"-loglevel error -f lavfi -i color=black:s=1080x1080 -vframes 1 -an -c:v {encodingParams} -f null -\"";
            var cmd = Args.Process.ExecuteShellCommand(new ExecuteArgs
            {
                Command = FFMPEG,
                Arguments = cmdArgs,
                Silent = true
            }).Result;
            if (cmd.ExitCode != 0 || string.IsNullOrWhiteSpace(cmd.Output) == false)
            {
                Args.Logger?.WLog($"Cant process '{encodingParams}': {cmd.Output ?? ""}");
                return false;
            }
            return true;
        }
    }
}