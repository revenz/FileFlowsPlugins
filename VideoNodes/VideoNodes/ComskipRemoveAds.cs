using FileFlows.VideoNodes.Helpers;

namespace FileFlows.VideoNodes;

public class ComskipRemoveAds: EncodingNode
{
    /// <inheritdoc />
    public override int Outputs => 2;

    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/comskip-remove-ads";

    /// <summary>
    /// Gets or sets if comskip should be run if no EDL file is found
    /// </summary>
    [Boolean(1)]
    public bool RunComskipIfNoEdl { get; set; }

    private string GetEdlFile(NodeParameters args)
    {
        string edlFile = args.WorkingFile.Substring(0, args.WorkingFile.LastIndexOf(".", StringComparison.Ordinal) + 1) +
            "edl";
        if (args.FileService.FileIsLocal(edlFile))
        {
            if (System.IO.File.Exists(edlFile))
                return edlFile;
            
            return args.WorkingFile.Substring(0,
                    args.WorkingFile.LastIndexOf(".", StringComparison.Ordinal) + 1) + "edl";
        }
        if (args.FileService.FileExists(edlFile).Is(true) == false)
        {
            edlFile = args.WorkingFile.Substring(0,
                args.WorkingFile.LastIndexOf(".", StringComparison.Ordinal) + 1) + "edl";
            
            if (args.FileService.FileExists(edlFile).Is(true) == false)
                return string.Empty;
                
        }
        
        var result = args.FileService.GetLocalPath(edlFile);
        if (result.IsFailed)
        {
            args.Logger.ELog("Failed to download edl file locally: " + result.Error);
            return null;
        }

        return result;

    }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        VideoInfo videoInfo = GetVideoInfo(args);
        if (videoInfo == null)
            return -1;

        var localFile = args.FileService.GetLocalPath(args.WorkingFile);
        if (localFile.Failed(out string error))
        {
            args.Logger?.WLog("Failed to get file locally: " + error);
            return -1;
        }

        if (localFile != args.WorkingFile)
            args.SetWorkingFile(localFile);
        
        float totalTime = (float)videoInfo.VideoStreams[0].Duration.TotalSeconds;

        string edlFile = GetEdlFile(args);

        if (string.IsNullOrWhiteSpace(edlFile) || System.IO.File.Exists(edlFile) == false)
        {
            if (RunComskipIfNoEdl)
            {
                args.Logger?.ILog("No edl file found, attempting to run comskip");
                var runComskipResult = ComskipHelper.RunComskip(args, localFile);
                if (runComskipResult.Failed(out string csError))
                {
                    args.Logger?.ELog(csError);
                    return 2;
                }
                edlFile = runComskipResult;
            }
            else
            {

                args.Logger?.ILog("No EDL file found for file");
                return 2;
            }
        }

        string text = System.IO.File.ReadAllText(edlFile) ?? string.Empty;
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
        string extension = args.WorkingFile[(args.WorkingFile.LastIndexOf(".", StringComparison.Ordinal) + 1)..];
        string segmentPrefix = System.IO.Path.Combine(args.TempPath, Guid.NewGuid().ToString())+"_";
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
        System.IO.File.WriteAllText(concatList, concatListContents);

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
            "-map", "0",
            "-c", "copy"
        };

        bool concatResult = Encode(args, FFMPEG, ffArgs, dontAddInputFile: true, extension: extension);

        foreach(string segment in segments.Union(new[] { concatList }))
        {
            try
            {
                System.IO.File.Delete(segment);
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
                "-map", "0",
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
