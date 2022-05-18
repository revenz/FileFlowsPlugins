namespace FileFlows.VideoNodes
{
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class SubtitleExtractor : EncodingNode
    {
        public override int Outputs => 2;

        public override string Icon => "fas fa-comment-dots";

        [Text(1)]
        public string Language { get; set; }

        [File(2)]
        public string OutputFile { get; set; }

        [Boolean(3)]
        public bool SetWorkingFile { get; set; }
        private Dictionary<string, object> _Variables;
        public override Dictionary<string, object> Variables => _Variables;
        public SubtitleExtractor()
        {
            _Variables = new Dictionary<string, object>()
            {
                { "sub.FileName", "/path/to/subtitle.sub" }
            };
        }

        public override int Execute(NodeParameters args)
        {
            try
            {
                VideoInfo videoInfo = GetVideoInfo(args);
                if (videoInfo == null)
                    return -1;
                
                // ffmpeg -i input.mkv -map "0:m:language:eng" -map "-0:v" -map "-0:a" output.srt
                var subTrack = videoInfo.SubtitleStreams?.Where(x => string.IsNullOrEmpty(Language) || x.Language?.ToLower() == Language.ToLower()).FirstOrDefault();
                if (subTrack == null)
                {
                    args.Logger?.ILog("No subtitles found to extract");
                    return 2;
                }

                if (string.IsNullOrEmpty(OutputFile) == false)
                {
                    OutputFile = args.ReplaceVariables(OutputFile, true);
                }
                else
                {
                    var file = new FileInfo(args.FileName);


                    OutputFile = file.FullName.Substring(0, file.FullName.LastIndexOf(file.Extension));
                }
                OutputFile = args.MapPath(OutputFile);

                string extension = "srt";
                if (subTrack.Codec?.ToLower()?.Contains("pgs") == true)
                    extension = "sup";
                if (OutputFile.ToLower().EndsWith(".srt") || OutputFile.ToLower().EndsWith(".sup"))
                    OutputFile = OutputFile[0..^4];

                OutputFile += "." + extension;
                //bool textSubtitles = Regex.IsMatch(OutputFile, @"\.(sup)$") == false;


                var extracted = ExtractSubtitle(args, FFMPEG, "0:s:" + subTrack.TypeIndex, OutputFile);
                if(extracted)
                {
                    args.UpdateVariables(new Dictionary<string, object>
                    {
                        { "sub.FileName", OutputFile }
                    });
                    if (SetWorkingFile)
                        args.SetWorkingFile(OutputFile, dontDelete: true);

                    return 1;
                }

                return -1;
            }
            catch (Exception ex)
            {
                args.Logger?.ELog("Failed processing VideoFile: " + ex.Message);
                return -1;
            }
        }

        internal bool ExtractSubtitle(NodeParameters args, string ffmpegExe, string subtitleStream, string output)
        {
            if (File.Exists(OutputFile))
            {
                args.Logger?.ILog("File already exists, deleting file: " + OutputFile);
                File.Delete(OutputFile);
            }

            bool textSubtitles = Regex.IsMatch(OutputFile.ToLower(), @"\.(sup)$") == false;
            // -y means it will overwrite a file if output already exists
            var result = args.Process.ExecuteShellCommand(new ExecuteArgs
            {
                Command = ffmpegExe,
                ArgumentList = textSubtitles ? 
                    new[] {

                        "-i", args.WorkingFile,
                        "-map", subtitleStream,
                        output
                    } : 
                    new[] {

                        "-i", args.WorkingFile,
                        "-map", subtitleStream,
                        "-c:s", "copy",
                        output
                    }
            }).Result;

            var of = new FileInfo(OutputFile);
            if (result.ExitCode != 0)
            {
                args.Logger?.ELog("FFMPEG process failed to extract subtitles");
                args.Logger?.ILog("Unexpected exit code: " + result.ExitCode);
                args.Logger?.ILog(result.StandardOutput ?? String.Empty);
                args.Logger?.ILog(result.StandardError ?? String.Empty);
                if (of.Exists && of.Length == 0)
                {
                    // delete the output file if it created an empty file
                    try
                    {
                        of.Delete();
                    }
                    catch (Exception) { }
                }
                return false;
            }
            return of.Exists;
        }
    }
}
