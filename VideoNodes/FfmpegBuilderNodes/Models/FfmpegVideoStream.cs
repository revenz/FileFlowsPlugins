namespace FileFlows.VideoNodes.FfmpegBuilderNodes.Models
{
    public class FfmpegVideoStream : FfmpegStream
    {
        public VideoStream Stream { get; set; }

        private List<string> _Filter = new List<string>();
        public List<string> Filter
        {
            get => _Filter;
            set
            {
                _Filter = value ?? new List<string>();
            }
        }

        private List<string> _EncodingParameters = new List<string>();
        public List<string> EncodingParameters
        {
            get => _EncodingParameters;
            set
            {
                _EncodingParameters = value ?? new List<string>();
            }
        }
        public override bool HasChange => EncodingParameters.Any() || Filter.Any();

        public override string[] GetParameters(int outputIndex)
        {
            if (Deleted)
                return new string[] { };

            var results = new List<string> { "-map", "0:v:" + outputIndex };
            if (Filter.Any() == false  && EncodingParameters.Any() == false)
            {
                results.Add("-c:v:" + Stream.TypeIndex);
                results.Add("copy");
                return results.ToArray();
            }

            if (EncodingParameters.Any())
            {
                results.Add("-c:v:" + Stream.TypeIndex);
                results.AddRange(EncodingParameters.Select(x => x.Replace("{index}", outputIndex.ToString())));
            }
            else
            {
                // we need to set this codec since a filter will be applied, so we cant copy it.
                //results.Add("copy");
            }

            if (Filter.Any())
            {
                results.Add("-vf");
                results.Add(String.Join(", ", Filter));
            }

            return results.ToArray();
        }
    }
}
