namespace FileFlows.VideoNodes
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public class VideoScaler: EncodingNode
    {
        public override string Icon => "fas fa-search-plus";
        public override int Outputs => 2; // this node always re-encodes

        [Select(nameof(CodecOptions), 1)]
        public string VideoCodec { get; set; }

        [Required]
        [TextVariable(2)]        
        [ConditionEquals(nameof(VideoCodec), "Custom")]
        public string VideoCodecParameters { get; set; }

        private static List<ListOption> _CodecOptions;
        public static List<ListOption> CodecOptions
        {
            get
            {
                if (_CodecOptions == null)
                {
                    _CodecOptions = new List<ListOption>
                    {
                        new ListOption { Label = "Automatic", Value = "###GROUP###"},
                        new ListOption { Value = "h264", Label = "H264"},
                        new ListOption { Value = "h265", Label = "H265"},

                        new ListOption { Label = "CPU Encoding", Value = "###GROUP###"},
                        new ListOption { Value = "libx264", Label = "H264 (CPU)"},
                        new ListOption { Value = "libx265", Label = "H265 (CPU)"},

                        new ListOption { Label = "NVIDIA Hardware Encoding", Value = "###GROUP###"},
                        new ListOption { Value = "h264_nvenc", Label = "H264 (NVIDIA)"},
                        new ListOption { Value = "hevc_nvenc -preset hq -crf 23", Label = "H265 (NVIDIA)"},

                        new ListOption { Label = "Intel Hardware Encoding", Value = "###GROUP###"},
                        new ListOption { Value = "h264_qsv", Label = "H264 (Intel)"},
                        new ListOption { Value = "hevc_qsv", Label = "H265 (Intel)"},

                        new ListOption { Label = "Custom", Value = "###GROUP###"},
                        new ListOption { Value = "Custom", Label = "Custom"},
                    };
                }
                return _CodecOptions;
            }
        }


        [DefaultValue("mkv")]
        [TextVariable(4)]
        public string Extension { get; set; }

        [Boolean(5)]
        public bool Force { get; set; }


        [Select(nameof(ResolutionOptions), 3)]
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

        public override int Execute(NodeParameters args)
        {
            this.args = args;
            Extension = args.ReplaceVariables(Extension)?.EmptyAsNull() ?? "mkv";

            try
            {
                VideoInfo videoInfo = GetVideoInfo(args);
                if (videoInfo == null)
                    return -1;


                if (Force == false)
                {
                    var resolution = ResolutionHelper.GetResolution(videoInfo);
                    if(resolution == ResolutionHelper.Resolution.r1080p && Resolution.StartsWith("1920"))
                        return 2;
                    else if (resolution == ResolutionHelper.Resolution.r4k && Resolution.StartsWith("3840"))
                        return 2;
                    else if (resolution == ResolutionHelper.Resolution.r720p && Resolution.StartsWith("1280"))
                        return 2;
                    else if (resolution == ResolutionHelper.Resolution.r480p && Resolution.StartsWith("640"))
                        return 2;
                }


                string ffmpegExe = GetFFMpegExe(args);
                if (string.IsNullOrEmpty(ffmpegExe))
                    return -1;

                List<string> ffArgs = new List<string>()
                {
                    "-vf", $"scale={Resolution}:flags=lanczos",
                    "-c:v"
                };

                string codec = VideoCodec == "Custom" && string.IsNullOrWhiteSpace(VideoCodecParameters) == false ?
                               VideoCodecParameters : CheckVideoCodec(ffmpegExe, VideoCodec);

                foreach (string c in codec.Split(" "))
                {
                    if (string.IsNullOrWhiteSpace(c.Trim()))
                        continue;
                    ffArgs.Add(c.Trim());
                }

                if (Encode(args, ffmpegExe, ffArgs, Extension) == false)
                    return -1;

                return 1;
            }
            catch (Exception ex)
            {
                args.Logger?.ELog("Failed processing VideoFile: " + ex.Message + Environment.NewLine + ex.StackTrace);
                return -1;
            }
        }
    }
}