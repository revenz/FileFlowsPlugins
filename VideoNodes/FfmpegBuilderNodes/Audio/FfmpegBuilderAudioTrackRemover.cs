namespace FileFlows.VideoNodes.FfmpegBuilderNodes
{
    public class FfmpegBuilderAudioTrackRemover: FfmpegBuilderNode
    {
        public override string Icon => "fas fa-volume-off";

        public override int Outputs => 2; 

        [Boolean(1)]
        public bool RemoveAll { get; set; }


        [TextVariable(2)]
        [ConditionEquals(nameof(RemoveAll), false)]
        public string Pattern { get; set; }

        [Boolean(3)]
        [ConditionEquals(nameof(RemoveAll), false)]
        public bool NotMatching { get; set; }

        [Boolean(4)]
        [ConditionEquals(nameof(RemoveAll), false)]
        public bool UseLanguageCode { get; set; }

        public override int Execute(NodeParameters args)
        {
            this.Init(args);
            bool removing = false;
            Regex? regex = null;
            foreach(var audio in Model.AudioStreams)
            {
                if (RemoveAll)
                {
                    audio.Deleted = true;
                    removing = true;
                    continue;
                }
                if(regex == null)
                    regex = new Regex(this.Pattern, RegexOptions.IgnoreCase);
                string str = UseLanguageCode ? audio.Stream.Language : audio.Stream.Title;
                if (string.IsNullOrEmpty(str) == false) // if empty we always use this since we have no info to go on
                {
                    bool matches = regex.IsMatch(str);
                    if (NotMatching)
                        matches = !matches;
                    if (matches)
                    {
                        audio.Deleted = true;
                        removing = true;
                    }
                }
            }
            return removing ? 1 : 2;
        }
    }
}
