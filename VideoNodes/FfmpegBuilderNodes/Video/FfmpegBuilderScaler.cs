namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderScaler : FfmpegBuilderNode
{
    public override string HelpUrl => "https://docs.fileflows.com/plugins/video-nodes/ffmpeg-builder/video-scaler";

    [Boolean(2)]
    public bool Force { get; set; }


    [Select(nameof(ResolutionOptions), 1)]
    public string Resolution { get; set; }



    private static List<ListOption> _ResolutionOptions;
    public static List<ListOption> ResolutionOptions
    {
        get
        {
            if (_ResolutionOptions == null)
            {
                _ResolutionOptions = new List<ListOption>
                {
                    // we use -2 here so the width is divisible by 2 and automatically scaled to
                    // the appropriate height, if we forced the height it could be stretched
                    new ListOption { Value = "640:-2", Label = "480P"},
                    new ListOption { Value = "1280:-2", Label = "720P"},
                    new ListOption { Value = "1920:-2", Label = "1080P"},
                    new ListOption { Value = "3840:-2", Label = "4K" }
                };
            }
            return _ResolutionOptions;
        }
    }
    public override int Outputs => 2;
    public override int Execute(NodeParameters args)
    {
        var videoInfo = GetVideoInfo(args);
        if (videoInfo == null || videoInfo.VideoStreams?.Any() != true)
            return -1;

        if (Force == false)
        {
            var resolution = ResolutionHelper.GetResolution(videoInfo);
            if (resolution == ResolutionHelper.Resolution.r1080p && Resolution.StartsWith("1920"))
                return 2;
            else if (resolution == ResolutionHelper.Resolution.r4k && Resolution.StartsWith("3840"))
                return 2;
            else if (resolution == ResolutionHelper.Resolution.r720p && Resolution.StartsWith("1280"))
                return 2;
            else if (resolution == ResolutionHelper.Resolution.r480p && Resolution.StartsWith("640"))
                return 2;
        }

        Model.VideoStreams[0].Filter.AddRange(new[] { $"scale={Resolution}:flags=lanczos" });

        return 1;
    }
}
