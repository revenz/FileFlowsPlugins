namespace FileFlows.VideoNodes
{
    using FileFlows.Plugin;
    using System.Text.Json;

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
        /// Executed before execute, sets ffmpeg executable etc
        /// </summary>
        /// <param name="args">the node parameters</param>
        /// <returns>true if successfully</returns>
        public override bool PreExecute(NodeParameters args)
        {
            this.Args = args;
            this.FFMPEG = GetFFmpegExecutable();
            return string.IsNullOrEmpty(this.FFMPEG) == false;
        }

        private string GetFFmpegExecutable()
        {
            string ffmpeg = Args.GetToolPath("FFmpeg")?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(ffmpeg))
            {
                Args.Logger.ELog("FFmpeg variable not found.");
                return string.Empty;
            }
            var fileInfo = new System.IO.FileInfo(ffmpeg);
            if (fileInfo.Exists == false)
            {
                Args.Logger.ELog("FFmpeg does not exist: " + ffmpeg);
                return string.Empty;
            }
            return fileInfo.FullName;
        }

        /// <summary>
        /// Gets the FFprobe location
        /// </summary>
        /// <param name="args">the node parameters</param>
        /// <returns>the FFprobe location</returns>
        protected Result<string> GetFFprobe(NodeParameters args)
        {
            string ffmpeg = args.GetToolPath("FFprobe");
            if (string.IsNullOrEmpty(ffmpeg))
                return Result<string>.Fail("FFprobe tool not found.");
            
            var fileInfo = new System.IO.FileInfo(ffmpeg);
            if (fileInfo.Exists == false)
                return Result<string>.Fail("FFprobe tool configured by ffmpeg file does not exist.");
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

        internal const string VIDEO_INFO = "VideoInfo";
        protected void SetVideoInfo(NodeParameters args, VideoInfo videoInfo, Dictionary<string, object> variables)
        {
            if (videoInfo.VideoStreams?.Any() == false)
                return;

            args.Parameters[VIDEO_INFO] = videoInfo;

            if (args.Variables.ContainsKey("vi.OriginalDuration") == false) // we only want to store this for the absolute original duration in the flow
                args.Variables["vi.OriginalDuration"] = videoInfo.VideoStreams[0].Duration;

            variables["vi.VideoInfo"] = videoInfo;
            variables["vi.Width"] = videoInfo.VideoStreams[0].Width;
            variables["vi.Height"] = videoInfo.VideoStreams[0].Height;
            variables["vi.Duration"] = videoInfo.VideoStreams[0].Duration.TotalSeconds;
            variables["vi.Video.Codec"] = videoInfo.VideoStreams[0].Codec;
            if (videoInfo.AudioStreams?.Any() == true)
            {
                variables["vi.Audio.Codec"] = videoInfo.AudioStreams[0].Codec?.EmptyAsNull();
                variables["vi.Audio.Channels"] = videoInfo.AudioStreams[0].Channels > 0 ? (object)videoInfo.AudioStreams[0].Channels : null;
                variables["vi.Audio.Language"] = videoInfo.AudioStreams[0].Language?.EmptyAsNull();
                variables["vi.Audio.Codecs"] = string.Join(", ", videoInfo.AudioStreams.Select(x => x.Codec).Where(x => string.IsNullOrEmpty(x) == false));
                variables["vi.Audio.Languages"] = string.Join(", ", videoInfo.AudioStreams.Select(x => x.Language).Where(x => string.IsNullOrEmpty(x) == false));
            }
            var resolution = ResolutionHelper.GetResolution(videoInfo.VideoStreams[0].Width, videoInfo.VideoStreams[0].Height);
            if(resolution == ResolutionHelper.Resolution.r1080p)
                variables["vi.Resolution"] = "1080p";
            else if (resolution == ResolutionHelper.Resolution.r4k)
                variables["vi.Resolution"] = "4K";
            else if (resolution == ResolutionHelper.Resolution.r720p)
                variables["vi.Resolution"] = "720p";
            else if (resolution == ResolutionHelper.Resolution.r480p)
                variables["vi.Resolution"] = "480p";
            else if (videoInfo.VideoStreams[0].Width < 900 && videoInfo.VideoStreams[0].Height < 800)
                variables["vi.Resolution"] = "SD";
            else
                variables["vi.Resolution"] = videoInfo.VideoStreams[0].Width + "x" + videoInfo.VideoStreams[0].Height;

            args.UpdateVariables(variables);

            var metadata = new Dictionary<string, object>();
            metadata.Add("Duration", videoInfo.VideoStreams[0].Duration);
            foreach (var (stream, i) in videoInfo.VideoStreams.Select((value, i) => (value, i)))
            {
                string prefix = "Video" + (i == 0 ? "" : " " + (i + 1)) + " ";
                metadata.Add(prefix + "Codec", stream.Codec);
                metadata.Add(prefix + "Resolution", stream.Width + "x" + stream.Height + (stream.HDR ? " (HDR)" : string.Empty));
                if(string.IsNullOrWhiteSpace(stream.PixelFormat) == false)
                    metadata.Add(prefix + "PixelFormat", stream.PixelFormat);
                if(stream.FramesPerSecond > 0)
                    metadata.Add(prefix + "FramesPerSecond", stream.FramesPerSecond);
                if(stream.Bitrate > 0)
                    metadata.Add(prefix + "Bitrate", stream.Bitrate);
                if(stream.HDR)
                    metadata.Add(prefix + "HDR", true);
                if(stream.DolbyVision)
                    metadata.Add(prefix + "DolbyVision", true);
                if(stream.Bits == 8)
                    metadata.Add(prefix + "Bits", "8 Bit");
                else if(stream.Bits == 10)
                    metadata.Add(prefix + "Bits", "10 Bit");
                else if(stream.Bits == 12)
                    metadata.Add(prefix + "Bits", "12 Bit");
            }
            foreach (var (stream, i) in videoInfo.AudioStreams.Select((value, i) => (value, i)))
            {
                string prefix = "Audio" + (i == 0 ? "" : " " + (i + 1)) + " ";
                metadata.Add(prefix + "Codec", stream.Codec);
                metadata.Add(prefix + "Channels", stream.Channels);
                if (string.IsNullOrEmpty(stream.Title) == false)
                    metadata.Add(prefix + "Title", stream.Title);
                if(string.IsNullOrEmpty(stream.Language) == false)
                    metadata.Add(prefix + "Language", stream.Language);
                if(stream.Default)
                    metadata.Add(prefix + "Default", true);
                if (stream.Bitrate > 0)
                    metadata.Add(prefix + "Bitrate", stream.Bitrate);
            }
            foreach (var (stream, i) in videoInfo.SubtitleStreams.Select((value, i) => (value, i)))
            {
                string prefix = "Subtitle" + (i == 0 ? "" : " " + (i + 1)) + " ";
                metadata.Add(prefix + "Codec", stream.Codec);
                if (string.IsNullOrEmpty(stream.Title) == false)
                    metadata.Add(prefix + "Title", stream.Title);
                if (string.IsNullOrEmpty(stream.Language) == false)
                    metadata.Add(prefix + "Language", stream.Language);
                if(stream.Default)
                    metadata.Add(prefix + "Default", true);
                if(stream.Forced)
                    metadata.Add(prefix + "Forced", true);
            }
            args.SetMetadata(metadata);
        }

        protected VideoInfo GetVideoInfo(NodeParameters args, bool refreshIfFileChanged = true)
        {
            var vi = GetVideoInfoActual(args);
            if(vi == null) return null;

            if (refreshIfFileChanged == false || vi.FileName == args.FileName)
                return vi;

            var local = args.FileService.GetLocalPath(args.WorkingFile);
            if (local.IsFailed)
            {
                args.Logger?.ELog("Failed to get local file: " + local.Error);
                return null;
            }

            var viResult = new VideoInfoHelper(FFMPEG, args.Logger).Read(local);
            if (viResult.Failed(out string error))
            {
                args.Logger?.ELog(error);
                return null;
            }

            vi = viResult.Value;
            SetVideoInfo(args, vi, Variables);
            return vi;
        }

        private VideoInfo GetVideoInfoActual(NodeParameters args)
        {
            if (args.Parameters.ContainsKey(VIDEO_INFO) == false)
            {
                args.Logger.WLog("No codec information loaded, use a 'VideoFile' flow element first");
                return null;
            }

            if (args.Parameters[VIDEO_INFO] == null)
            {
                args.Logger.WLog("VideoInfo not found for file");
                return null;
            }
            var result = args.Parameters[VIDEO_INFO] as VideoInfo;
            if (result != null)
                return result;

            // may be from non Legacy VideoNodes
            try
            {
#pragma warning disable IL2026
                string json = JsonSerializer.Serialize(args.Parameters[VIDEO_INFO]);
                var vi = JsonSerializer.Deserialize<VideoInfo>(json);
#pragma warning restore IL2026
                if (vi == null)
                    throw new Exception("Failed to deserailize object");
                return vi;

            }
            catch (Exception ex)
            {
                args.Logger.WLog("VideoInfo could not be deserialized: " + ex.Message);
                return null;
            }
        }


   
    }
}