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

            object? infinity = null;
            if (OperatingSystem.IsWindows() && args.Variables.TryGetValue("FFmpegInfinity", out infinity) && infinity != null )
            {
                args.Logger?.ILog("Got FFmpeg Infinity: " + infinity);
            }
            
            Encoder = new FFMpegEncoder(ffmpegExe, args.Logger, args.CancellationToken);
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
            var success = Encoder.Encode(args.WorkingFile, outputFile, ffmpegParameters,
                dontAddInputFile: dontAddInputFile, dontAddOutputFile: dontAddOutputFile, strictness: strictness, 
                infinity: infinity);
            
            Encoder.AtTime -= AtTimeEvent;
            Encoder.OnStatChange -= EncoderOnOnStatChange;
            Encoder = null;
            
            args.Logger.ILog("Encoding successful: " + success.successs);
            if (success.successs && updateWorkingFile)
            {
                if (System.IO.File.Exists(outputFile) == false)
                {
                    args.FailureReason = "File does not exist after encoding: " + outputFile;
                    args.Logger.ELog(args.FailureReason);
                }
                else
                {
                    args.SetWorkingFile(outputFile);

                    // get the new video info
                    args.Logger.ILog("Reading new video information");
                    var result = new VideoInfoHelper(ffmpegExe, args.Logger, args.Process).Read(outputFile);
                    if (result.Failed(out string error))
                        args.Logger.WLog("Failed reading video information: " + error);
                    else if (result.Success(out var videoInfo) && videoInfo != null)
                    {
                        args.Logger.ILog("Setting new video information");
                        SetVideoInfo(args, videoInfo, this.Variables ?? new Dictionary<string, object>());
                    }
                    else
                    {
                        args.Logger.WLog("Video information was null");
                    }
                }
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
                        if (line.Contains("mp4") && line.Contains("codec ttf"))
                            args.FailureReason = "MP4 format does not support embedding TrueType font (TTF) streams. Remove the TTF attachment or use a different container format.";
                        else if (line.Contains("codec ttf")) // add more as i see them
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
            output = success.output;
            return success.successs;

            bool HasLine(string[] lines, string text, out string line)
            {
                line = lines.FirstOrDefault(x => x.Contains(text));
                return line != null;
            }
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
                try
                {
                    TimeSpan elapsed = DateTime.Now - startedAt;
                    double remainingMilliseconds = elapsed.TotalMilliseconds * (100 - percent) / percent;
                    TimeSpan eta = TimeSpan.FromMilliseconds(remainingMilliseconds);

                    Args?.AdditionalInfoRecorder?.Invoke("ETA", TimeHelper.ToHumanReadableString(eta), 1,
                        new TimeSpan(0, 1, 0));
                }
                catch (Exception)
                {
                    // Ignored
                }
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
        }
    }
}