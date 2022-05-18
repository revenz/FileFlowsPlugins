using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using System.Diagnostics.CodeAnalysis;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderAudioNormalization : FfmpegBuilderNode
{
    public override string HelpUrl => "https://github.com/revenz/FileFlows/wiki/FFMPEG-Builder:-Audio-Normalization";
    public override string Icon => "fas fa-volume-up";
    public override int Outputs => 2;

    [Boolean(1)]
    public bool AllAudio { get; set; }

    [Boolean(2)]
    public bool TwoPass { get; set; }

    [TextVariable(3)]
    public string Pattern { get; set; }

    [Boolean(4)]
    public bool NotMatching { get; set; }

    [RequiresUnreferencedCode("")]
    public override int Execute(NodeParameters args)
    {
        if (Model.AudioStreams?.Any() != true)
        {
            args.Logger?.ILog("No audio streams detected");
            return 2;
        }

        // store them incase we are creating duplicate tracks from same source, we dont need 
        // to calculate the normalization each time
        Dictionary<int, string> normalizedTracks = new Dictionary<int, string>();
        bool normalizing = false;
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

            if (TwoPass)
            {
                if (normalizedTracks.ContainsKey(audio.TypeIndex))
                    item.stream.Filter.Add(normalizedTracks[audio.TypeIndex]);
                else
                {
                    string twoPass = AudioNormalization.DoTwoPass(this, args, FFMPEG, audio.TypeIndex);
                    item.stream.Filter.Add(twoPass);
                    normalizedTracks.Add(audio.TypeIndex, twoPass); 
                }
            }
            else
            {
                item.stream.Filter.Add($"loudnorm={AudioNormalization.LOUDNORM_TARGET}");
            }
            normalizing = true;
        }

        return normalizing ? 1 : 2;
    }

}
