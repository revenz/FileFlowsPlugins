namespace FileFlows.VideoNodes;

/// <summary>
/// A flow element that reads the video information for the current working file
/// </summary>
public class ReadVideoInfo: EncodingNode
{
    /// <inheritdoc />
    public override string Icon => "fas fa-video";
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/read-video-info";

    private Dictionary<string, object> _Variables;
    /// <inheritdoc />
    public override Dictionary<string, object> Variables => _Variables;


    /// <summary>
    /// Constructs and instance of the flow element
    /// </summary>
    public ReadVideoInfo()
    {
        _Variables = new Dictionary<string, object>()
        {
            { "vi.Video.Codec", "hevc" },
            { "vi.Audio.Codec", "ac3" },
            { "vi.Audio.Codecs", "ac3,aac"},
            { "vi.Audio.Language", "eng" },
            { "vi.Audio.Languages", "eng, mao" },
            { "vi.Resolution", "1080p" },
            { "vi.Duration", 1800 },
            { "vi.VideoInfo", new VideoInfo()
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
            { "vi.Width", 1920 },
            { "vi.Height", 1080 },
        };
    }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        try
        {
            var localFileResult = args.FileService.GetLocalPath(args.WorkingFile);
            if (localFileResult.Failed(out string lfError))
            {
                args.FailureReason = "Failed getting local file: " + lfError;
                args.Logger.ILog(args.FailureReason);
                return -1;
            }

            var videoInfoResult = new VideoInfoHelper(FFMPEG, args.Logger, args).Read(localFileResult.Value);
            if (videoInfoResult.Failed(out string error))
            {
                args.Logger.ELog(error);
                return 2;
            }

            var videoInfo = videoInfoResult.Value;
            if (videoInfo.VideoStreams.Any() == false)
            {
                args.Logger.ILog("No video streams detected.");
                return 2;
            }
            foreach (var vs in videoInfo.VideoStreams)
            {
                args.Logger.ILog($"Video stream '{vs.Codec}' '{vs.Index}'");
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
            args.Logger.WLog("Failed processing VideoFile: " + ex.Message);
            return 2;
        }
    }
}