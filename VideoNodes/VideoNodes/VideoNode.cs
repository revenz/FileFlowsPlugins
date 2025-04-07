using FileFlows.VideoNodes.Helpers;

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
        /// <inheritdoc />
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
                Args.FailureReason = "FFmpeg variable not found.";
                return string.Empty;
            }
            var fileInfo = new System.IO.FileInfo(ffmpeg);
            if (fileInfo.Exists == false)
            {
                Args.Logger.ELog("FFmpeg does not exist: " + ffmpeg);
                Args.FailureReason = "FFmpeg does not exist: " + ffmpeg;
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
            var firstVideo = videoInfo?.VideoStreams?.FirstOrDefault();
            if (firstVideo == null)
                return;


            args.Logger.ILog("Setting traits");
            args.SetTraits(new[]
            {
                firstVideo.Codec?.ToUpper(),
                videoInfo.AudioStreams?.FirstOrDefault()?.Codec?.ToUpper(),
                ChannelHelper.FormatAudioChannels(videoInfo.AudioStreams?.FirstOrDefault()?.Channels ?? 0),
                VideoHelper.FormatResolution(firstVideo.Width, firstVideo.Height),
                firstVideo.HDR == true ? "HDR" : null,
                firstVideo.DolbyVision == true ? "Dolby Vision" : null,
            }.Where(x => string.IsNullOrWhiteSpace(x) == false).ToArray());
            
            args.Logger.ILog("Setting Video Info");
            args.Parameters[VIDEO_INFO] = videoInfo;

            if (args.Variables.ContainsKey("vi.OriginalDuration") == false) // we only want to store this for the absolute original duration in the flow
                args.Variables["vi.OriginalDuration"] = videoInfo.VideoStreams[0].Duration;

            args.Variables["vi.VideoInfo"] = videoInfo;
            args.Logger.ILog("Setting Video stream information");
            var videoVariables = new Dictionary<string, object>();
            videoVariables["vi.Width"] = videoInfo.VideoStreams[0].Width;
            videoVariables["vi.Height"] = videoInfo.VideoStreams[0].Height;
            videoVariables["vi.Duration"] = videoInfo.VideoStreams[0].Duration.TotalSeconds;
            videoVariables["vi.Video.Codec"] = videoInfo.VideoStreams[0].Codec;
            videoVariables["vi.Codec"] = videoInfo.VideoStreams[0].Codec; // assume they want the Videos codec here
            if (videoInfo.AudioStreams?.Any() == true)
            {
                args.Logger.ILog("Setting Video audio information");
                videoVariables["vi.Audio.Codec"] = videoInfo.AudioStreams[0].Codec?.EmptyAsNull();
                videoVariables["vi.Audio.Channels"] = videoInfo.AudioStreams[0].Channels > 0 ? (object)videoInfo.AudioStreams[0].Channels : null;
                videoVariables["vi.Audio.Language"] = videoInfo.AudioStreams[0].Language?.EmptyAsNull();
                videoVariables["vi.Audio.Codecs"] = string.Join(", ", videoInfo.AudioStreams.Select(x => x.Codec).Where(x => string.IsNullOrEmpty(x) == false));
                videoVariables["vi.Audio.Languages"] = string.Join(", ", videoInfo.AudioStreams.Select(x => x.Language).Where(x => string.IsNullOrEmpty(x) == false));
            }
            args.Logger.ILog("Setting Video resolution");
            var resolution = ResolutionHelper.GetResolution(videoInfo.VideoStreams[0].Width, videoInfo.VideoStreams[0].Height);
            if(resolution == ResolutionHelper.Resolution.r1080p)
                videoVariables["vi.Resolution"] = "1080p";
            else if (resolution == ResolutionHelper.Resolution.r4k)
                videoVariables["vi.Resolution"] = "4K";
            else if (resolution == ResolutionHelper.Resolution.r720p)
                videoVariables["vi.Resolution"] = "720p";
            else if (resolution == ResolutionHelper.Resolution.r480p)
                videoVariables["vi.Resolution"] = "480p";
            else if (videoInfo.VideoStreams[0].Width < 900 && videoInfo.VideoStreams[0].Height < 800)
                videoVariables["vi.Resolution"] = "SD";
            else
                videoVariables["vi.Resolution"] = videoInfo.VideoStreams[0].Width + "x" + videoInfo.VideoStreams[0].Height;
            videoVariables["vi.FramesPerSecond"] = videoInfo.VideoStreams[0].FramesPerSecond;
            videoVariables["vi.FPS"] = videoInfo.VideoStreams[0].FramesPerSecond;
            videoVariables["vi.HDR"] = videoInfo.VideoStreams[0].HDR;

            args.Logger.ILog("Setting Video variables");
            foreach (var vv in videoVariables)
            {
                args.Variables[vv.Key] = vv.Value;
                // so the same variable is added as "video."
                args.Variables["video." + vv.Key[3..]] = vv.Value;
            }
            

            args.Logger.ILog("Setting metadata");
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
            args.Logger.ILog("Setting audio metadata");
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
            args.Logger.ILog("Setting subtitle metadata");
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

            args.Logger.ILog("Setting mimetype");
            string extension = FileHelper.GetExtension(videoInfo.FileName).ToLowerInvariant()[1..]; 
            switch (extension)
            {
                case "mp4":
                    args.SetMimeType("video/mp4");
                    break;
                case "avi":
                    args.SetMimeType("video/x-msvideo");
                    break;
                case "mkv":
                    args.SetMimeType("video/x-matroska");
                    break;
                case "mov":
                    args.SetMimeType("video/quicktime");
                    break;
                case "wmv":
                    args.SetMimeType("video/x-ms-wmv");
                    break;
                case "flv":
                    args.SetMimeType("video/x-flv");
                    break;
                case "webm":
                    args.SetMimeType("video/webm");
                    break;
                case "3gp":
                    args.SetMimeType("video/3gpp");
                    break;
                case "mpeg":
                case "mpg":
                    args.SetMimeType("video/mpeg");
                    break;
                case "ogg":
                    args.SetMimeType("video/ogg");
                    break;
                case "m4v":
                    args.SetMimeType("video/x-m4v");
                    break;
                case "ts":
                    args.SetMimeType("video/mp2t");
                    break;
                case "mpx":
                    args.SetMimeType("video/mpx");
                    break;
                default:
                    args.SetMimeType("video/" + extension); // Fallback for unknown extensions
                    break;
            }

            args.Logger.ILog("Setting metadata");
            args.SetMetadata(metadata);
        }

        private int Test(ILogger Logger)
        {
            
            var MAX_BITRATE = 3_000_000; // bitrate is 3,000 KBps

            if(Variables.TryGetValue("vi.VideoInfo", out var oVideoInfo) == false || oVideoInfo is FileFlows.VideoNodes.VideoInfo videoInfo == false)
            {
                Logger.ILog("Failed to locate VideoInformation in variables");
                return -1;
            }
            Logger.ILog("Got video information.");

            var video = videoInfo.VideoStreams.FirstOrDefault();
            if(video == null)
            {
                Logger.ILog("No video streams detected.");
                return -1;
            }

// get the video stream
            var bitrate = video.Bitrate;

            if(bitrate < 1)
            {
                // video stream doesn't have bitrate information
                // need to use the overall bitrate
                var overall = videoInfo.Bitrate;
                if(overall < 1)
                    return 0; // couldn't get overall bitrate either

                // overall bitrate includes all audio streams, so we try and subtract those
                var calculated = overall;
                if(videoInfo.AudioStreams.Count > 0) // check there are audio streams
                {
                    foreach(var audio in videoInfo.AudioStreams)
                    {
                        if(audio.Bitrate > 0)
                            calculated -= audio.Bitrate;
                        else{
                            // audio doesn't have bitrate either, so we just subtract 5% of the original bitrate
                            // this is a guess, but it should get us close
                            calculated -= (overall * 0.05f);
                        }
                    }
                }
                bitrate = calculated;
            }

// check if the bitrate is over the maximum bitrate
            if(bitrate > MAX_BITRATE)
                return 1; // it is, so call output 1
            return 2; // it isn't so call output 2
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

            var viResult = new VideoInfoHelper(FFMPEG, args.Logger, args.Process).Read(local);
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
                string json = JsonSerializer.Serialize(args.Parameters[VIDEO_INFO]);
                var vi = JsonSerializer.Deserialize<VideoInfo>(json);
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