namespace FileFlows.VideoNodes.FfmpegBuilderNodes
{
    public class FfmpegBuilderSubtitleTrackRemover : FfmpegBuilderNode
    {
        public override string Icon => "fas fa-comment";

        public override int Outputs => 2; 


        [TextVariable(1)]
        public string Pattern { get; set; }

        [Boolean(2)]
        public bool NotMatching { get; set; }

        [Boolean(3)]
        public bool UseLanguageCode { get; set; }

        public override int Execute(NodeParameters args)
        {
            this.Init(args);
            bool removing = false;
            var regex = new Regex(this.Pattern, RegexOptions.IgnoreCase);
            foreach(var stream in Model.SubtitleStreams)
            {
                string str = UseLanguageCode ? stream.Stream.Language : stream.Stream.Title;
                if (string.IsNullOrEmpty(str) == false) // if empty we always use this since we have no info to go on
                {
                    bool matches = regex.IsMatch(str);
                    if (NotMatching)
                        matches = !matches;
                    if (matches)
                    {
                        stream.Deleted = true;
                        removing = true;
                    }
                }
            }
            return removing ? 1 : 2;
        }
    }
}
