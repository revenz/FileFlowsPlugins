namespace FileFlows.VideoNodes.FfmpegBuilderNodes.Models
{
    public class FfmpegSubtitleStream : FfmpegStream
    {
        public SubtitleStream Stream { get; set; }

        public override bool HasChange => false;

        public override string[] GetParameters(int outputIndex)
        {
            if (Deleted)
                return new string[] { };

            var results = new List<string> { "-map", "0:s:" + outputIndex, "-c:s:" + (Stream.TypeIndex - 1) };
            //if (EncodingParameters.Any() == false)
            {
                results.Add("copy");
            }

            if (Metadata.Any())
            {
                results.AddRange(Metadata.Select(x => x.Replace("{index}", outputIndex.ToString())));
            }

            return results.ToArray();
        }
    }
}
