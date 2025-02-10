namespace FileFlows.VideoNodes
{
    using System.ComponentModel;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public abstract class EncodingNode : VideoNode
    {
        /// <summary>
        /// Gets the number of outputs
        /// </summary>
        public override int Outputs => 2;
        /// <summary>
        /// Gets the number of inputs
        /// </summary>
        public override int Inputs => 1;
        /// <summary>
        /// Gets the type of flow element this is
        /// </summary>
        public override FlowElementType Type => FlowElementType.Process;

        protected TimeSpan TotalTime;

        private FFMpegEncoder Encoder;

        /// <summary>
        /// Encodes a video
        /// </summary>
        /// <param name="args">the node parameters</param>
        /// <param name="ffmpegExe">the FFmpeg executable</param>
        /// <param name="ffmpegParameters">the FFmpeg parameters</param>
        /// <param name="extension">the output fil extension</param>
        /// <param name="outputFile">the output file</param>
        /// <param name="updateWorkingFile">if the working file should be updated to the newly created file</param>
        /// <param name="dontAddInputFile">if the input file should not be added to the arguments</param>
        /// <param name="dontAddOutputFile">if the output file should not be added to the arguments</param>
        /// <param name="strictness">the strictness to use</param>
        /// <returns>true if the encode was successful</returns>
        public bool Encode(NodeParameters args, string ffmpegExe, List<string> ffmpegParameters, string extension = "mkv", string outputFile = "", bool updateWorkingFile = true, bool dontAddInputFile = false, bool dontAddOutputFile = false, string strictness = "-2")
        {
            string output;
            return Encode(args, ffmpegExe, ffmpegParameters, out output, extension, outputFile, updateWorkingFile, dontAddInputFile: dontAddInputFile, dontAddOutputFile: dontAddOutputFile, strictness: strictness);
        }

        /// <summary>
        /// Encodes a video
        /// </summary>
        /// <param name="args">the node parameters</param>
        /// <param name="ffmpegExe">the FFmpeg executable</param>
        /// <param name="ffmpegParameters">the FFmpeg parameters</param>
        /// <param name="output">the output from the command</param>
        /// <param name="extension">the output fil extension</param>
        /// <param name="outputFile">the output file</param>
        /// <param name="updateWorkingFile">if the working file should be updated to the newly created file</param>
        /// <param name="dontAddInputFile">if the input file should not be added to the arguments</param>
        /// <param name="dontAddOutputFile">if the output file should not be added to the arguments</param>
        /// <param name="strictness">the strictness to use</param>
        /// <returns>true if the encode was successful</returns>
        public bool Encode(NodeParameters args, string ffmpegExe, List<string> ffmpegParameters, out string output, string extension = "mkv", string outputFile = "", bool updateWorkingFile = true, bool dontAddInputFile = false, bool dontAddOutputFile = false, string strictness = "-2")
        {
            if (string.IsNullOrEmpty(extension))
                extension = "mkv";

            Encoder = new FFMpegEncoder(ffmpegExe, args.Logger);
            Encoder.AtTime += AtTimeEvent;
            Encoder.OnStatChange += EncoderOnOnStatChange;

            if (string.IsNullOrEmpty(outputFile))
                outputFile = System.IO.Path.Combine(args.TempPath, Guid.NewGuid() + "." + extension);

            if (TotalTime.TotalMilliseconds == 0)
            {
                VideoInfo videoInfo = GetVideoInfo(args);
                if (videoInfo != null)
                {
                    TotalTime = videoInfo.VideoStreams[0].Duration;
                    args.Logger.ILog("### Total Run-Time Of Video: " + TotalTime);
                }
            }

            var success = Encoder.Encode(args.WorkingFile, outputFile, ffmpegParameters, dontAddInputFile: dontAddInputFile, dontAddOutputFile: dontAddOutputFile, strictness: strictness);
            args.Logger.ILog("Encoding successful: " + success.successs);
            if (success.successs && updateWorkingFile)
            {
                args.SetWorkingFile(outputFile);

                // get the new video info
                var videoInfo = new VideoInfoHelper(ffmpegExe, args.Logger).Read(outputFile).ValueOrDefault;
                SetVideoInfo(args, videoInfo, this.Variables ?? new Dictionary<string, object>());
            }
            else if (success.successs == false)
            {
                if (string.IsNullOrWhiteSpace(success.abortReason) == false)
                {
                    args.FailureReason = success.abortReason;
                }
                else
                {
                    // look for known error messages
                    var lines = success.output
                        .Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                        .ToArray();

                    if (lines.Count() >= 50)
                    {
                        lines = lines.TakeLast(50).ToArray();
                    }

                    string line;
                    if (HasLine(lines, "is not supported by the bitstream filter", out line))
                    {
                        // get that line, more efficiently that twice
                        int codecIndex = line.IndexOf("Codec", StringComparison.InvariantCulture);
                        if (codecIndex > 0)
                            line = line[codecIndex..];
                        int periodIndex = line.IndexOf(".", StringComparison.InvariantCulture);
                        if (periodIndex > 0)
                            line = line[..periodIndex];
                        args.FailureReason = line;
                    }
                    else if (HasLine(lines, "codec not currently supported in container", out line))
                    {
                        if (line.Contains("codec ttf")) // add more as i see them
                            args.FailureReason = "Subtitle codec not currently supported in container";
                        else // generic
                            args.FailureReason = "Codec not currently supported in container";
                    }
                    else if (HasLine(lines,
                                 "encoding with ProRes Proxy/LT/422/422 HQ (apco, apcs, apcn, ap4h) profile, need YUV422P10 input",
                                 out line))
                    {
                        args.FailureReason =
                            "Encoding with ProRes Proxy/LT/422/422 HQ (apco, apcs, apcn, ap4h) profile, need YUV422P10 input";
                    }
                }
            }
            Encoder.AtTime -= AtTimeEvent;
            Encoder.OnStatChange -= EncoderOnOnStatChange;
            Encoder = null;
            output = success.output;
            return success.successs;

            bool HasLine(string[] lines, string text, out string line)
            {
                line = lines.FirstOrDefault(x => x.Contains(text));
                return line != null;
            }
        }

        public override Task Cancel()
        {
            if (Encoder != null)
                Encoder.Cancel();
            return base.Cancel();
        }

        void AtTimeEvent(TimeSpan time, DateTime startedAt)
        {
            if (TotalTime.TotalMilliseconds == 0)
            {
                //Args?.Logger?.DLog("Can't report time progress as total time is 0");
                return;
            }
            float percent = (float)((time.TotalMilliseconds / TotalTime.TotalMilliseconds) * 100);
            if(Args?.PartPercentageUpdate != null)
                Args.PartPercentageUpdate(percent);
            
            // Calculate ETA to reach 100%
            if (percent > 0)
            {
                TimeSpan elapsed = DateTime.Now - startedAt;
                double remainingMilliseconds = elapsed.TotalMilliseconds * (100 - percent) / percent;
                TimeSpan eta = TimeSpan.FromMilliseconds(remainingMilliseconds);

                Args?.AdditionalInfoRecorder?.Invoke("ETA", TimeHelper.ToHumanReadableString(eta), 1, new TimeSpan(0, 1, 0));
            }
        }

        private void EncoderOnOnStatChange(string name, string value, bool recordStatistic)
        {
            Args.AdditionalInfoRecorder?.Invoke(name, value, 1, new TimeSpan(0, 1, 0));
            if(recordStatistic)
                Args.RecordStatisticRunningTotals(name, value);
        }

        public string CheckVideoCodec(string ffmpeg, string vidparams)
        {
            if (string.IsNullOrEmpty(vidparams))
                return string.Empty;

            if(vidparams.ToLower() == "hevc" || vidparams.ToLower() == "h265")
            {
                // try find best hevc encoder
                foreach(string vidparam in new [] { "hevc_nvenc -preset hq", "hevc_qsv -global_quality 28 -load_plugin hevc_hw", "hevc_amf", "hevc_vaapi" })
                {
                    if (CanUseHardwareEncoding.DisabledByVariables(Args, vidparam.Split(' ')))
                        continue;

                    bool canProcess = CanUseHardwareEncoding.CanProcess(Args, ffmpeg, vidparam.Split(' '));
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
                    bool canProcess = CanUseHardwareEncoding.CanProcess(Args, ffmpeg, new [] { vidparam });
                    if (canProcess)
                        return vidparam;
                }
                return "libx264";
            }

            return vidparams;

            // no longer do this
            //if (vidparams.ToLower().Contains("hevc_nvenc"))
            //{
            //    // nvidia h265 encoding, check can
            //    bool canProcess = CanUseHardwareEncoding.CanProcess(Args, ffmpeg, vidparams);
            //    if (canProcess == false)
            //    {
            //        // change to cpu encoding 
            //        Args.Logger?.ILog("Can't encode using hevc_nvenc, falling back to CPU encoding H265 (libx265)");
            //        return "libx265";
            //    }
            //    return vidparams;
            //}
            //else if (vidparams.ToLower().Contains("h264_nvenc"))
            //{
            //    // nvidia h264 encoding, check can
            //    bool canProcess = CanUseHardwareEncoding.CanProcess(Args, ffmpeg, vidparams);
            //    if (canProcess == false)
            //    {
            //        // change to cpu encoding 
            //        Args.Logger?.ILog("Can't encode using h264_nvenc, falling back to CPU encoding H264 (libx264)");
            //        return "libx264";
            //    }
            //    return vidparams;
            //}
            //else if (vidparams.ToLower().Contains("hevc_qsv"))
            //{
            //    // nvidia h265 encoding, check can
            //    bool canProcess = CanUseHardwareEncoding.CanProcess(Args, ffmpeg, vidparams);
            //    if (canProcess == false)
            //    {
            //        // change to cpu encoding 
            //        Args.Logger?.ILog("Can't encode using hevc_qsv, falling back to CPU encoding H265 (libx265)");
            //        return "libx265";
            //    }
            //    return vidparams;
            //}
            //else if (vidparams.ToLower().Contains("h264_qsv"))
            //{
            //    // nvidia h264 encoding, check can
            //    bool canProcess = CanUseHardwareEncoding.CanProcess(Args, ffmpeg, vidparams);
            //    if (canProcess == false)
            //    {
            //        // change to cpu encoding 
            //        Args.Logger?.ILog("Can't encode using h264_qsv, falling back to CPU encoding H264 (libx264)");
            //        return "libx264";
            //    }
            //    return vidparams;
            //}
            //return vidparams;
        }

        public bool HasNvidiaCard(string ffmpeg)
        {
            try
            {
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    var cmd = Args.Process.ExecuteShellCommand(new ExecuteArgs
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
                    var dir = new System.IO.DirectoryInfo("/dev");
                    if (dir.Exists == false)
                        return false;

                    bool dev = dir.GetDirectories().Any(x => x.Name.ToLower().Contains("nvidia"));
                    if (dev == false)
                        return false;
                }

                // check cuda in ffmpeg itself
                var result = Args.Process.ExecuteShellCommand(new ExecuteArgs
                {
                    Command = ffmpeg,
                    Arguments = "-hide_banner -init_hw_device list"
                }).Result;
                return result.Output?.Contains("cuda") == true;
            }
            catch (Exception ex)
            {
                Args.Logger?.ELog("Failed to detect NVIDIA card: " + ex.Message + Environment.NewLine + ex.StackTrace);
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