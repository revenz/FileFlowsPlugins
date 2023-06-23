namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderScaler : FfmpegBuilderNode
{
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/video-scaler";


    [Select(nameof(ResolutionOptions), 1)]
    public string Resolution { get; set; }
    public override int Outputs => 2;

    [Boolean(2)]
    public bool Force { get; set; }

    [ConditionEquals(nameof(Force), true, inverse: true)]
    [Boolean(3)]
    public bool OnlyIfLarger { get; set; }


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
                    new ListOption { Value = "2560:-2", Label = "1440P"},
                    new ListOption { Value = "3840:-2", Label = "4K" }
                };
            }
            return _ResolutionOptions;
        }
    }
    public override int Execute(NodeParameters args)
    {
        var videoInfo = GetVideoInfo(args);
        if (videoInfo == null || videoInfo.VideoStreams?.Any() != true)
            return -1;

        bool scale1920 = Resolution.StartsWith("1920");
        bool scale2560 = Resolution.StartsWith("2560");
        bool scale4k= Resolution.StartsWith("3840");
        bool scale720 = Resolution.StartsWith("1280");
        bool scale480 = Resolution.StartsWith("640");
        int width = videoInfo.VideoStreams[0].Width;

        if (Force == false)
        {
            var resolution = ResolutionHelper.GetResolution(videoInfo);

            if (OnlyIfLarger)
            {
                if (scale4k && width > 3840)
                    return Scale();
                if (scale1920 && width > 1920)
                    return Scale();
                if (scale2560 && width > 2560)
                    return Scale();
                if (scale720 && width > 1280)
                    return Scale();
                if (scale480 && width > 640)
                    return Scale();
                return 2;
            }
            
            if (resolution == ResolutionHelper.Resolution.r1080p && scale1920)
                return 2;
            else if (resolution == ResolutionHelper.Resolution.r1440p && scale2560)
                return 2;
            else if (resolution == ResolutionHelper.Resolution.r4k && scale4k)
                return 2;
            else if (resolution == ResolutionHelper.Resolution.r720p && scale720)
                return 2;
            else if (resolution == ResolutionHelper.Resolution.r480p && scale480)
                return 2;
        }
        return Scale();

        int Scale()
        {
            Model.VideoStreams[0].Filter.AddRange(new[] { $"scale={Resolution}:flags=lanczos" });
            return 1;
        }
    }
}
