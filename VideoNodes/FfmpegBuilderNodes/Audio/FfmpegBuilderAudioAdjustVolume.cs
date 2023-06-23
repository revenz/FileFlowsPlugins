using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using System.Diagnostics.CodeAnalysis;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderAudioAdjustVolume : FfmpegBuilderNode
{
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/audio-adjust-volume";

    public override string Icon => "fas fa-volume-up";

    [NumberInt(1)]
    [Range(0, 1000)]
    public int VolumePercent { get; set; }

    [Boolean(2)]
    public bool AllAudio { get; set; }

    [TextVariable(3)]
    public string Pattern { get; set; }

    [Boolean(4)]
    public bool NotMatching { get; set; }

    public override int Execute(NodeParameters args)
    {
        if (Model.AudioStreams?.Any() != true)
        {
            args.Logger?.ILog("No audio streams detected");
            return 2;
        }

        if (VolumePercent == 100)
        {
            args.Logger?.ILog("Volume percent set to 100, no adjustment necassary");
            return 2;
        }

        float volume = this.VolumePercent / 100f;
        bool working = false;
        foreach (var item in Model.AudioStreams.Select((x, index) => (stream: x, index)))
        {
            var audio = item.stream.Stream;
            if (item.stream.Deleted)
                continue;

            if (string.IsNullOrEmpty(Pattern) == false)
            {
                if (PatternMatches2(this.Pattern, item.index, item.stream, this.NotMatching) == false)
                    continue;
            }
            else if (AllAudio == false && item.index > 0)
                continue;

            item.stream.Filter.Add($"volume={volume.ToString(".0######")}");
            working = true;
        }
        return working ? 1 : 2;
    }
}
