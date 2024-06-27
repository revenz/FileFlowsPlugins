using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderAudioNormalization : FfmpegBuilderNode
{
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/audio-normalization";
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

    internal const string LOUDNORM_TARGET = "I=-24:LRA=7:TP=-2.0";

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        if (Model.AudioStreams?.Any() != true)
        {
            args.Logger?.ILog("No audio streams detected");
            return 2;
        }

        var localFile = args.FileService.GetLocalPath(args.WorkingFile);
        if (localFile.IsFailed)
        {
            args.Logger?.WLog("Failed to get local file: " + localFile.Error);
            return 2;
        }

        // store them in case we are creating duplicate tracks from same source, we dont need 
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
                if (normalizedTracks.TryGetValue(audio.TypeIndex, out var track))
                    item.stream.Filter.Add(track);
                else
                {
                    string twoPass = DoTwoPass(this, args, FFMPEG, audio.TypeIndex, localFile);
                    item.stream.Filter.Add(twoPass);
                    normalizedTracks.Add(audio.TypeIndex, twoPass); 
                }
            }
            else
            {
                item.stream.Filter.Add($"loudnorm={LOUDNORM_TARGET}");
            }
            normalizing = true;
        }

        return normalizing ? 1 : 2;
    }
    
    public static string DoTwoPass(EncodingNode node, NodeParameters args, string ffmpegExe, int audioIndex, string localFile)
    {
        //-af loudnorm=I=-24:LRA=7:TP=-2.0"
        string output;
        var result = node.Encode(args, ffmpegExe, new List<string>
        {
            "-hide_banner",
            "-i", localFile,
            "-strict", "-2",  // allow experimental stuff
            "-map", "0:a:" + audioIndex,
            "-af", "loudnorm=" + LOUDNORM_TARGET + ":print_format=json",
            "-f", "null",
            "-"
        }, out output, updateWorkingFile: false, dontAddInputFile: true);

        if (result == false)
            throw new Exception("Failed to process audio track");

        int index = output.LastIndexOf("{", StringComparison.Ordinal);
        if (index == -1)
            throw new Exception("Failed to detected json in output");

        string json = output[index..];
        json = json.Substring(0, json.IndexOf("}", StringComparison.Ordinal) + 1);
        if (string.IsNullOrEmpty(json))
            throw new Exception("Failed to parse TwoPass json");
        LoudNormStats? stats = JsonSerializer.Deserialize<LoudNormStats>(json);

        if (stats.input_i == "-inf" || stats.input_lra == "-inf" || stats.input_tp == "-inf" || stats.input_thresh == "-inf" || stats.target_offset == "-inf")
        {
            args.Logger?.WLog("-inf detected in loud norm two pass, falling back to single pass loud norm");
            return $"loudnorm={LOUDNORM_TARGET}";
        }

        string ar = $"loudnorm=print_format=summary:linear=true:{LOUDNORM_TARGET}:measured_I={stats.input_i}:measured_LRA={stats.input_lra}:measured_tp={stats.input_tp}:measured_thresh={stats.input_thresh}:offset={stats.target_offset}";
        return ar;
    }

    private class LoudNormStats
    {
        /* 
{
	"input_i" : "-7.47",
	"input_tp" : "12.33",
	"input_lra" : "6.70",
	"input_thresh" : "-18.13",
	"output_i" : "-24.25",
	"output_tp" : "-3.60",
	"output_lra" : "5.90",
	"output_thresh" : "-34.74",
	"normalization_type" : "dynamic",
	"target_offset" : "0.25"
}
       */
        public string input_i { get; set; }
        public string input_tp { get; set; }
        public string input_lra { get; set; }
        public string input_thresh { get; set; }
        public string target_offset { get; set; }
    }

}
