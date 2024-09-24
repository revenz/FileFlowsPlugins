// using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
//
// namespace FileFlows.VideoNodes.FfmpegBuilderNodes;
//
// /// <summary>
// /// FFmpeg Builder flow element that burns in a subtitle
// /// </summary>
// public class FfmpegBuilderSubtitleBurnIn: TrackSelectorFlowElement<FfmpegBuilderSubtitleBurnIn>
// {
//     /// <inheritdoc />
//     public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/subtitle-burn-in";
//     /// <inheritdoc />
//     public override string Icon => "fas fa-fire";
//     /// <inheritdoc />
//     public override int Outputs => 2;
//
//     /// <inheritdoc />
//     public override int Execute(NodeParameters args)
//     {
//         // Select a single subtitle track to burn in
//         var subtitleTrack = Model.SubtitleStreams.FirstOrDefault(track =>
//             !track.Deleted && StreamMatches(track));
//
//         if (subtitleTrack == null)
//         {
//             args.Logger?.ILog("No matching subtitle track found to burn in.");
//             return 2; // No matching track, exit
//         }
//
//         args.Logger?.ILog($"Burning in subtitle track: {subtitleTrack}");
//
//         // Build FFmpeg command for burning in the subtitle
//         string subtitleFilter = BuildSubtitleFilter(subtitleTrack);
//         if (string.IsNullOrEmpty(subtitleFilter))
//         {
//             args.Logger?.ILog("Failed to build subtitle filter for FFmpeg.");
//             return 2; // Failed to create subtitle filter
//         }
//
//         Model.VideoStreams[0].Filter.Add(subtitleFilter);  // Add the subtitle filter to the FFmpeg filter chain
//         subtitleTrack.Deleted = true; 
//
//         return 1;
//     }
//
//     /// <summary>
//     /// Builds the FFmpeg filter string for burning in the selected subtitle track.
//     /// </summary>
//     /// <param name="subtitleTrack">The subtitle track to burn in.</param>
//     /// <returns>FFmpeg filter string for burning in the subtitle.</returns>
//     private string BuildSubtitleFilter(FfmpegSubtitleStream subtitleTrack)
//     {
//         // For different subtitle codecs, we need different filter formats
//         if (subtitleTrack.Codec.ToLowerInvariant() is "subrip" or "srt" or "ass")
//         {
//             // FFmpeg filter for SRT subtitles
//             if (int.TryParse(subtitleTrack.Stream.IndexString.Split(':')[0], out int index) == false)
//                 return null;
//             if (index < 0 || index > Model.InputFiles.Count)
//                 return null;
//             
//             return $"subtitles={Model.InputFiles[index].FileName}:{subtitleTrack.Stream.TypeIndex}";
//         }
//     
//         // Unsupported subtitle format
//         return null;
//     }
// }