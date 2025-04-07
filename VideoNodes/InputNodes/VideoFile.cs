using System.Diagnostics;

namespace FileFlows.VideoNodes;

public class VideoFile : VideoNode
{
    public override int Outputs => 1;
    public override FlowElementType Type => FlowElementType.Input;

    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/video-file";

    public override bool NoEditorOnAdd => true;

    /// <summary>
    /// Gets or sets the probe size in MegaBytes
    /// </summary>
    [DefaultValue(25)]
    [NumberInt(1)]
    [Range(5, 5000)]
    public int ProbeSize { get; set; }

    /// <summary>
    /// Gets or sets how many seconds are analyzed to probe the input
    /// </summary>
    [DefaultValue(5)]
    [NumberInt(1)]
    [Range(1, 600)]
    public int AnalyzeDuration { get; set; }

    private Dictionary<string, object> _Variables;
    public override Dictionary<string, object> Variables => _Variables;
    public VideoFile()
    {
        _Variables = new Dictionary<string, object>()
        {
            { "video.Codec", "hevc" },
            { "video.Audio.Codec", "ac3" },
            { "video.Audio.Codecs", "ac3,aac"},
            { "video.Audio.Language", "eng" },
            { "video.Audio.Languages", "eng, mao" },
            { "video.Resolution", "1080p" },
            { "video.Duration", 1800 },
            { "video.VideoInfo", new VideoInfo() 
                {
                    Bitrate = 10_000_000,
                    VideoStreams = new List<VideoStream> {
                        new VideoStream { }
                    },
                    AudioStreams = new List<AudioStream> {
                        new AudioStream { }
                    },
                    SubtitleStreams = new List<SubtitleStream>
                    {
                        new SubtitleStream { }
                    }
                } 
            },
            { "video.Width", 1920 },
            { "video.Height", 1080 },
            { "video.FPS", 29.97f },
            { "video.HDR", true },
            { nameof(ProbeSize), 5_000_000 },
            { nameof(AnalyzeDuration), 25}
        };
    }

    public override int Execute(NodeParameters args)
    {
        PrintFFmpegVersion(args);
        VideoInfoHelper.ProbeSize = this.ProbeSize;
        VideoInfoHelper.AnalyzeDuration = (this.AnalyzeDuration > 1 ? AnalyzeDuration : 5) * 1_000_000;

        try
        {
            var file = args.FileService.GetLocalPath(args.WorkingFile);
            if (file.IsFailed)
            {
                args.FailureReason = "Failed getting local file: " + file.Error;
                args.Logger?.ELog(args.FailureReason);
                return -1;
            }
            
            var videoInfoResult = new VideoInfoHelper(FFMPEG, args.Logger, args.Process).Read(file);
            if (videoInfoResult.Failed(out string error))
            {
                args.FailureReason = error;
                args.Logger?.ELog(args.FailureReason);
                return -1;
            }

            var videoInfo = videoInfoResult.Value;
            if (videoInfo.VideoStreams.Any() == false)
            {
                args.FailureReason = "No video streams detected.";
                args.Logger?.ELog(args.FailureReason);
                return -1;
            }
            foreach (var vs in videoInfo.VideoStreams)
            {
                args.Logger.ILog($"Video stream '{vs.Codec}' '{vs.Index}'");
            }

            if (args.FileService.FileCreationTimeUtc(args.WorkingFile).Success(out DateTime creationTimeUtc))
                args.Variables["ORIGINAL_CREATE_UTC"] = creationTimeUtc;
            if (args.FileService.FileCreationTimeUtc(args.WorkingFile).Success(out DateTime lastWriteTimeUtc))
                args.Variables["ORIGINAL_LAST_WRITE_UTC"] = lastWriteTimeUtc;

            

            foreach (var stream in videoInfo.VideoStreams)
            {
                if (string.IsNullOrEmpty(stream.Codec) == false)
                {
                    args.RecordStatisticRunningTotals("CODEC", stream.Codec);
                    args.RecordStatisticRunningTotals("VIDEO_CODEC", stream.Codec);
                }
            }

            foreach (var stream in videoInfo.AudioStreams)
            {
                if (string.IsNullOrEmpty(stream.Codec) == false)
                {
                    args.RecordStatisticRunningTotals("CODEC", stream.Codec);
                    args.RecordStatisticRunningTotals("AUDIO_CODEC", stream.Codec);
                }
            }

            var resolution = ResolutionHelper.GetResolution(videoInfo);
            var resName = resolution switch
            {
                ResolutionHelper.Resolution.r1080p => "1080p",
                ResolutionHelper.Resolution.r480p => "480p",
                ResolutionHelper.Resolution.r720p => "720p",
                ResolutionHelper.Resolution.r4k => "4k",
                _ => null
            };

            if (resName != null)
            {
                args.Logger?.ILog("Video Resolution: " + resName);
                args.RecordStatisticRunningTotals("VIDEO_RESOLUTION", resName);
            }

            string extension = FileHelper.GetExtension(args.FileName).ToLowerInvariant().TrimStart('.');
            var container = extension switch
            {
                "mkv" => "MKV",
                "mk3d" => "MKV",
                "mp4" => "MP4",
                "mpeg4" => "MP4",
                "mp4v" => "MP4",
                "m4v" => "MP4",
                "mpg" => "MPEG",
                "mpeg" => "MPEG",
                "mov" => "MOV",
                "qt" => "MOV",
                "asf" => "ASF",
                "wmv" => "ASF",
                _ => extension.ToUpper()
            };
            if (string.IsNullOrEmpty(container) == false)
            {
                args.Logger?.ILog("Video Container: " + container);
                args.RecordStatisticRunningTotals("VIDEO_CONTAINER", container);
            }


            foreach (var vs in videoInfo.AudioStreams)
            {
                args.Logger.ILog($"Audio stream '{vs.Codec}' '{vs.Index}' 'Language: {vs.Language}' 'Channels: {vs.Channels}'");
            }

            SetVideoInfo(args, videoInfo, Variables);

            return 1;
        }
        catch (Exception ex)
        {
            args.Logger.ELog("Failed processing VideoFile: " + ex.Message);
            return -1;
        }
    }

    /// <summary>
    /// Prints the FFmpeg version
    /// </summary>
    /// <param name="args">the node parameters</param>
    private void PrintFFmpegVersion(NodeParameters args)
    {
        if(string.IsNullOrEmpty(FFMPEG))
            return;
        string firstLine = string.Empty;

        try
        {
            Process process = new Process();
            process.StartInfo.FileName = FFMPEG;
            process.StartInfo.Arguments = "-version";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (string.IsNullOrEmpty(output))
            {
                args.Logger.ELog("Failed detecting FFmpeg version");
                return;

            }
            // Split the output into lines
            var line = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).First();
            
            
            string pattern = @"ffmpeg\s+version\s+(.*?)(?:\s+Copyright|$)";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(line);
            var version = match.Success ? match.Groups[1].Value.Trim() : line;
            
            args.Logger.ILog("FFmpeg Version: " + version);
        }
        catch (Exception ex)
        {
            // Handle any exceptions that occurred during the process execution
            args.Logger.ELog("Failed detecting FFmpeg version: " + ex.Message);
        }

    }
}