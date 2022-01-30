namespace FileFlows.VideoNodes
{
    using System.ComponentModel;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public abstract class EncodingNode : VideoNode
    {
        public override int Outputs => 2;
        public override int Inputs => 1;
        public override FlowElementType Type => FlowElementType.Process;

        protected TimeSpan TotalTime;

        protected NodeParameters args;

        private FFMpegEncoder Encoder;

        protected bool Encode(NodeParameters args, string ffmpegExe, string ffmpegParameters, string extension = "mkv", string outputFile = "", bool updateWorkingFile = true, bool dontAddInputFile = false)
        {
            if (string.IsNullOrEmpty(extension))
                extension = "mkv";

            this.args = args;
            Encoder = new FFMpegEncoder(ffmpegExe, args.Logger);
            Encoder.AtTime += AtTimeEvent;

            if (string.IsNullOrEmpty(outputFile))
                outputFile = Path.Combine(args.TempPath, Guid.NewGuid().ToString() + "." + extension);

            if (TotalTime.TotalMilliseconds == 0)
            {
                VideoInfo videoInfo = GetVideoInfo(args);
                if (videoInfo != null)
                {
                    TotalTime = videoInfo.VideoStreams[0].Duration;
                    args.Logger.ILog("### Total Time: " + TotalTime);
                }
            }

            bool success = Encoder.Encode(args.WorkingFile, outputFile, ffmpegParameters, dontAddInputFile: dontAddInputFile);
            args.Logger.ILog("Encoding successful: " + success);
            if (success && updateWorkingFile)
            {
                args.SetWorkingFile(outputFile);

                // get the new video info
                var videoInfo = new VideoInfoHelper(ffmpegExe, args.Logger).Read(outputFile);
                var newVariables = new Dictionary<string, object>();
                SetVideoInfo(args, videoInfo, newVariables);
                args.UpdateVariables(newVariables);
            }
            Encoder.AtTime -= AtTimeEvent;
            Encoder = null;
            return success;
        }

        public override Task Cancel()
        {
            if (Encoder != null)
                Encoder.Cancel();
            return base.Cancel();
        }

        void AtTimeEvent(TimeSpan time)
        {
            if (TotalTime.TotalMilliseconds == 0)
            {
                args?.Logger?.DLog("Can't report time progress as total time is 0");
                return;
            }
            float percent = (float)((time.TotalMilliseconds / TotalTime.TotalMilliseconds) * 100);
            if(args?.PartPercentageUpdate != null)
                args.PartPercentageUpdate(percent);
        }


#if (DEBUG)
        /// <summary>
        /// Used for unit tests
        /// </summary>
        /// <param name="args">the args</param>
        public void SetArgs(NodeParameters args)
        {
            this.args = args;
        }
#endif

        public string CheckVideoCodec(string ffmpeg, string vidparams)
        {
            if (string.IsNullOrEmpty(vidparams))
                return string.Empty;

            if(vidparams.ToLower() == "hevc" || vidparams.ToLower() == "h265")
            {
                // try find best hevc encoder
                foreach(string vidparam in new [] { "hevc_nvenc -preset hq", "hevc_qsv -load_plugin hevc_hw", "hevc_amf", "hevc_vaapi" })
                {
                    bool canProcess = CanProcessEncoder(ffmpeg, vidparam);
                    if (canProcess)
                        return vidparam;
                }
                return "libx265";
            }
            if (vidparams.ToLower() == "h264")
            {
                // try find best hevc encoder
                foreach (string vidparam in new[] { "h264_nvenc", "h264_qsv", "h264_amf", "h264_vaapi" })
                {
                    bool canProcess = CanProcessEncoder(ffmpeg, vidparam);
                    if (canProcess)
                        return vidparam;
                }
                return "libx264";
            }

            if (vidparams.ToLower().Contains("hevc_nvenc"))
            {
                // nvidia h265 encoding, check can
                bool canProcess = CanProcessEncoder(ffmpeg, vidparams);
                if (canProcess == false)
                {
                    // change to cpu encoding 
                    args.Logger?.ILog("Can't encode using hevc_nvenc, falling back to CPU encoding H265 (libx265)");
                    return "libx265";
                }
                return vidparams;
            }
            else if (vidparams.ToLower().Contains("h264_nvenc"))
            {
                // nvidia h264 encoding, check can
                bool canProcess = CanProcessEncoder(ffmpeg, vidparams);
                if (canProcess == false)
                {
                    // change to cpu encoding 
                    args.Logger?.ILog("Can't encode using h264_nvenc, falling back to CPU encoding H264 (libx264)");
                    return "libx264";
                }
                return vidparams;
            }
            else if (vidparams.ToLower().Contains("hevc_qsv"))
            {
                // nvidia h265 encoding, check can
                bool canProcess = CanProcessEncoder(ffmpeg, vidparams);
                if (canProcess == false)
                {
                    // change to cpu encoding 
                    args.Logger?.ILog("Can't encode using hevc_qsv, falling back to CPU encoding H265 (libx265)");
                    return "libx265";
                }
                return vidparams;
            }
            else if (vidparams.ToLower().Contains("h264_qsv"))
            {
                // nvidia h264 encoding, check can
                bool canProcess = CanProcessEncoder(ffmpeg, vidparams);
                if (canProcess == false)
                {
                    // change to cpu encoding 
                    args.Logger?.ILog("Can't encode using h264_qsv, falling back to CPU encoding H264 (libx264)");
                    return "libx264";
                }
                return vidparams;
            }
            return vidparams;
        }

        public bool CanProcessEncoder(string ffmpeg, string encodingParams)
        {
            //ffmpeg -loglevel error -f lavfi -i color=black:s=1080x1080 -vframes 1 -an -c:v hevc_nven2c -preset hq -f null -"

            string cmdArgs = $"-loglevel error -f lavfi -i color=black:s=1080x1080 -vframes 1 -an -c:v {encodingParams} -f null -\"";
            var cmd = args.Process.ExecuteShellCommand(new ExecuteArgs
            {
                Command = ffmpeg,
                Arguments = cmdArgs,
                Silent = true
            }).Result;
            if (cmd.ExitCode != 0 || string.IsNullOrWhiteSpace(cmd.Output) == false)
            {
                args.Logger?.WLog($"Cant prcoess '{encodingParams}': {cmd.Output ?? ""}");
                return false;
            }
            return true;
        }

        public bool HasNvidiaCard(string ffmpeg)
        {
            try
            {
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    var cmd = args.Process.ExecuteShellCommand(new ExecuteArgs
                    {
                        Command = "wmic",
                        Arguments = "path win32_VideoController get name"
                    }).Result;
                    if (cmd.ExitCode == 0)
                    {
                        // it worked
                        if (cmd.Output?.ToLower()?.Contains("nvidia") == false)
                            return false;
                    }
                }
                else
                {
                    // linux, crude method, look for nvidia in the /dev dir
                    var dir = new DirectoryInfo("/dev");
                    if (dir.Exists == false)
                        return false;

                    bool dev = dir.GetDirectories().Any(x => x.Name.ToLower().Contains("nvidia"));
                    if (dev == false)
                        return false;
                }

                // check cuda in ffmpeg itself
                var result = args.Process.ExecuteShellCommand(new ExecuteArgs
                {
                    Command = ffmpeg,
                    Arguments = "-hide_banner -init_hw_device list"
                }).Result;
                return result.Output?.Contains("cuda") == true;
            }
            catch (Exception ex)
            {
                args.Logger?.ELog("Failed to detect NVIDIA card: " + ex.Message + Environment.NewLine + ex.StackTrace);
                return false;
            }
        }

        protected bool IsSameVideoCodec(string current, string wanted)
        {
            wanted = ReplaceCommon(wanted);
            current = ReplaceCommon(current);

            return wanted == current;

            string ReplaceCommon(string input)
            {
                input = input.ToLower();
                input = Regex.Replace(input, "^(divx|xvid|m(-)?peg(-)4)$", "mpeg4", RegexOptions.IgnoreCase);
                input = Regex.Replace(input, "^(hevc|h[\\.x\\-]?265)$", "h265", RegexOptions.IgnoreCase);
                input = Regex.Replace(input, "^(h[\\.x\\-]?264)$", "h264", RegexOptions.IgnoreCase);
                return input;
            }
        }

        protected bool SupportsSubtitles(NodeParameters args, VideoInfo videoInfo, string extension)
        {
            if (videoInfo?.SubtitleStreams?.Any() != true)
                return false;
            bool mov_text = videoInfo.SubtitleStreams.Any(x => x.Codec == "mov_text");
            // if mov_text and going to mkv, we can't convert these subtitles
            if (mov_text && extension?.ToLower()?.EndsWith("mkv") == true)
                return false;
            return true;
            //if (Regex.IsMatch(container ?? string.Empty, "(mp(e)?(g)?4)|avi|divx|xvid", RegexOptions.IgnoreCase))
            //    return false;
            //return true;
        }
    }
}