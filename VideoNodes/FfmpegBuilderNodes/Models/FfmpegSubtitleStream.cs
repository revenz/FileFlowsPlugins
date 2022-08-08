namespace FileFlows.VideoNodes.FfmpegBuilderNodes.Models
{
    public class FfmpegSubtitleStream : FfmpegStream
    {
        public SubtitleStream Stream { get; set; }

        public override bool HasChange => false;

        public override string[] GetParameters(GetParametersArgs args)
        {
            if (Deleted)
                return new string[] { };

            List<string> results= new List<string> { "-map", Stream.InputFileIndex + ":s:{sourceTypeIndex}", "-c:s:{index}" };

            switch (args.DestinationExtension)
            {
                case "mkv":
                    {
                        if(Stream.Codec == "mov_text")
                            results.Add("srt");
                        else
                            results.Add("copy");
                    }
                    break;
                case "mp4":
                    {
                        if (Helpers.SubtitleHelper.IsImageSubtitle(Stream.Codec))
                        {
                            results.Add("copy");
                        }
                        else
                        {
                            results.Add("mov_text");
                        }
                    }
                    break;
                default:
                    {
                        results.Add("copy");
                    }
                    break;
            }
           

            if (string.IsNullOrWhiteSpace(this.Title) == false)
            {
                // first s: means stream speicific, this is suppose to have :s:s
                // https://stackoverflow.com/a/21059838
                results.Add($"-metadata:s:s:{args.OutputTypeIndex}");
                results.Add($"title={(this.Title == FfmpegStream.REMOVED ? "" : this.Title)}");
            }
            if (string.IsNullOrWhiteSpace(this.Language) == false)
            {
                results.Add($"-metadata:s:s:{args.OutputTypeIndex}");
                results.Add($"language={(this.Language == FfmpegStream.REMOVED ? "" : this.Language)}");
            }

            if (Metadata.Any())
            {
                results.AddRange(Metadata.Select(x => x.Replace("{index}", args.OutputTypeIndex.ToString())));
            }

            return results.ToArray();
        }
    }
}
