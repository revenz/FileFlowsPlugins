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

            var results = new List<string> { "-map", "0:s:" + outputIndex, "-c:s:" + Stream.TypeIndex };
            //if (EncodingParameters.Any() == false)
            {
                results.Add("copy");
            }

            if (string.IsNullOrWhiteSpace(this.Title) == false)
            {
                results.Add($"-metadata:s:s:{outputIndex}");
                results.Add($"title={(this.Title == FfmpegStream.REMOVED ? "" : this.Title)}");
            }
            if (string.IsNullOrWhiteSpace(this.Language) == false)
            {
                results.Add($"-metadata:s:s:{outputIndex}");
                results.Add($"language={(this.Language == FfmpegStream.REMOVED ? "" : this.Language)}");
            }

            if (Metadata.Any())
            {
                results.AddRange(Metadata.Select(x => x.Replace("{index}", outputIndex.ToString())));
            }

            return results.ToArray();
        }
    }
}
