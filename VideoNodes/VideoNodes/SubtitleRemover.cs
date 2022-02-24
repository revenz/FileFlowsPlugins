namespace FileFlows.VideoNodes
{
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class SubtitleRemover: EncodingNode
    {
        public override int Outputs => 2;

        public override string Icon => "fas fa-comment";


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
            try
            {
                VideoInfo videoInfo = GetVideoInfo(args);
                if (videoInfo == null)
                    return -1;

                string ffmpegExe = GetFFMpegExe(args);
                if (string.IsNullOrEmpty(ffmpegExe))
                    return -1;

                List<string> ffArgs = new List<string>()
                {
                    "-map", "0:v",
                    "-map", "0:a",
                };

                bool foundBadSubtitle = false;

                if (RemoveAll == false)
                {

                    var removeCodecs = SubtitlesToRemove?.Where(x => string.IsNullOrWhiteSpace(x) == false)?.Select(x => x.ToLower())?.ToList() ?? new List<string>();

                    if (removeCodecs.Count == 0)
                        return 2; // nothing to remove


                    foreach (var sub in videoInfo.SubtitleStreams)
                    {
                        args.Logger?.ILog("Subtitle found: " + sub.Codec + ", " + sub.Title);
                        if (removeCodecs.Contains(sub.Codec.ToLower()))
                        {
                            foundBadSubtitle = true;
                            continue;
                        }
                        ffArgs.AddRange(new[] { "-map", sub.IndexString });
                    }
                }
                else
                {
                    foundBadSubtitle = videoInfo.SubtitleStreams?.Any() == true;
                }

                if(foundBadSubtitle == false)
                {
                    // nothing to remove
                    return 2;
                }
                ffArgs.AddRange(new[] { "-c", "copy" });


                string extension = new FileInfo(args.WorkingFile).Extension;
                if(extension.StartsWith("."))
                    extension = extension.Substring(1); 

                if (Encode(args, ffmpegExe, ffArgs, extension) == false)
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
