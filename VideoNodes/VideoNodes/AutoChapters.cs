namespace FileFlows.VideoNodes
{
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;


    public class AutoChapters: EncodingNode
    {
        public override int Outputs => 2;

        [NumberInt(1)]
        [DefaultValue(60)]
        public int MinimumLength { get; set; } = 60;

        [NumberInt(2)]
        [DefaultValue(45)]
        public int Percent { get; set; } = 45;

        public override int Execute(NodeParameters args)
        {
            string ffmpegExe = GetFFMpegExe(args);
            if (string.IsNullOrEmpty(ffmpegExe))
                return -1;
            VideoInfo videoInfo = GetVideoInfo(args);
            if (videoInfo == null)
                return -1;

            if (videoInfo.Chapters?.Count > 3)
            {
                args.Logger.ILog(videoInfo.Chapters.Count + " chapters already detected in file");
                return 2;
            }

            string output;
            var result = Encode(args, ffmpegExe, new List<string>
            {
                "-hide_banner",
                "-i", args.WorkingFile,
                "-filter:v", $"select='gt(scene,{Percent / 100f})',showinfo",
                "-f", "null",
                "-"
            }, out output, updateWorkingFile: false, dontAddInputFile: true);

            if(result == false)
            {
                args.Logger?.WLog("Failed to detect scenes");
                return 2;
            }


            if (MinimumLength < 30)
            {
                args.Logger?.ILog("Mimium length set to invalid number, defaulting to 60 seconds");
                MinimumLength = 60;
            }
            else
            {
                args.Logger?.ILog($"Minimum length of chapter {MinimumLength} seconds");
            }

            StringBuilder metadata = new StringBuilder();
            metadata.AppendLine(";FFMETADATA1");
            metadata.AppendLine("");

            int chapter = 0;

            TimeSpan previous = TimeSpan.Zero;
            foreach (Match match in Regex.Matches(output, @"(?<=(pts_time:))[\d]+\.[\d]+"))
            {
                TimeSpan time = TimeSpan.FromSeconds(double.Parse(match.Value));                
                if(Math.Abs((time - previous).TotalSeconds) < MinimumLength)
                    continue;

                AddChapter(previous, time);
                previous = time;
            }

            var totalTime = TimeSpan.FromSeconds(videoInfo.VideoStreams[0].Duration.TotalSeconds);
            if (Math.Abs((totalTime - previous).TotalSeconds) > MinimumLength)
                AddChapter(previous, totalTime);

            if (chapter == 0)
            {
                args.Logger?.ILog("No ads found in edl file");
                return 2;
            }

            string tempMetaDataFile = Path.Combine(args.TempPath, Guid.NewGuid().ToString() + ".txt");
            File.WriteAllText(tempMetaDataFile, metadata.ToString());

            string[] ffArgs = new[] { "-i", tempMetaDataFile, "-map_metadata", "1", "-codec", "copy", "-max_muxing_queue_size", "1024" };
            if (Encode(args, ffmpegExe, ffArgs.ToList())) 
            {
                args.Logger?.ILog($"Adding {chapter} chapters to file");
                return 1;
            }
            args.Logger?.ELog("Processing failed");
            return -1;

            void AddChapter(TimeSpan start, TimeSpan end)
            {

                metadata.AppendLine("[CHAPTER]");
                metadata.AppendLine("TIMEBASE=1/1000");
                metadata.AppendLine("START=" + ((int)(start.TotalSeconds * 1000)));
                metadata.AppendLine("END=" + ((int)(end.TotalSeconds * 1000)));
                metadata.AppendLine("title=Chapter " + (++chapter));
                metadata.AppendLine();
            }
        }
    }
}
