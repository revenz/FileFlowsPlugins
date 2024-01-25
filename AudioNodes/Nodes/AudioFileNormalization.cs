using FileFlows.Plugin;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace FileFlows.AudioNodes;

public class AudioFileNormalization : AudioNode
{
    public override int Inputs => 1;
    public override int Outputs => 1;
    public override FlowElementType Type => FlowElementType.Process;
    public override string HelpUrl => "https://fileflows.com/docs/plugins/audio-nodes/audio-file-normalization";

    public override string Icon => "fas fa-volume-up";


    const string LOUDNORM_TARGET = "I=-24:LRA=7:TP=-2.0";

    public override int Execute(NodeParameters args)
    {
        try
        {
            string ffmpegExe = GetFFmpeg(args);
            if (string.IsNullOrEmpty(ffmpegExe))
                return -1;

            AudioInfo AudioInfo = GetAudioInfo(args);
            if (AudioInfo == null)
                return -1;

            List<string> ffArgs = new List<string>();


            long sampleRate = AudioInfo.Frequency > 0 ? AudioInfo.Frequency : 48_000;
            
            var twoPass = DoTwoPass(args, ffmpegExe, LocalWorkingFile);
            if (twoPass.Success == false)
            {
                args.Logger?.WLog("Failed to normalize audio, skipping");
                return 1;
            }
            
            ffArgs.AddRange(new[] { "-i", args.WorkingFile, "-c:a", AudioInfo.Codec, "-ar", sampleRate.ToString(), "-af", twoPass.Normalization });

            string extension = FileHelper.GetExtension(args.WorkingFile);

            string outputFile = FileHelper.Combine(args.TempPath, Guid.NewGuid() + extension);
            ffArgs.Add(outputFile);

            var result = args.Execute(new ExecuteArgs
            {
                Command = ffmpegExe,
                ArgumentList = ffArgs.ToArray()
            });

            return result.ExitCode == 0 ? 1 : -1;
        }
        catch (Exception ex)
        {
            args.Logger?.ELog("Failed processing AudioFile: " + ex.Message);
            return -1;
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public static (bool Success, string Normalization) DoTwoPass(NodeParameters args, string ffmpegExe, string localFile)
    {
        //-af loudnorm=I=-24:LRA=7:TP=-2.0"
        var result = args.Execute(new ExecuteArgs
        {
            Command = ffmpegExe,
            ArgumentList = new[]
            {
                "-hide_banner",
                "-i", localFile,
                "-af", "loudnorm=" + LOUDNORM_TARGET + ":print_format=json",
                "-f", "null",
                "-"
            }
        });
        if (result.ExitCode != 0)
        {
            args.Logger?.WLog("Failed to process audio track");
            return (false, string.Empty);
        }

        string output = result.StandardOutput;

        int index = output.LastIndexOf("{");
        if (index == -1)
        {
            args.Logger?.WLog("Failed to detected json in output");
            return (false, string.Empty);
        }

        string json = output.Substring(index);
        json = json.Substring(0, json.IndexOf("}") + 1);
        if (string.IsNullOrEmpty(json))
        {
            args.Logger?.WLog("Failed to parse TwoPass json\"");
            return (false, string.Empty);
        }
        LoudNormStats stats = JsonSerializer.Deserialize<LoudNormStats>(json);
        string ar = $"loudnorm=print_format=summary:linear=true:{LOUDNORM_TARGET}:measured_I={stats.input_i}:measured_LRA={stats.input_lra}:measured_tp={stats.input_tp}:measured_thresh={stats.input_thresh}:offset={stats.target_offset}";
        return (true, ar);
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
