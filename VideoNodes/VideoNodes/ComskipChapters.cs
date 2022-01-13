namespace FileFlows.VideoNodes
{
    using FileFlows.Plugin;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;


    public class ComskipChapters: EncodingNode
    {
        public override int Outputs => 2;

        public override int Execute(NodeParameters args)
        {
            string ffmpegExe = GetFFMpegExe(args);
            if (string.IsNullOrEmpty(ffmpegExe))
                return -1;
            VideoInfo videoInfo = GetVideoInfo(args);
            if (videoInfo == null)
                return -1;
            float totalTime = (float)videoInfo.VideoStreams[0].Duration.TotalSeconds;


            string edlFile = args.WorkingFile.Substring(0, args.WorkingFile.LastIndexOf(".") + 1) + "edl";
            if(File.Exists(edlFile) == false)
                edlFile = args.WorkingFile.Substring(0, args.WorkingFile.LastIndexOf(".") + 1) + "edl";
            if (File.Exists(edlFile) == false)
            {
                args.Logger?.ILog("No EDL file found for file");
                return 2;
            }

            string text = File.ReadAllText(edlFile) ?? string.Empty;
            float last = 0;

            StringBuilder metadata = new StringBuilder();
            metadata.AppendLine(";FFMETADATA1");
            metadata.AppendLine("");
            int chapter = 0;

            foreach (string line in text.Split(new string[] { "\r\n", "\n", "\r"}, StringSplitOptions.RemoveEmptyEntries))
            {
                // 93526.47 93650.13 0
                string[] parts = line.Split(new[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                    continue;
                float start = 0;
                float end = 0;
                if (float.TryParse(parts[0], out start) == false || float.TryParse(parts[1], out end) == false)
                    continue;

                if (start < last)
                    continue;

                AddChapter(last, start);
                last = end;
            }

            if(chapter == 0)
            {
                args.Logger?.ILog("No ads found in edl file");
                return 2;
            }
            AddChapter(last, totalTime);

            string tempMetaDataFile = Path.Combine(args.TempPath, Guid.NewGuid().ToString() + ".txt");
            File.WriteAllText(tempMetaDataFile, metadata.ToString());

            string ffArgs = $"-i \"{tempMetaDataFile}\" -map_metadata 1 -codec copy -max_muxing_queue_size 1024";
            if (Encode(args, ffmpegExe, ffArgs)) 
            {
                args.Logger?.ILog($"Adding {chapter} chapters to file");
                return 1;
            }
            args.Logger?.ELog("Processing failed");
            return -1;

            void AddChapter(float start, float end)
            {

                metadata.AppendLine("[CHAPTER]");
                metadata.AppendLine("TIMEBASE=1/1000");
                metadata.AppendLine("START=" + ((int)(start * 1000)));
                metadata.AppendLine("END=" + ((int)(end * 1000)));
                metadata.AppendLine("title=Chapter " + (++chapter));
                metadata.AppendLine();
            }
        }
    }
}
