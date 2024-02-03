using FileFlows.Plugin;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes
{
    public abstract class FfmpegBuilderNode: EncodingNode
    {
        internal const string MODEL_KEY = "FfmpegBuilderModel";

        /// <summary>
        /// Gets the number of inputs
        /// </summary>
        public override int Inputs => 1;
        /// <summary>
        /// Gets the number of outputs
        /// </summary>
        public override int Outputs => 1;
        /// <summary>
        /// Gets the icon
        /// </summary>
        public override string Icon => "far fa-file-video";
        /// <summary>
        /// Gets the flow element type
        /// </summary>
        public override FlowElementType Type => FlowElementType.BuildPart;
        /// <summary>
        /// Gets the help URL
        /// </summary>
        public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder";


        /// <summary>
        /// Runs any code that needs to run before the execution code
        /// E.g. loads any variables that the Execute will use
        /// </summary>
        /// <param name="args">the node parameters</param>
        /// <returns>true if successful, otherwise false and will fail the flow</returns>
        /// <exception cref="Exception">throw if the video is not initialized</exception>
        public override bool PreExecute(NodeParameters args)
        {
            if (base.PreExecute(args) == false)
                return false;
            
            if(this is FfmpegBuilderStart == false && Model == null)
                throw new Exception("FFMPEG Builder Model not set, you must add and use the \"FFMPEG Builder Start\" node first");

            if (this is FfmpegBuilderStart == false)
            {
                if (Model.VideoInfo == null)
                    throw new Exception("FFMPEG Builder VideoInfo is null");
                string header = "------------------------ Starting FFmpeg Builder Model ------------------------";
                args.Logger?.ILog(header);
                foreach (var stream in Model.VideoStreams ?? new List<FfmpegVideoStream>())
                {
                    string line = "| Video Stream: " + stream;
                    args.Logger?.ILog(line + new string(' ', Math.Max(1, header.Length - line.Length - 1)) + '|');
                }
                foreach (var stream in Model.AudioStreams ?? new List<FfmpegAudioStream>())
                {
                    string line = "| Audio Stream: " + stream;
                    args.Logger?.ILog(line + new string(' ', Math.Max(1, header.Length - line.Length - 1)) + '|');
                }
                foreach (var stream in Model.SubtitleStreams ?? new List<FfmpegSubtitleStream>())
                {
                    string line = "| Subtitle Stream: " + stream;
                    args.Logger?.ILog(line + new string(' ', Math.Max(1, header.Length - line.Length - 1)) + '|');
                }

                args.Logger?.ILog(new string('-', header.Length));
            }

            return true;
        }

        public override int Execute(NodeParameters args)
        {
            return 1;
        }

        internal FfmpegModel GetModel() => this.Model;

        protected FfmpegModel Model
        {
            get
            {
                if (Args.Variables.TryGetValue(MODEL_KEY, out var variable))
                    return variable as FfmpegModel;
                return null;
            }
            set
            {
                if (Args.Variables.ContainsKey(MODEL_KEY))
                {
                    if (value == null)
                        Args.Variables.Remove(MODEL_KEY);
                    else
                        Args.Variables[MODEL_KEY] = value;
                }
                else if(value != null)
                    Args.Variables.Add(MODEL_KEY, value);
            }
        }

        protected string[] SplitCommand(string cmd)
        {
            return Regex.Matches(cmd, @"[\""].+?[\""]|[^ ]+")
                .Cast<Match>()
                .Select(x => x.Value.Trim('"'))
                .ToArray();
        }


        protected bool PatternMatches2(string pattern, int index, FfmpegStream stream, bool notMatching = false)
        {
            bool match;
            var matchLessThan = Regex.Match(pattern, @"^[\s]*<[\s]*([\d]+)[\s]*$");
            var matchLessThanEqual = Regex.Match(pattern, @"^[\s]*<=[\s]*([\d]+)[\s]*$");
            var matchGreaterThan = Regex.Match(pattern, @"^[\s]*>[\s]*([\d]+)[\s]*$");
            var matchGreaterThanEqual = Regex.Match(pattern, @"^[\s]*>=[\s]*([\d]+)[\s]*$");

            if (matchLessThan.Success)
            {
                int lessThanIndex = int.Parse(matchLessThan.Groups[1].Value);
                match = index < lessThanIndex;
            }
            else if (matchLessThanEqual.Success)
            {
                int lessThanIndex = int.Parse(matchLessThanEqual.Groups[1].Value);
                match = index <= lessThanIndex;
            }
            else if (matchGreaterThan.Success)
            {
                int greaterThanIndex = int.Parse(matchGreaterThan.Groups[1].Value);
                match = index > greaterThanIndex;
            }
            else if (matchGreaterThanEqual.Success)
            {
                int greaterThanIndex = int.Parse(matchGreaterThanEqual.Groups[1].Value);
                match = index >= greaterThanIndex;
            }
            else
            {
                string matchString;
                if (stream is FfmpegAudioStream audio)
                    matchString = audio.Stream.Title + ":" + audio.Stream.Language + ":" + audio.Stream.Codec;
                else if (stream is FfmpegVideoStream video)
                    matchString = video.Stream.Title + ":" + video.Stream.Codec;
                else if (stream is FfmpegSubtitleStream subtitle)
                    matchString = subtitle.Stream.Title + ":" + subtitle.Stream.Language + ":" + subtitle.Stream.Codec;
                else
                    return false;
                Args.Logger?.ILog($"Track [{index}] test string: {matchString}");
                match = new Regex(pattern, RegexOptions.IgnoreCase).IsMatch(matchString);
            }

            if (notMatching)
                match = !match;
            return match;
        }
    }
}
