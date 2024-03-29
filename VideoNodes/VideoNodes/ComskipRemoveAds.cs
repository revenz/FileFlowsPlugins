﻿namespace FileFlows.VideoNodes
{
    using FileFlows.Plugin;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;


    public class ComskipRemoveAds: EncodingNode
    {
        public override int Outputs => 2;

        public override int Execute(NodeParameters args)
        {
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
            float last = -1;
            List<BreakPoint> breakPoints = new List<BreakPoint>();
            foreach(string line in text.Split(new string[] { "\r\n", "\n", "\r"}, StringSplitOptions.RemoveEmptyEntries))
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

                BreakPoint bp = new BreakPoint();
                bp.Start = start;
                bp.End = end;
                breakPoints.Add(bp);    
            }

            if(breakPoints.Any() == false)
            {
                args.Logger?.ILog("No break points detected in file");
                return 2;
            }

            List<string> segments = new List<string>();

            float segStart = 0;
            string extension = args.WorkingFile.Substring(args.WorkingFile.LastIndexOf(".") + 1);
            string segmentPrefix = Path.Combine(args.TempPath, Guid.NewGuid().ToString())+"_";
            int count = 0;
            List<string> segmentsInfo = new List<string>();
            foreach (BreakPoint bp in breakPoints)
            {
                if (EncodeSegment(segStart, bp.Start) == false)
                {
                    args.Logger?.ELog("Failed to create segment: " + count);
                    return 2;
                }
                segStart = bp.End;
            }
            // add the end
            if (EncodeSegment(segStart, totalTime) == false)
            {
                args.Logger?.ELog("Failed to create segment: " + count);
                return 2;
            }

            // stitch file back together
            string concatList = segmentPrefix + "concatlist.txt";
            string concatListContents = String.Join(Environment.NewLine, segments.Select(x => $"file '{x}'"));
            File.WriteAllText(concatList, concatListContents);

            args.Logger?.ILog("====================================================");
            foreach (var str in segmentsInfo)
                args.Logger?.ILog(str);
            args.Logger?.ILog("Concat list:");
            foreach (var line in concatListContents.Split(new String[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                args.Logger?.ILog(line);
            }
            args.Logger?.ILog("====================================================");


            List<string> ffArgs = new List<string>
            {
                "-f", "concat",
                "-safe", "0",
                "-i", concatList,
                "-c", "copy"
            };

            bool concatResult = Encode(args, FFMPEG, ffArgs, dontAddInputFile: true, extension: extension);

            foreach(string segment in segments.Union(new[] { concatList }))
            {
                try
                {
                    File.Delete(segment);
                }
                catch (Exception) { }
            }

            if (concatResult)
                return 1;
            args.Logger?.ELog("Failed to stitch file back together");
            return 2;

            bool EncodeSegment(float start, float end)
            {
                string segment = segmentPrefix + (++count).ToString("D2") + "." + extension;
                float duration = end - start;
                if (duration < 30)
                {
                    args.Logger?.ILog("Segment is less than 30 seconds, skipping");
                    return true;
                }
                List<string> ffArgs = new List<string>
                {
                    "-ss", start.ToString(),
                    "-t", duration.ToString(),
                    "-c", "copy"
                };
                if (Encode(args, FFMPEG, ffArgs, outputFile: segment, updateWorkingFile: false))
                {
                    segments.Add(segment);
                    segmentsInfo.Add(DebugString(start, end));
                    return true;
                }
                return false;
            }
        }

        private string DebugString(float start, float end)
        {
            var tsStart = new TimeSpan((long)start * TimeSpan.TicksPerSecond);
            var tsEnd= new TimeSpan((long)end * TimeSpan.TicksPerSecond);

            return "Segment: " + tsStart.ToString(@"mm\:ss") + " to " + tsEnd.ToString(@"mm\:ss");
        }
        private class BreakPoint
        {
            public float Start { get; set; }
            public float End { get; set; }

            public float Duration => End - Start;
        }
    }
}
