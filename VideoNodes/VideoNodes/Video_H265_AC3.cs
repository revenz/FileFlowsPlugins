namespace FileFlows.VideoNodes
{
    using System.ComponentModel;
    using System.Text.RegularExpressions;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public class Video_H265_AC3 : EncodingNode
    {

        [DefaultValue("eng")]
        [Text(1)]
        public string Language { get; set; }

        [DefaultValue(21)]
        [NumberInt(2)]
        public int Crf { get; set; }
        [DefaultValue(true)]
        [Boolean(3)]
        public bool NvidiaEncoding { get; set; }
        [DefaultValue(0)]
        [NumberInt(4)]
        public int Threads { get; set; }

        [DefaultValue(false)]
        [Boolean(5)]
        public bool NormalizeAudio { get; set; }


        [DefaultValue(false)]
        [Boolean(6)]
        public bool ForceRencode { get; set; }


        public override string Icon => "far fa-file-video";

        public override int Execute(NodeParameters args)
        {
            this.args = args;
            try
            {
                VideoInfo videoInfo = GetVideoInfo(args);
                if (videoInfo == null)
                    return -1;

                Language = Language?.ToLower() ?? "";

                // ffmpeg is one based for stream index, so video should be 1, audio should be 2

                var videoH265 = videoInfo.VideoStreams.FirstOrDefault(x => Regex.IsMatch(x.Codec ?? "", @"^(hevc|h(\.)?265)$", RegexOptions.IgnoreCase));
                var videoTrack = videoH265 ?? videoInfo.VideoStreams[0];
                args.Logger.ILog("Video: ", videoTrack);

                var bestAudio = videoInfo.AudioStreams.Where(x => System.Text.Json.JsonSerializer.Serialize(x).ToLower().Contains("commentary") == false)
                .OrderBy(x =>
                {
                    if (Language != string.Empty)
                    {
                        args.Logger.ILog("Language: " + x.Language, x);
                        if (string.IsNullOrEmpty(x.Language))
                            return 50; // no language specified
                        if (x.Language?.ToLower() != Language)
                            return 100; // low priority not the desired language
                    }
                    return 0;
                })
                .ThenByDescending(x => x.Channels)
                //.ThenBy(x => x.CodecName.ToLower() == "ac3" ? 0 : 1) // if we do this we can get commentary tracks...
                .ThenBy(x => x.Index)
                .FirstOrDefault();

                bool firstAc3 = bestAudio?.Codec?.ToLower() == "ac3" && videoInfo.AudioStreams[0] == bestAudio;
                args.Logger.ILog("Best Audio: ", bestAudio == null ? (object)"null" : (object)bestAudio);


                string crop = args.GetParameter<string>(DetectBlackBars.CROP_KEY) ?? "";
                if (crop != string.Empty)
                    crop = " -vf crop=" + crop;

                if (ForceRencode == false && firstAc3 == true && videoH265 != null)
                {
                    if (crop == string.Empty)
                    {
                        args.Logger.DLog("File is hevc with the first audio track being AC3");
                        return 2;
                    }
                    else
                    {
                        args.Logger.ILog("Video is hevc and ac3 but needs to be cropped");
                    }
                }

                string ffmpegExe = GetFFMpegExe(args);
                if (string.IsNullOrEmpty(ffmpegExe))
                    return -1;

                List<string> ffArgs = new List<string>();

                if (NvidiaEncoding == false && Threads > 0)
                    ffArgs.Add($"-threads {Math.Min(Threads, 16)}");

                if (videoH265 == null || crop != string.Empty)
                    ffArgs.Add($"-map 0:v:0 -c:v {(NvidiaEncoding ? "hevc_nvenc -preset hq" : "libx265")} -crf " + (Crf > 0 ? Crf : 21) + crop);
                else
                    ffArgs.Add($"-map 0:v:0 -c:v copy");

                TotalTime = videoInfo.VideoStreams[0].Duration;

                if (NormalizeAudio)
                {
                    int sampleRate = bestAudio.SampleRate > 0 ? bestAudio.SampleRate : 48_000;
                    ffArgs.Add($"-map 0:{bestAudio.Index} -c:a ac3 -ar {sampleRate} -af loudnorm=I=-24:LRA=7:TP=-2.0");
                }
                else if (bestAudio.Codec.ToLower() != "ac3")
                    ffArgs.Add($"-map 0:{bestAudio.Index} -c:a ac3");
                else
                    ffArgs.Add($"-map 0:{bestAudio.Index} -c:a copy");

                if (Language != string.Empty)
                    ffArgs.Add($"-map 0:s:m:language:{Language}? -c:s copy");
                else
                    ffArgs.Add($"-map 0:s? -c:s copy");

                string ffArgsLine = string.Join(" ", ffArgs);

                if (Encode(args, ffmpegExe, ffArgsLine) == false)
                    return -1;

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