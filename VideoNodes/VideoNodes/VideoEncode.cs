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
            if (VideoCodec == "COPY")
                VideoCodec = string.Empty;
            if (AudioCodec == "COPY")
                AudioCodec = string.Empty;

            if (string.IsNullOrEmpty(VideoCodec) && string.IsNullOrEmpty(AudioCodec))
            {
                args.Logger?.ELog("Video codec or Audio codec must be set");
                return -1;
            }

            VideoCodec = args.ReplaceVariables(VideoCodec ?? string.Empty);
            VideoCodecParameters = args.ReplaceVariables(VideoCodecParameters ?? string.Empty);
            AudioCodec = args.ReplaceVariables(AudioCodec ?? string.Empty);
            Language = args.ReplaceVariables(Language);

            VideoCodec = VideoCodec.ToLower();
            AudioCodec = AudioCodec.ToLower();

            this.args = args;
            try
            {
                VideoInfo videoInfo = GetVideoInfo(args);
                if (videoInfo == null)
                    return -1;

                Language = Language?.ToLower() ?? "";

                string ffmpegExe = GetFFMpegExe(args);
                if (string.IsNullOrEmpty(ffmpegExe))
                    return -1;

                // ffmpeg is one based for stream index, so video should be 1, audio should be 2

                string encodeVideoParameters = string.Empty, encodeAudioParameters = string.Empty;
                const string copyVideoStream = "-map 0:v -c:v copy";
                const string copyAudioStream = "-map 0:a -c:a copy";

                if (string.IsNullOrEmpty(VideoCodec) == false)
                {

                    var videoIsRightCodec = videoInfo.VideoStreams.FirstOrDefault(x => IsSameVideoCodec(x.Codec ?? string.Empty, VideoCodec));
                    var videoTrack = videoIsRightCodec ?? videoInfo.VideoStreams[0];
                    args.Logger?.ILog("Video: ", videoTrack);

                    string crop = args.GetParameter<string>(DetectBlackBars.CROP_KEY) ?? "";
                    if (crop != string.Empty)
                        crop = " -vf crop=" + crop;

                    if (videoIsRightCodec == null || crop != string.Empty)
                    {
                        string codecParameters = CheckVideoCodec(ffmpegExe, VideoCodecParameters);
                        encodeVideoParameters = $"-map 0:v:0 -c:v {codecParameters} {crop}";
                    }
                    Extension = args.ReplaceVariables(Extension)?.EmptyAsNull() ?? "mkv";
                }
                else if(string.IsNullOrEmpty(Extension) == false)
                {
                    // vidoe is being copied so use the same extension
                    Extension = new FileInfo(args.WorkingFile).Extension;
                    if(Extension.StartsWith("."))
                        Extension = Extension.Substring(1);
                }


                if (string.IsNullOrEmpty(AudioCodec) == false)
                {

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
                    .ThenBy(x => x.Index)
                    .FirstOrDefault();

                    bool audioRightCodec = bestAudio?.Codec?.ToLower() == AudioCodec && videoInfo.AudioStreams[0] == bestAudio;
                    args.Logger?.ILog("Best Audio: ", bestAudio == null ? "null" : (object)bestAudio);


                    if (audioRightCodec == false)
                        encodeAudioParameters = $"-map 0:{bestAudio!.Index} -c:a {AudioCodec}";
                    else if(videoInfo.AudioStreams.Count > 1)
                        encodeAudioParameters = $"-map 0:{bestAudio!.Index} -c:a copy";
                }

                if(string.IsNullOrEmpty(encodeVideoParameters) && string.IsNullOrEmpty(encodeAudioParameters))
                {
                    args.Logger?.ILog("Video and Audio does not need to be reencoded");
                    return 2;
                }


                List<string> ffArgs = new List<string>();

                ffArgs.Add(encodeVideoParameters?.EmptyAsNull() ?? copyVideoStream);
                ffArgs.Add(encodeAudioParameters?.EmptyAsNull() ?? copyAudioStream);

                TotalTime = videoInfo.VideoStreams[0].Duration;

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