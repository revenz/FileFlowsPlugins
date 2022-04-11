namespace FileFlows.VideoNodes.FfmpegBuilderNodes
{
    public class FfmpegBuilderVideoCodec:FfmpegBuilderNode
    {
        public override int Outputs => 2;

        [DefaultValue("hevc")]
        [TextVariable(1)]
        public string VideoCodec { get; set; }

        [DefaultValue("hevc_nvenc -preset hq -crf 23")]
        [TextVariable(2)]
        public string VideoCodecParameters { get; set; }

        [Boolean(3)]
        public bool Force { get; set; }

        [Boolean(4)]
        [DefaultValue(true)]
        public bool DisableOtherVideoStreams { get; set; }

        public override int Execute(NodeParameters args)
        {
            base.Init(args);

            string codec = args.ReplaceVariables(VideoCodec ?? string.Empty);
            string parameters = args.ReplaceVariables(VideoCodecParameters ?? codec);
            
            if (string.IsNullOrWhiteSpace(parameters))
                return 1; // nothing to do

            parameters = CheckVideoCodec(ffmpegExe, parameters);

            bool encoding = false;
            foreach (var item in Model.VideoStreams.Select((x, index) => (stream: x, index)))
            {
                if(DisableOtherVideoStreams && item.index > 0)
                {
                    item.stream.Deleted = true;
                    continue;
                }
                if(Force == false)
                {
                    if (IsSameVideoCodec(item.stream.Stream.Codec, this.VideoCodec))
                        continue;
                }
                item.stream.EncodingParameters.Clear();
                item.stream.EncodingParameters.AddRange(SplitCommand(parameters));
                encoding = true;
            }
            return encoding ? 1 : 2;
        }

        protected bool IsSameVideoCodec(string current, string wanted)
        {
            wanted = ReplaceCommon(wanted);
            current = ReplaceCommon(current);

            return wanted == current;

            string ReplaceCommon(string input)
            {
                input = input.ToLower();
                input = Regex.Replace(input, "^(divx|xvid|m(-)?peg(-)4)$", "mpeg4", RegexOptions.IgnoreCase);
                input = Regex.Replace(input, "^(hevc|h[\\.x\\-]?265)$", "h265", RegexOptions.IgnoreCase);
                input = Regex.Replace(input, "^(h[\\.x\\-]?264)$", "h264", RegexOptions.IgnoreCase);
                return input;
            }
        }
    }
}
