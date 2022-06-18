namespace FileFlows.VideoNodes.FfmpegBuilderNodes.Models
{
    public class FfmpegAudioStream:FfmpegStream
    {
        public AudioStream Stream { get; set; }
        public override bool HasChange => EncodingParameters.Any() || Filter.Any();

        private List<string> _EncodingParameters = new List<string>();
        public List<string> EncodingParameters
        {
            get => _EncodingParameters;
            set
            {
                _EncodingParameters = value ?? new List<string>();
            }
        }
        private List<string> _Filter = new List<string>();
        public List<string> Filter
        {
            get => _Filter;
            set
            {
                _Filter = value ?? new List<string>();
            }
        }
        public override string[] GetParameters(int outputIndex)
        {
            if (Deleted)
                return new string[] { };

            var results = new List<string> { "-map", "0:a:" + Stream.TypeIndex };
            if (EncodingParameters.Any() == false && Filter.Any() == false)
            {
                results.Add("-c:a:" + outputIndex);
                results.Add("copy");
            }
            else
            {
                if (EncodingParameters.Any())
                {
                    if (EncodingParameters[0] == "-map")
                        results.Clear();
                    else
                        results.Add("-c:a:" + outputIndex);

                    results.AddRange(EncodingParameters.Select(x => x.Replace("{index}", outputIndex.ToString())));
                }
                else
                {
                    // we need to set this codec since a filter will be applied, so we cant copy it.
                    //results.Add("copy");
                }
                if (Filter.Any())
                {
                    results.Add("-filter:a:" + outputIndex);
                    results.Add(String.Join(", ", Filter));
                }
            }

            if (string.IsNullOrWhiteSpace(this.Title) == false)
            {
                results.Add($"-metadata:s:a:{outputIndex}");
                results.Add($"title={(this.Title == FfmpegStream.REMOVED ? "" : this.Title)}");
            }
            if(string.IsNullOrWhiteSpace(this.Language) == false)
            {
                results.Add($"-metadata:s:a:{outputIndex}");
                results.Add($"language={(this.Language == FfmpegStream.REMOVED ? "" : this.Language)}");
            }

            if (Metadata.Any())
                results.AddRange(Metadata.Select(x => x.Replace("{index}", outputIndex.ToString())));

            return results.ToArray();
        }
    }
}
