using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// FFmpeg Builder flow element that burns in a subtitle
/// </summary>
public class FfmpegBuilderSubtitleBurnIn: TrackSelectorFlowElement<FfmpegBuilderSubtitleBurnIn>
{
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/subtitle-burn-in";
    /// <inheritdoc />
    public override string Icon => "fas fa-fire";
    /// <inheritdoc />
    public override int Outputs => 2;

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        // Select a single subtitle track to burn in
        var subtitleTrack = Model.SubtitleStreams.FirstOrDefault(track =>
            StreamMatches(track));

        if (subtitleTrack == null)
        {
            args.Logger?.ILog("No matching subtitle track found to burn in.");
            return 2; // No matching track, exit
        }

        args.Logger?.ILog($"Attempting to burn in subtitle track: {subtitleTrack}");

        // Build FFmpeg command for burning in the subtitle
        string subtitleFilter = BuildSubtitleFilter(subtitleTrack);
        if (string.IsNullOrEmpty(subtitleFilter))
        {
            args.Logger?.ILog("Failed to build subtitle filter for FFmpeg.");
            return 2; // Failed to create subtitle filter
        }

        Model.VideoStreams[0].Filter.Add(subtitleFilter); // Add subtitle filter to FFmpeg filter chain
        subtitleTrack.Deleted = true; // Mark subtitle as burned in after successful filter creation

        return 1;
    }

    /// <summary>
    /// Builds the FFmpeg filter string for burning in the selected subtitle track.
    /// </summary>
    /// <param name="subtitleTrack">The subtitle track to burn in.</param>
    /// <returns>FFmpeg filter string for burning in the subtitle.</returns>
    private string BuildSubtitleFilter(FfmpegSubtitleStream subtitleTrack)
    {
        // Check if the subtitle codec is supported
        if (subtitleTrack.Codec.ToLowerInvariant() is not ("subrip" or "srt" or "ass"))
        {
            Args.Logger?.ILog($"Unsupported subtitle codec '{subtitleTrack.Codec}' for burning in.");
            return null;
        }

        // Extract the index and type index for the subtitle stream to use in the filter
        string inputFile = Model.InputFiles.FirstOrDefault()?.FileName; // Assumes a single input file
        if (string.IsNullOrEmpty(inputFile))
        {
            Args.Logger?.ILog("No input file found to build subtitle filter.");
            return null;
        }

        return $"subtitles='{inputFile}':si={subtitleTrack.Stream.TypeIndex}";
    }

}