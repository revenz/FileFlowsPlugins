namespace FileFlows.VideoNodes
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public class VideoEncode : EncodingNode
    {

        [DefaultValue("hevc")]
        [TextVariable(1)]
        public string VideoCodec { get; set; }


        [DefaultValue("hevc_nvenc -preset hq -crf 23")]
        [TextVariable(2)]
        public string VideoCodecParameters { get; set; }

        [DefaultValue("ac3")]
        [TextVariable(3)]
        public string AudioCodec { get; set; }

        [DefaultValue("eng")]
        [TextVariable(4)]
        public string Language { get; set; }

        [DefaultValue("mkv")]
        [TextVariable(5)]
        public string Extension { get; set; }

        public override string Icon => "far fa-file-video";

        public override int Execute(NodeParameters args)
        {
            if (string.IsNullOrEmpty(VideoCodec))
            {
                args.Logger?.ELog("Video codec not set");
                return -1;
            }
            if (string.IsNullOrEmpty(AudioCodec))
            {
                args.Logger?.ELog("Audio codec not set");
                return -1;
            }
            VideoCodec = args.ReplaceVariables(VideoCodec);
            VideoCodecParameters = args.ReplaceVariables(VideoCodecParameters);
            AudioCodec = args.ReplaceVariables(AudioCodec);
            Language = args.ReplaceVariables(Language);
            Extension = args.ReplaceVariables(Extension)?.EmptyAsNull() ?? "mkv";

            VideoCodec = VideoCodec.ToLower();
            AudioCodec = AudioCodec.ToLower();

            this.args = args;
            try
            {
                VideoInfo videoInfo = GetVideoInfo(args);
                if (videoInfo == null)
                    return -1;

                Language = Language?.ToLower() ?? "";

                // ffmpeg is one based for stream index, so video should be 1, audio should be 2

                var videoIsRightCodec = videoInfo.VideoStreams.FirstOrDefault(x => IsSameVideoCodec(x.Codec ?? string.Empty, VideoCodec));
                var videoTrack = videoIsRightCodec ?? videoInfo.VideoStreams[0];
                args.Logger?.ILog("Video: ", videoTrack);

                var bestAudio = videoInfo.AudioStreams.Where(x => System.Text.Json.JsonSerializer.Serialize(x).ToLower().Contains("commentary") == false)
                .OrderBy(x =>
                {
                    if (Language != string.Empty)
                    {
                        args.Logger?.ILog("Language: " + x.Language, x);
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

                bool audioRightCodec = bestAudio?.Codec?.ToLower() == AudioCodec && videoInfo.AudioStreams[0] == bestAudio;
                args.Logger?.ILog("Best Audio: ", bestAudio == null ? "null" : (object)bestAudio);

                string crop = args.GetParameter<string>(DetectBlackBars.CROP_KEY) ?? "";
                if (crop != string.Empty)
                    crop = " -vf crop=" + crop;

                if (audioRightCodec == true && videoIsRightCodec != null)
                {
                    if (crop == string.Empty)
                    {
                        args.Logger?.DLog($"File is {VideoCodec} with the first audio track is {AudioCodec}");
                        return 2;
                    }
                    else
                    {
                        args.Logger?.ILog($"Video is {VideoCodec} and audio is {AudioCodec} but needs to be cropped");
                    }
                }

                string ffmpegExe = GetFFMpegExe(args);
                if (string.IsNullOrEmpty(ffmpegExe))
                    return -1;

                List<string> ffArgs = new List<string>();

                if (videoIsRightCodec == null || crop != string.Empty)
                {
                    string codecParameters = CheckVideoCodec(ffmpegExe, VideoCodecParameters);
                    ffArgs.Add($"-map 0:v:0 -c:v {codecParameters} {crop}");
                }
                else
                    ffArgs.Add($"-map 0:v:0 -c:v copy");

                TotalTime = videoInfo.VideoStreams[0].Duration;

                if (audioRightCodec == false)
                    ffArgs.Add($"-map 0:{bestAudio!.Index} -c:a {AudioCodec}");
                else
                    ffArgs.Add($"-map 0:{bestAudio!.Index} -c:a copy");

                if (videoInfo?.SubtitleStreams?.Any() == true)
                {
                    if (SupportsSubtitles(args, videoInfo, Extension))
                    {
                        if (Language != string.Empty)
                            ffArgs.Add($"-map 0:s:m:language:{Language}? -c:s copy");
                        else
                            ffArgs.Add($"-map 0:s? -c:s copy");
                    }
                    else
                    {
                        args.Logger?.WLog("Unsupported subtitle for target container, subtitles will be removed.");
                    }
                }

                string ffArgsLine = string.Join(" ", ffArgs);

                if (Encode(args, ffmpegExe, ffArgsLine, Extension) == false)
                    return -1;

                return 1;
            }
            catch (Exception ex)
            {
                args.Logger?.ELog("Failed processing VideoFile: " + ex.Message);
                return -1;
            }
        }
    }
}