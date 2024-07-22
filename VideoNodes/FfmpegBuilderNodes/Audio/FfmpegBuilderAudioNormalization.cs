using System.Text.Json;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Flow element that normalizes the audio
/// </summary>
public class FfmpegBuilderAudioNormalization : FfmpegBuilderNode
{
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/audio-normalization";
    /// <inheritdoc />
    public override string Icon => "fas fa-volume-up";
    /// <inheritdoc />
    public override int Outputs => 2;
    
    /// <summary>
    /// Gets or sets if all audio should be normalised
    /// </summary>
    [Boolean(1)]
    public bool AllAudio { get; set; }

    /// <summary>
    /// Gets or sets if the audio should be normalised using two passes or if false, a single pass
    /// </summary>
    [Boolean(2)]
    public bool TwoPass { get; set; }

    /// <summary>
    /// Gets or sets the pattern to match against the audio file
    /// </summary>
    [TextVariable(3)]
    public string Pattern { get; set; }

    /// <summary>
    /// Gets or sets if the match should be inversed
    /// </summary>
    [Boolean(4)]
    public bool NotMatching { get; set; }

    /// <summary>
    /// The loud norm target
    /// </summary>
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
                    var twoPassResult = DoTwoPass(this, args, FFMPEG, audio.TypeIndex, localFile);
                    if (twoPassResult.Failed(out var error))
                    {
                        args.Logger?.ELog(error);
                        args.FailureReason = error;
                        return -1;
                    }

                    var twoPass = twoPassResult.Value;
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
    
    /// <summary>
    /// Do a two pass normalization against a file
    /// </summary>
    /// <param name="node">the encoding node</param>
    /// <param name="args">the node parameters</param>
    /// <param name="ffmpegExe">the FFmpeg executable</param>
    /// <param name="audioIndex">the audio index in the file</param>
    /// <param name="localFile">the local filename of the file</param>
    /// <returns>the result of the normalization</returns>
    public static Result<string> DoTwoPass(EncodingNode node, NodeParameters args, string ffmpegExe, int audioIndex, string localFile)
    {
        //-af loudnorm=I=-24:LRA=7:TP=-2.0"
        var result = node.Encode(args, ffmpegExe, [
            "-hide_banner",
            "-i", localFile,
            "-strict", "-2", // allow experimental stuff
            "-map", "0:a:" + audioIndex,
            "-af", "loudnorm=" + LOUDNORM_TARGET + ":print_format=json",
            "-f", "null",
            "-"
        ], out var output, updateWorkingFile: false, dontAddInputFile: true);

        if (result == false)
            return Result<string>.Fail("Failed to process audio track");

        var loudNorm = ExtractParsedLoudnormJson(args.Logger, output);

        if (loudNorm.Count == 0)
        {
            args.Logger?.WLog("No LoudNormStats found in:\n" + output);
            return Result<string>.Fail("Failed to detected json in output");
        }

        LoudNormStats? stats = loudNorm.FirstOrDefault(x =>
        {
            if (x.input_i == "-inf" || x.input_lra == "-inf" || x.input_tp == "-inf" || x.input_thresh == "-inf" ||
                x.target_offset == "-inf")
                return false;
            return true;
        });
        if (stats == null)
        {
            args.Logger?.WLog("-inf detected in loud norm two pass, falling back to single pass loud norm");
            return $"loudnorm={LOUDNORM_TARGET}";
        }

        string ar = $"loudnorm=print_format=summary:linear=true:{LOUDNORM_TARGET}:measured_I={stats.input_i}:measured_LRA={stats.input_lra}:measured_tp={stats.input_tp}:measured_thresh={stats.input_thresh}:offset={stats.target_offset}";
        return ar;
    }

    /// <summary>
    /// Extracts Loud Norm Ststs object following the [Parsed_loudnorm log entries from the provided log data.
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="log">The log as a string.</param>
    /// <returns>A list of Loud Norm Stats objects.</returns>
    static List<LoudNormStats> ExtractParsedLoudnormJson(ILogger logger, string log)
    {
        List<LoudNormStats> results = new ();
        Regex regex = new Regex(@"\[Parsed_loudnorm.*?\]\s*{(.*?)}", RegexOptions.Singleline);
        MatchCollection matches = regex.Matches(log);

        foreach (Match match in matches)
        {
            string json = "{" + match.Groups[1].Value + "}";
            try
            {
                var ln = JsonSerializer.Deserialize<LoudNormStats>(json);
                if (ln != null)
                    results.Add(ln);
            }
            catch (Exception ex)
            {
                // Ignored
                logger?.ELog("Failed to parse JSON: " + ex.Message);
                logger?.ELog("JSON:" + json);
            }
        }

        return results;
    }
    
    /// <summary>
    /// Represents the loudness normalization statistics.
    /// </summary>
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
        
        /// <summary>
        /// Integrated loudness of the input in LUFS.
        /// </summary>
        public string input_i { get; init; }

        /// <summary>
        /// True peak of the input in dBTP.
        /// </summary>
        public string input_tp { get; init; }

        /// <summary>
        /// Loudness range of the input in LU.
        /// </summary>
        public string input_lra { get; init; }

        /// <summary>
        /// Threshold of the input in LUFS.
        /// </summary>
        public string input_thresh { get; init; }

        /// <summary>
        /// Target offset for normalization in LU.
        /// </summary>
        public string target_offset { get; init; }
    }

}
