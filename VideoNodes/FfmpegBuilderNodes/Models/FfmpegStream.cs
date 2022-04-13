namespace FileFlows.VideoNodes.FfmpegBuilderNodes.Models
{
    public abstract class FfmpegStream
    {
        public bool Deleted { get; set; }
        public int Index { get; set; }
        public string Title { get; set; }
        public string Language { get; set; }

        public abstract bool HasChange { get; }
        public bool ForcedChange { get; set; }

        public abstract string[] GetParameters(int outputIndex);

        private List<string> _Metadata = new List<string>();
        public List<string> Metadata
        {
            get => _Metadata;
            set
            {
                _Metadata = value ?? new List<string>();
            }
        }
    }
}
