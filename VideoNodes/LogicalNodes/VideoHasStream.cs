namespace FileFlows.VideoNodes
{
    using System.Linq;
    using System.ComponentModel;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;
    using System.ComponentModel.DataAnnotations;

    public class VideoHasStream : VideoNode
    {
        public override int Inputs => 1;
        public override int Outputs => 2;
        public override FlowElementType Type => FlowElementType.Logic;

        [Select(nameof(StreamTypeOptions), 1)]
        public string Stream { get; set; }

        private static List<ListOption> _StreamTypeOptions;
        public static List<ListOption> StreamTypeOptions
        {
            get
            {
                if (_StreamTypeOptions == null)
                {
                    _StreamTypeOptions = new List<ListOption>
                    {
                        new ListOption { Label = "Video", Value = "Video" },
                        new ListOption { Label = "Audio", Value = "Audio" },
                        new ListOption { Label = "Subtitle", Value = "Subtitle" }
                    };
                }
                return _StreamTypeOptions;
            }
        }

        [TextVariable(2)]
        public string Title { get; set; }
        
        [TextVariable(3)]
        public string Codec { get; set; }
        
        [ConditionEquals(nameof(Stream), "Video", inverse: true)]
        [TextVariable(4)]
        public string Language { get; set; }
        
        [ConditionEquals(nameof(Stream), "Audio")]
        [NumberInt(5)]
        public float Channels { get; set; }

        public override int Execute(NodeParameters args)
        {
            var videoInfo = GetVideoInfo(args);
            if (videoInfo == null)
                return -1;

            bool found = false;
            if (this.Stream == "Video")
            {
                found = videoInfo.VideoStreams.Where(x =>
                {
                    if (TitleMatches(x.Title))
                        return false;
                    if (CodecMatches(x.Codec))
                        return false;
                    return true;
                }).Any();
            }
            else if (this.Stream == "Audio")
            {
                found = videoInfo.AudioStreams.Where(x =>
                {
                    if (TitleMatches(x.Title))
                        return false;
                    if (CodecMatches(x.Codec))
                        return false;
                    if (LanguageMatches(x.Codec))
                        return false;
                    if (this.Channels > 0 && Math.Abs(x.Channels - this.Channels) > 0.05f)
                        return false;
                    return true;
                }).Any();
            }
            else if (this.Stream == "Subtitle")
            {   
                found = videoInfo.SubtitleStreams.Where(x =>
                {
                    if (TitleMatches(x.Title))
                        return false;
                    if (CodecMatches(x.Codec))
                        return false;
                    if (LanguageMatches(x.Codec))
                        return false;
                    return true;
                }).Any();
            }

            return found ? 1 : 2;
        }

        private bool TitleMatches(string value) => ValueMatch(this.Title, value);
        private bool CodecMatches(string value) => ValueMatch(this.Codec, value);
        private bool LanguageMatches(string value) => ValueMatch(this.Language, value);
        private bool ValueMatch(string pattern, string value)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                return true; // no test, it matches
            try
            {
                if (string.IsNullOrEmpty(value))
                    return false;
                var rgx = new Regex(pattern, RegexOptions.IgnoreCase);
                return rgx.IsMatch(value);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}