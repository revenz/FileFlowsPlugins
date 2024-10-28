using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Flow element used to remove matching tracks
/// </summary>
public class FfmpegBuilderTrackRemover:  TrackSelectorFlowElement<FfmpegBuilderTrackRemover>
{
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/track-remover";

    /// <inheritdoc />
    public override string Icon => "fas fa-eraser";

    /// <inheritdoc />
    public override int Outputs => 2;

    /// <inheritdoc />
    protected override string AutomaticLabel => "All";
    /// <inheritdoc />
    protected override string CustomLabel => "Matching";
    /// <inheritdoc />
    protected override bool AllowIndex => true;

    /// <summary>
    /// Gets or sets the stream types to remove
    /// </summary>
    [Select(nameof(StreamTypeOptions), 1)]
    public string StreamType { get; set; }

    /// <summary>
    /// The static stream type list
    /// </summary>
    private static List<ListOption> _StreamTypeOptions;
    
    /// <summary>
    /// Gets the available stream types
    /// </summary>
    public static List<ListOption> StreamTypeOptions
    {
        get
        {
            if (_StreamTypeOptions == null)
            {
                _StreamTypeOptions = new List<ListOption>
                {
                    new () { Label = "Video", Value = "Video" },
                    new () { Label = "Audio", Value = "Audio" },
                    new () { Label = "Subtitle", Value = "Subtitle" }
                };
            }
            return _StreamTypeOptions;
        }
    }
    
    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        if(string.IsNullOrEmpty(StreamType) || StreamType.Equals("audio", StringComparison.CurrentCultureIgnoreCase))
            return RemoveTracks(Model.AudioStreams) ? 1 : 2;
        if (StreamType.Equals("subtitle", StringComparison.CurrentCultureIgnoreCase))
            return RemoveTracks(Model.SubtitleStreams) ? 1 : 2;
        if (StreamType.Equals("video", StringComparison.CurrentCultureIgnoreCase))
            return RemoveTracks(Model.VideoStreams) ? 1 : 2;

        return 2;
    }

    /// <summary>
    /// Iteerate the tracks/streams and remove any that match the conditions
    /// </summary>
    /// <param name="tracks">the track to iterate</param>
    /// <typeparam name="T">The type of the track</typeparam>
    /// <returns>true if any tracks were removed/deleted</returns>
    private bool RemoveTracks<T>(List<T> tracks) where T: FfmpegStream
    {
        bool removing = false;
        int index = -1;
        foreach (var track in tracks)
        {
            if (track.Deleted)
                continue;
            index++;

            if (CustomTrackSelection && StreamMatches(track, index) == false)
            {
                Args.Logger?.ILog("Stream does not match conditions: " + track);
                continue;
            }
            Args.Logger?.ILog($"Deleting Stream: {track}");
            track.Deleted = true;
            removing = true;
        }

        return removing;
    }
}