namespace FileFlows.VideoNodes.FfmpegBuilderNodes.Models
{
    public class FfmpegAudioStream:FfmpegStream
    {
        public AudioStream Stream { get; set; }
        public override bool HasChange => EncodingParameters.Any() || Filter.Any();
        
        /// <summary>
        /// Gets or sets the channels for this stream
        /// Note: changing this will not magically change the channels for processing, you must change manually
        /// down-mix or up-mix then update this channel count, this is intended for sorting only
        /// </summary>
        public float Channels { get; set; }

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

        /// <summary>
        /// Converts the object to a string
        /// </summary>
        /// <returns>the string representation of stream</returns>
        public override string ToString()
        {
            // if (Stream != null)
            //     return Stream.ToString() + (Deleted ? " / Deleted" : "");
            // can be null in unit tests
            return string.Join(" / ", new string[]
            {
                Index.ToString(),
                Language,
                Codec,
                Title,
                Channels > 0 ? Channels.ToString("0.0") : null,
                IsDefault ? "Default" : null,
                Deleted ? "Deleted" : null,
                HasChange ? "Changed" : null
            }.Where(x => string.IsNullOrWhiteSpace(x) == false));
        }
    }
}
