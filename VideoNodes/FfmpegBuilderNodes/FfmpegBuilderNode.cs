using FileFlows.Plugin;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes
{
    public abstract class FfmpegBuilderNode: EncodingNode
    {
        private const string MODEL_KEY = "FFMPEG_BUILDER_MODEL";
        protected string ffmpegExe;

        public override int Inputs => 1;
        public override int Outputs => 1;
        public override string Icon => "far fa-file-video";
        public override FlowElementType Type => FlowElementType.BuildPart;

        protected void Init(NodeParameters args)
        {
            this.args = args;
            this.ffmpegExe = GetFFMpegExe(args);
            if (string.IsNullOrEmpty(ffmpegExe))
                throw new Exception("FFMPEG not found");

            if (Model == null)
                throw new Exception("FFMPEG Builder Model not set, use the \"FFMPEG Builder Start\" node to first");
        }

        public override int Execute(NodeParameters args)
        {
            return 1;
        }

        protected FfmpegModel Model
        {
            get
            {
                if (args.Variables.ContainsKey(MODEL_KEY))
                    return args.Variables[MODEL_KEY] as FfmpegModel;
                return null;
            }
            set
            {
                if (args.Variables.ContainsKey(MODEL_KEY))
                {
                    if (value == null)
                        args.Variables.Remove(MODEL_KEY);
                    else
                        args.Variables[MODEL_KEY] = value;
                }
                else if(value != null)
                    args.Variables.Add(MODEL_KEY, value);
            }
        }

        protected string[] SplitCommand(string cmd)
        {
            return Regex.Matches(cmd, @"[\""].+?[\""]|[^ ]+")
                .Cast<Match>()
                .Select(x => x.Value.Trim('"'))
                .ToArray();
        }
    }
}
