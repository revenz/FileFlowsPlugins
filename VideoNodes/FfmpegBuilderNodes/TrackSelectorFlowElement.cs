using FileFlows.VideoNodes.Helpers;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Flow element that has a track selector
/// </summary>
public abstract class TrackSelectorFlowElement : FfmpegBuilderNode
{
    /// <summary>
    /// Gets or sets replacements to replace
    /// </summary>
    [Select(nameof(CustomTrackSelectionOptions), 1)]
    [Required]
    public bool CustomTrackSelection { get; set; }
    
    private static List<ListOption> _CustomTrackSelection;
    /// <summary>
    /// Gets the codec options
    /// </summary>
    public static List<ListOption> CustomTrackSelectionOptions
    {
        get
        {
            if (_CustomTrackSelection == null)
            {
                _CustomTrackSelection = new List<ListOption>
                {
                    new () { Label = "Automatic", Value = false },
                    new () { Label = "Custom", Value = true},
                };
            }
            return _CustomTrackSelection;
        }
    }

    /// <summary>
    /// Gets or sets the track selection options
    /// </summary>
    [KeyValue(2, optionsProperty: nameof(TrackSelectionOptionsOptions), allowDuplicates: true)]
    [ConditionEquals(nameof(CustomTrackSelection), true)]
    [Required]
    public List<KeyValuePair<string, string>> TrackSelectionOptions { get; set; }

    private static List<ListOption> _TrackSelectionOptionsOptions;

    /// <summary>
    /// Gets or sets the stream type options
    /// </summary>
    public static List<ListOption> TrackSelectionOptionsOptions
    {
        get
        {
            if (_TrackSelectionOptionsOptions == null)
            {
                _TrackSelectionOptionsOptions = new List<ListOption>
                {
                    new() { Label = "Channels", Value = "Channels" },
                    new() { Label = "Codec", Value = "Codec" },
                    new() { Label = "Language", Value = "Language" },
                    new() { Label = "Title", Value = "Title" }
                };
            }

            return _TrackSelectionOptionsOptions;
        }
    }
    
    /// <summary>
    /// Gets the source audio from the parameters
    /// </summary>
    /// <returns>the source audio</returns>
    internal T? GetSourceTrack<T>() where T : VideoFileStream
    {
        if (CustomTrackSelection == false || TrackSelectionOptions?.Any() != true)
        {
            Args?.Logger?.ILog("Not using custom track selection");
            return null;
        }

        if (Model?.AudioStreams?.Any() != true)
            return null;

        Args?.Logger?.ILog("Using custom track selection");
        
        List<VideoFileStream> possible = new();
        List<VideoFileStream>? sourceStreams = typeof(T).Name switch
        {
            nameof(VideoStream) => Model.VideoStreams.Select(x => (VideoFileStream)x.Stream).Distinct().ToList(),
            nameof(AudioStream) => Model.AudioStreams.Select(x => (VideoFileStream)x.Stream).Distinct().ToList(),
            nameof(SubtitleStream) => Model.SubtitleStreams.Select(x => (VideoFileStream)x.Stream).Distinct().ToList(),
            _ => null
        };
        if (sourceStreams?.Any() != true)
        {
            Args?.Logger?.ILog("No source streams found");
            return null;
        }

        foreach (var stream in sourceStreams)
        {
            bool matches = true;
            foreach (var kv in TrackSelectionOptions)
            {
                var key = kv.Key?.ToLowerInvariant() ?? string.Empty;
                var kvValue = Args.ReplaceVariables(kv.Value?.Replace("{orig}", "{OriginalLanguage}") ?? string.Empty, stripMissing: true);
                switch (key)
                {
                    case "language":
                        if (LanguageMatches(stream, kv.Value))
                            Args?.Logger?.ILog($"Language Matches '{stream}' = {kv.Value}");
                        else
                        {
                            Args?.Logger?.ILog($"Language does not match '{stream}' = {kv.Value}");
                            matches = false;
                        }
                        break;
                    case "codec":
                        if(Args.StringHelper.Matches(kv.Value, stream.Codec))
                            Args?.Logger?.ILog($"Stream '{stream}' Codec '{stream.Codec}' matches '{kv.Value}'");
                        else
                        {
                            Args?.Logger?.ILog($"Stream '{stream}' Codec '{stream.Codec}' does not match {kv.Value}");
                            matches = false;
                        }
                        break;
                    case "title":
                        if(Args.StringHelper.Matches(kv.Value, stream.Title))
                            Args?.Logger?.ILog($"Stream '{stream}' Title '{stream.Codec}' does match '{kv.Value}'");
                        else
                        {
                            Args?.Logger?.ILog($"Stream '{stream}' Title '{stream.Codec}' does not match '{kv.Value}'");
                            matches = false;
                        }
                        break;
                    case "channels":
                        if (ChannelsMatches(stream, kv.Value))
                            Args?.Logger?.ILog($"Channels Matches '{stream}' = {kv.Value}");
                        else
                        {
                            Args?.Logger?.ILog($"Channels does not match '{stream}' = {kv.Value}");
                            matches = false;
                        }
                        break;
                }

                if (matches == false)
                    break;
            }

            if (matches == false)
            {
                Args?.Logger?.ILog($"Track '{stream}' not suitable");
                continue;
            }


            Args?.Logger?.ILog($"Track '{stream}' suitable");
            possible.Add(stream);
        }
        
        Args?.Logger?.ILog("Possible streams found: " + possible.Count);
        foreach (var stream in possible)
            Args?.Logger.ILog(" - " + stream);

        return possible.FirstOrDefault() as T;

    }

    /// <summary>
    /// Checks if the channels matches
    /// </summary>
    /// <param name="value">the value to check</param>
    /// <returns>true if channels matches, otherwise false</returns>
    private bool ChannelsMatches(VideoFileStream stream, string value)
    {
        if (stream is AudioStream audio == false)
        {
            Args?.Logger?.WLog("Not an audio stream, cannot test Channels");
            return false;
        }

        if (Args?.MathHelper.IsMathOperation(value) == true)
        {
            Args?.Logger?.DLog($"Is Math operation '{value}' comparing '{audio.Channels}'");
            return Args.MathHelper.IsTrue(value, audio.Channels);
        }

        if (double.TryParse(value, out var dblValue) == false)
        {
            Args?.Logger?.WLog($"Failed to parse '{value}' as a double");
            return false;
        }

        return Math.Abs(audio.Channels - dblValue) < 0.05f;
    }

    /// <summary>
    /// Checks if the language matches
    /// </summary>
    /// <param name="stream">the stream to check</param>
    /// <param name="value">the value to check</param>
    /// <returns>true if language matches, otherwise false</returns>
    private bool LanguageMatches(VideoFileStream stream, string value)
    {
        if (value.StartsWith('='))
            value = value[1..];
        if (stream is AudioStream audio)
            return LanguageHelper.AreSame(audio.Language, value);
        if (stream is SubtitleStream sub)
            return LanguageHelper.AreSame(sub.Language, value);
        return false;
    }
}