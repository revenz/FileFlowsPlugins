﻿using FileFlows.Plugin;
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

        internal FfmpegModel GetModel() => this.Model;

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
                args.Logger?.ILog($"Track [{index}] test string: {matchString}");
                match = new Regex(pattern, RegexOptions.IgnoreCase).IsMatch(matchString);
            }

            if (notMatching)
                match = !match;
            return match;
        }
    }
}