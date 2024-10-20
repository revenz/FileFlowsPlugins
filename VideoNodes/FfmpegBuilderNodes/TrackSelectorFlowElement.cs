using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using FileFlows.VideoNodes.Helpers;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Flow element that has a track selector
/// </summary>
public abstract class TrackSelectorFlowElement<T> : FfmpegBuilderNode where T : TrackSelectorFlowElement<T>
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
                var instance = (T)Activator.CreateInstance(typeof(T))!;
                
                _CustomTrackSelection = new List<ListOption>
                {
                    new () { Label = instance.AutomaticLabel, Value = false },
                    new () { Label = instance.CustomLabel, Value = true},
                };
            }
            return _CustomTrackSelection;
        }
    }

    /// <summary>
    /// Gets the label for the automatic selection
    /// </summary>
    protected virtual string AutomaticLabel => "Automatic";
    /// <summary>
    /// Gets the label for the custom selection
    /// </summary>
    protected virtual string CustomLabel => "Custom";

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
            if (StreamMatches(stream) == false)
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
    /// Tests if a stream matches the specified conditions
    /// </summary>
    /// <param name="stream">the stream to check</param>
    /// <returns>true if matches, otherwise false</returns>
    protected bool StreamMatches(IVideoStream stream)
    {
        foreach (var kv in TrackSelectionOptions)
        {
            var key = kv.Key?.ToLowerInvariant() ?? string.Empty;
            var kvValue = Args.ReplaceVariables(kv.Value?.Replace("{orig}", "{OriginalLanguage}") ?? string.Empty,
                stripMissing: true);
            switch (key)
            {
                case "language":
                    if (LanguageMatches(stream, kvValue))
                        Args?.Logger?.ILog($"Language Matches '{stream}' = {kvValue}");
                    else
                    {
                        Args?.Logger?.ILog($"Language does not match '{stream}' = {kvValue}");
                        return false;
                    }

                    break;
                case "codec":
                    if (Args.StringHelper.Matches(kvValue, stream.Codec))
                        Args?.Logger?.ILog($"Stream '{stream}' Codec '{stream.Codec}' matches '{kvValue}'");
                    else
                    {
                        Args?.Logger?.ILog($"Stream '{stream}' Codec '{stream.Codec}' does not match {kvValue}");
                        return false;
                    }

                    break;
                case "title":
                    if (Args.StringHelper.Matches(kvValue, stream.Title))
                        Args?.Logger?.ILog($"Stream '{stream}' Title '{stream.Codec}' does match '{kvValue}'");
                    else
                    {
                        Args?.Logger?.ILog($"Stream '{stream}' Title '{stream.Codec}' does not match '{kvValue}'");
                        return false;
                    }

                    break;
                case "channels":
                    if (ChannelsMatches(stream, kvValue))
                        Args?.Logger?.ILog($"Channels Matches '{stream}' = {kvValue}");
                    else
                    {
                        Args?.Logger?.ILog($"Channels does not match '{stream}' = {kvValue}");
                        return false;
                    }

                    break;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks if the channels matches
    /// </summary>
    /// <param name="value">the value to check</param>
    /// <returns>true if channels matches, otherwise false</returns>
    private bool ChannelsMatches(IVideoStream stream, string value)
    {
        float? channels = null;
        if (stream is AudioStream audio)
            channels = audio.Channels;
        else if(stream is FfmpegAudioStream ffAudio)
            channels = ffAudio.Channels;
        else
        {
            Args?.Logger?.WLog("Not an audio stream, cannot test Channels");
            return false;
        }
        if (Args?.MathHelper.IsMathOperation(value) == true)
        {
            Args?.Logger?.DLog($"Is Math operation '{value}' comparing '{channels}'");
            return Args.MathHelper.IsTrue(value, channels.Value);
        }

        if (double.TryParse(value, out var dblValue) == false)
        {
            Args?.Logger?.WLog($"Failed to parse '{value}' as a double");
            return false;
        }

        return Math.Abs(channels.Value - dblValue) < 0.05f;
    }

    /// <summary>
    /// Checks if the language matches
    /// </summary>
    /// <param name="stream">the stream to check</param>
    /// <param name="value">the value to check</param>
    /// <returns>true if language matches, otherwise false</returns>
    private bool LanguageMatches(IVideoStream stream, string value)
    {
        if (value.StartsWith('='))
            value = value[1..];
        return LanguageHelper.AreSame(stream.Language, value);
    }
}