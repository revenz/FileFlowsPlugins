namespace FileFlows.VideoNodes
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public class FFMPEG : EncodingNode
    {
        public override int Outputs => 1;

        [DefaultValue("-i {WorkingFile} {TempDir}output.mkv")]
        [TextArea(1)]
        [Required]
        public string CommandLine { get; set; }

        [DefaultValue("mkv")]
        [Text(2)]
        [Required]
        public string Extension { get; set; }

        public override string Icon => "far fa-file-video";

        public List<string> GetFFMPEGArgs(NodeParameters args, string outputFile)
        {
            string cmdLine = args.ReplaceVariables(CommandLine);

            List<string> ffArgs = cmdLine.SplitCommandLine().Select(x =>
            {
                if (x.ToLower() == "{workingfile}") return args.WorkingFile;
                if (x.ToLower() == "{output}") return outputFile;
                return x;
            }).ToList();
            return ffArgs;
        }

        public override int Execute(NodeParameters args)
        {
            if (string.IsNullOrEmpty(CommandLine))
            {
                args.Logger.ELog("Command Line not set");
                return -1;
            }
            this.args = args;
            try
            {
                VideoInfo videoInfo = GetVideoInfo(args);
                if (videoInfo == null)
                    return -1;

                string ffmpegExe = GetFFMpegExe(args);
                if (string.IsNullOrEmpty(ffmpegExe))
                    return -1;

                if (string.IsNullOrEmpty(Extension))
                    Extension = "mkv";

                string outputFile = Path.Combine(args.TempPath, Guid.NewGuid().ToString() + "." + Extension);
                var ffArgs = GetFFMPEGArgs(args, outputFile);

                if (Encode(args, ffmpegExe, ffArgs, updateWorkingFile: false, dontAddInputFile: true, dontAddOutputFile: true) == false)
                    return -1;

                if (File.Exists(outputFile))
                {
                    args.Logger?.ILog("Output file exists, updating working file: " + outputFile);
                    args.SetWorkingFile(outputFile);
                }

                return 1;
            }
            catch (Exception ex)
            {
                args.Logger.ELog("Failed processing VideoFile: " + ex.Message);
                return -1;
            }
        }
    }
}