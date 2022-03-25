namespace FileFlows.VideoNodes
{
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Text.RegularExpressions;

    public class AudioNormalization: EncodingNode
    {
        public override int Outputs => 2;

        public override string Icon => "fas fa-volume-up";

        [Boolean(1)]
        public bool AllAudio { get; set; }

        [Boolean(2)]
        public bool TwoPass { get; set; }

        [TextVariable(3)]
        public string Pattern { get; set; }

        [Boolean(4)]
        public bool NotMatching { get; set; }

        const string LOUDNORM_TARGET = "I=-24:LRA=7:TP=-2.0";

        public override int Execute(NodeParameters args)
        {
            try
            {
                VideoInfo videoInfo = GetVideoInfo(args);
                if (videoInfo == null)
                    return -1;

                string ffmpegExe = GetFFMpegExe(args);
                if (string.IsNullOrEmpty(ffmpegExe))
                    return -1;

                if (videoInfo.AudioStreams?.Any() != true)
                {
                    args.Logger?.ILog("No audio streams detected");
                    return 2;
                }

                List<string> ffArgs = new List<string>();

                ffArgs.AddRange(new[] { "-strict", "-2" }); // allow experimental stuff

                ffArgs.AddRange(new[] { "-c", "copy" });


                if (videoInfo.VideoStreams?.Any() == true)
                    ffArgs.AddRange(new[] { "-map", "0:v" });

                List<int> tracksToNormalize = new ();
                for (int j = 0; j < videoInfo.AudioStreams.Count;j++)
                {
                    var audio = videoInfo.AudioStreams[j];

                    if(string.IsNullOrEmpty(Pattern) == false)
                    {
                        string audioString = audio.Title + ":" + audio.Language + ":" + audio.Codec;
                        args.Logger?.ILog($"Audio Track [{j}] test string: {audioString}");
                        bool match = new Regex(Pattern, RegexOptions.IgnoreCase).IsMatch(audioString);
                        if (NotMatching)
                            match = !match;
                        if (match == false)
                        {
                            ffArgs.AddRange(new[] { "-map", $"0:a:{j}" });
                            continue;
                        }
                    }

                    if (AllAudio || j == 0)
                    {
                        int sampleRate = audio.SampleRate > 0 ? audio.SampleRate : 48_000;
                        if (TwoPass)
                        {
                            string twoPass = DoTwoPass(args, ffmpegExe, j);
                            ffArgs.AddRange(new[] { "-map", $"0:a:{j}", "-c:a:" + j, audio.Codec, "-ar", sampleRate.ToString(), "-filter:a:" + j, twoPass });
                        }
                        else
                        {
                            ffArgs.AddRange(new[] { "-map", $"0:a:{j}", "-c:a:" + j, audio.Codec, "-ar", sampleRate.ToString(), "-filter:a:" + j, $"loudnorm={LOUDNORM_TARGET}" });
                        }
                        tracksToNormalize.Add(j);
                    }
                    else
                    {
                        ffArgs.AddRange(new[] { "-map", $"0:a:{j}" });
                    }
                }

                if (tracksToNormalize.Any() == false)
                {
                    args.Logger?.ILog("No audio streams to normalize");
                    return 2;
                }

                foreach (int i in tracksToNormalize)
                    args.Logger?.ILog($"Normalizing track [{i}]: {videoInfo.AudioStreams[i].Title};{videoInfo.AudioStreams[i].Language};{videoInfo.AudioStreams[i].Codec};");

                if (videoInfo.SubtitleStreams?.Any() == true)
                    ffArgs.AddRange(new[] { "-map", "0:s" });

                string extension = new FileInfo(args.WorkingFile).Extension;
                if (extension.StartsWith("."))
                    extension = extension.Substring(1);

                if (Encode(args, ffmpegExe, ffArgs, extension) == false)
                    return -1;
                
                return 1;
            }
            catch (Exception ex)
            {
                args.Logger?.ELog("Failed processing VideoFile: " + ex.Message);
                return -1;
            }
        }

        [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Deserialize<FileFlows.VideoNodes.AudioNormalization.LoudNormStats>(string, System.Text.Json.JsonSerializerOptions?)")]
        public string DoTwoPass(NodeParameters args,string ffmpegExe, int audioIndex)
        {
            //-af loudnorm=I=-24:LRA=7:TP=-2.0"
            string output;
            var result = Encode(args, ffmpegExe, new List<string>
            {
                "-hide_banner",
                "-i", args.WorkingFile,
                "-strict", "-2",  // allow experimental stuff
                "-map", "0:a:" + audioIndex,
                "-af", "loudnorm=" + LOUDNORM_TARGET + ":print_format=json",
                "-f", "null",
                "-"
            }, out output, updateWorkingFile: false, dontAddInputFile: true);

            if (result == false)
                throw new Exception("Failed to prcoess audio track");

            int index = output.LastIndexOf("{");
            if (index == -1)
                throw new Exception("Failed to detected json in output");
            string json = output.Substring(index);
            json = json.Substring(0, json.IndexOf("}") + 1);
            if (string.IsNullOrEmpty(json))
                throw new Exception("Failed to parse TwoPass json");
            LoudNormStats stats = JsonSerializer.Deserialize<LoudNormStats>(json);
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
}
