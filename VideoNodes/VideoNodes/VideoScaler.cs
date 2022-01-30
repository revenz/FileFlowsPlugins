namespace FileFlows.VideoNodes
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public class VideoScaler: EncodingNode
    {
        public override string Icon => "fas fa-search-plus";
        public override int Outputs => 1; // this node always re-encodes

        [Select(nameof(CodecOptions), 1)]
        public string VideoCodec { get; set; }

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
                        new ListOption { Value = "hevc_qsv", Label = "H265 (Intel)"}
                    };
                }
                return _CodecOptions;
            }
        }


        [DefaultValue("mkv")]
        [TextVariable(3)]
        public string Extension { get; set; }

        [Select(nameof(ResolutionOptions), 2)]
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

                string ffmpegExe = GetFFMpegExe(args);
                if (string.IsNullOrEmpty(ffmpegExe))
                    return -1;


                string codec = CheckVideoCodec(ffmpegExe, VideoCodec);
                List<string> ffArgs = new List<string>()
                {
                    "-vf", $"scale={Resolution}:flags=lanczos",
                    "-c:v", "codec"
                };

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