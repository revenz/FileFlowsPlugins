namespace FileFlows.VideoNodes.FfmpegBuilderNodes.Models
{
    public class FfmpegAudioStream:FfmpegStream
    {
        public AudioStream Stream { get; set; }
        public override bool HasChange => false;

        private List<string> _EncodingParameters = new List<string>();
        public List<string> EncodingParameters
        {
            get => _EncodingParameters;
            set
            {
                _EncodingParameters = value ?? new List<string>();
            }
        }
        public override string[] GetParameters(int outputIndex)
        {
            if (Deleted)
                return new string[] { };

            var results = new List<string> { "-map", "0:a:" + (Stream.TypeIndex - 1), "-c:a:" + outputIndex };
            if (EncodingParameters.Any() == false)
            {
                results.Add("copy");
                return results.ToArray();
            }
            if (EncodingParameters[0] == "-map")
                results.Clear();

            results.AddRange(EncodingParameters.Select(x => x.Replace("{index}", outputIndex.ToString())));

            return results.ToArray();
        }
    }
}
