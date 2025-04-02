using FileFlows.VideoNodes.FfmpegBuilderNodes;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes;

/// <summary>
/// Flow element that tests if a video file has an audio file
/// </summary>
public class VideoHasAudio : VideoNode
{
    /// <inheritdoc />
    public override int Inputs => 1;

    /// <inheritdoc />
    public override int Outputs => 2;

    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;

    /// <inheritdoc />
    public override string Icon => "fas fa-question";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/logical-nodes/video-has-audio";
    
    /// <summary>
    /// Gets or sets if the FFmpeg model should be checked
    /// </summary>
    [Boolean(1)]
    public bool CheckFFmpegModel { get; set; }
    
    /// <summary>
    /// Gets or sets if deleted audio should be checked
    /// </summary>
    [Boolean(2)]
    [ConditionEquals(nameof(CheckFFmpegModel), true)]
    public bool CheckDeleted { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        bool hasAudio;
        if (CheckFFmpegModel)
        {
            if (Args.Variables.TryGetValue(FfmpegBuilderNode.MODEL_KEY, out var variable) == false
                || variable is FfmpegModel ffmpegModel == false)
            {
                return args.Fail("FFmpeg Model is required.");
            }

            hasAudio = ffmpegModel.AudioStreams.Any(x => x.Deleted == false || CheckDeleted);
        }
        else
        {
            var videoInfo = GetVideoInfo(args);
            if (videoInfo == null)
                return args.Fail("No video info available.");
            hasAudio = videoInfo.AudioStreams.Count > 0;
        }

        if (hasAudio)
        {
            args.Logger?.ILog("Audio found!");
            return 1;
        }
        args.Logger?.ILog("No audio found!");
        return 2;
    }
}