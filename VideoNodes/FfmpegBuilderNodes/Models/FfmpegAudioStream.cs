﻿namespace FileFlows.VideoNodes.FfmpegBuilderNodes.Models
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
        public override string[] GetParameters(GetParametersArgs args)
        {
            if (Deleted)
                return new string[] { };

            var results = new List<string> { "-map", "0:a:{sourceTypeIndex}" };
            if (EncodingParameters.Any() == false && Filter.Any() == false)
            {
                results.Add("-c:a:{index}");
                results.Add("copy");
            }
            else
            {
                if (EncodingParameters.Any())
                {
                    if (EncodingParameters[0] == "-map")
                        results.Clear();
                    else
                        results.Add("-c:a:{index}");

                    results.AddRange(EncodingParameters.Select(x => x.Replace("{index}", args.OutputTypeIndex.ToString())));
                }
                else
                {
                    // we need to set this codec since a filter will be applied, so we cant copy it.
                    //results.Add("copy");
                }
                if (Filter.Any())
                {
                    if (EncodingParameters.Any() == false && Filter.Any(x => x?.Contains("loudnorm") == true) == true)
                    {
                        // normalized.  we must set the codec
                        results.AddRange(new[] { "-c:a:{index}", Stream.Codec });
                    }

                    results.Add("-filter:a:{index}");
                    results.Add(String.Join(", ", Filter));
                }
            }

            if (string.IsNullOrWhiteSpace(this.Title) == false)
            {
                results.Add("-metadata:s:a:{index}");
                results.Add($"title={(this.Title == FfmpegStream.REMOVED ? "" : this.Title)}");
            }
            if(string.IsNullOrWhiteSpace(this.Language) == false)
            {
                results.Add("-metadata:s:a:{index}");
                results.Add($"language={(this.Language == FfmpegStream.REMOVED ? "" : this.Language)}");
            }

            if (Metadata.Any())
                results.AddRange(Metadata.Select(x => x.Replace("{index}", args.OutputTypeIndex.ToString())));

            if (args.UpdateDefaultFlag)
            {
                results.AddRange(new[] { "-disposition:a:" + args.OutputTypeIndex, this.IsDefault ? "default" : "0" });
            }

            return results.ToArray();
        }
    }
}
