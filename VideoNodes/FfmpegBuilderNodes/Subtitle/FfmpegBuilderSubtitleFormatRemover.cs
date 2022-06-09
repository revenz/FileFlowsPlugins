namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderSubtitleFormatRemover : FfmpegBuilderNode
{
    public override string HelpUrl => "https://docs.fileflows.com/plugins/video-nodes/ffmpeg-builder/subtitle-format-remover";

    public override string Icon => "fas fa-comment";
    public override int Outputs => 2;

    [Boolean(1)]
    public bool RemoveAll { get; set; }

    [Checklist(nameof(Options), 2)]
    public List<string> SubtitlesToRemove { get; set; }

    private static List<ListOption> _Options;
    public static List<ListOption> Options
    {
        get
        {
            if (_Options == null)
            {
                _Options = new List<ListOption>
                {
                    new ListOption { Value = "mov_text", Label = "3GPP Timed Text subtitle"},
                    new ListOption { Value = "ssa", Label = "ASS (Advanced SubStation Alpha) subtitle (codec ass)"},
                    new ListOption { Value = "ass", Label = "ASS (Advanced SubStation Alpha) subtitle"},
                    new ListOption { Value = "xsub", Label = "DivX subtitles (XSUB)" },
                    new ListOption { Value = "dvbsub", Label = "DVB subtitles (codec dvb_subtitle)"},
                    new ListOption { Value = "dvdsub", Label = "DVD subtitles (codec dvd_subtitle)"},
                    new ListOption { Value = "dvb_teletext", Label = "DVB/Teletext Format"},
                    new ListOption { Value = "text", Label = "Raw text subtitle"},
                    new ListOption { Value = "subrip", Label = "SubRip subtitle"},
                    new ListOption { Value = "srt", Label = "SubRip subtitle (codec subrip)"},
                    new ListOption { Value = "ttml", Label = "TTML subtitle"},
                    new ListOption { Value = "webvtt", Label = "WebVTT subtitle"},
                };
            }
            return _Options;
        }
    }


    public override int Execute(NodeParameters args)
    {
        if (RemoveAll)
        {
            if (Model.SubtitleStreams.Any() == false)
                return 2;
            foreach (var stream in Model.SubtitleStreams)
                stream.Deleted = true;
            return 1;
        }


        var removeCodecs = SubtitlesToRemove?.Where(x => string.IsNullOrWhiteSpace(x) == false)?.Select(x => x.ToLower())?.ToList() ?? new List<string>();

        if (removeCodecs.Count == 0)
            return 2; // nothing to remove


        bool removing = false;
        foreach (var sub in Model.SubtitleStreams)
        {
            args.Logger?.ILog("Subtitle found: " + sub.Stream.Codec + ", " + sub.Stream.Title);
            if (removeCodecs.Contains(sub.Stream.Codec.ToLower()))
            {
                sub.Deleted = true;
                removing = true;
                continue;
            }
        }

        return removing ? 1 : 2;
    }
}
