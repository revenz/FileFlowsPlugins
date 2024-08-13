namespace FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

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
    private List<string> _OptionalFilter = new List<string>();

    private List<string> _FilterComplex = new List<string>();
    public List<string> FilterComplex
    {
        get => _FilterComplex;
        set
        {
            _FilterComplex = value ?? new List<string>();
        }
    }
    /// <summary>
    /// Gets or sets filters that will process but only if processing is needed, these won't trigger a has changed
    /// value of the video file by themselves
    /// </summary>
    public List<string> OptionalFilter
    {
        get => _OptionalFilter;
        set
        {
            _OptionalFilter = value ?? new List<string>();
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
    // private List<string> _OptionalEncodingParameters = new List<string>();
    // /// <summary>
    // /// Gets or sets encoding paramaters that will process but only if processing is needed, these won't trigger a has changed
    // /// value of the video file by themselves
    // /// </summary>
    // public List<string> OptionalEncodingParameters
    // {
    //     get => _OptionalEncodingParameters;
    //     set
    //     {
    //         _OptionalEncodingParameters = value ?? new List<string>();
    //     }
    // }
    private List<string> _AdditionalParameters = new List<string>();
    public List<string> AdditionalParameters
    {
        get => _AdditionalParameters;
        set
        {
            _AdditionalParameters = value ?? new List<string>();
        }
    }
    public override bool HasChange => EncodingParameters.Any() || Filter.Any() || AdditionalParameters.Any();


    public override string[] GetParameters(GetParametersArgs args)
    {
        if (Deleted)
            return [];

        var results = new List<string> { "-map", "0:v:{sourceTypeIndex}" };
        if (Filter.Any() == false && EncodingParameters.Any() == false && AdditionalParameters.Any() == false)
        {
            results.Add("-c:v:{index}");
            results.Add("copy");
            if (Title == REMOVED)
            {
                results.Add($"-metadata:s:v:{args.OutputTypeIndex}");
                results.Add($"title=");
                ForcedChange = true;
            }
            return results.ToArray();
        }
        else
        {
            if (EncodingParameters.Any())
            {
                results.Add("-c:v:" + Stream.TypeIndex);
                results.AddRange(EncodingParameters.Select(x => x.Replace("{index}", args.OutputTypeIndex.ToString())));
                // if(OptionalEncodingParameters.Any())
                //     results.AddRange(OptionalEncodingParameters.Select(x => x.Replace("{index}", args.OutputTypeIndex.ToString())));
            }
            else
            {
                // we need to set this codec since a filter will be applied, so we cant copy it.
                //results.Add("copy");
            }
            if (AdditionalParameters.Any())
                results.AddRange(AdditionalParameters.Select(x => x.Replace("{index}", args.OutputTypeIndex.ToString())));

            if (Filter.Any() || OptionalFilter.Any())
            {
                results.Add("-filter:v:" + args.OutputTypeIndex);
                results.Add(string.Join(", ", Filter.Concat(OptionalFilter)).Replace("{index}", args.OutputTypeIndex.ToString()));
            }

            if (FilterComplex?.Any() == true)
            {
                results.Add("-filter_complex:v:" + args.OutputTypeIndex);
                results.Add(string.Join(", ", FilterComplex).Replace("{index}", args.OutputTypeIndex.ToString()));
            }
        }

        if (string.IsNullOrWhiteSpace(this.Title) == false)
        {
            results.Add($"-metadata:s:v:{args.OutputTypeIndex}");
            results.Add($"title={(this.Title == FfmpegStream.REMOVED ? "" : this.Title)}");
        }

        if (Metadata.Any())
        {
            results.AddRange(Metadata.Select(x => x.Replace("{index}", args.OutputTypeIndex.ToString())));
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
            Codec,
            Title,
            Deleted ? "Deleted" : null,
            HasChange ? "Changed" : null
        }.Where(x => string.IsNullOrWhiteSpace(x) == false));
    }
}
