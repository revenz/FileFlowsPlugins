using System.Diagnostics;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;
public class FfmpegBuilderCropBlackBars : FfmpegBuilderNode
{
    [NumberInt(1)]
    [DefaultValue(10)]
    public int CroppingThreshold { get; set; }
    public override int Outputs => 2;

    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/crop-black-bars";

    public override int Execute(NodeParameters args)
    {
        var videoInfo = GetVideoInfo(args);
        if (videoInfo == null || videoInfo.VideoStreams?.Any() != true)
            return -1;


        string crop = Detect(FFMPEG, videoInfo, args, this.CroppingThreshold);
        if (string.IsNullOrWhiteSpace(crop))
            return 2;

        //var parts = crop.Split(':');
        ////parts[2] = "iw-" + parts[2];
        ////parts[3] = "ih-" + parts[3];
        //crop = String.Join(":", parts.Take(2));

        args.Logger?.ILog("Black bars detected, crop: " + crop);

        var video = Model.VideoStreams[0];
        video.Filter.AddRange(new[] { "crop=" + crop });
        return 1;
    }


    public string Detect(string ffmpeg, VideoInfo videoInfo, NodeParameters args, int threshold)
    {
        int vidWidth = videoInfo.VideoStreams[0].Width;
        int vidHeight = videoInfo.VideoStreams[0].Height;
        if (vidWidth < 1)
        {
            args.Logger?.ELog("Failed to find video width");
            return string.Empty;
        }
        if (vidHeight < 1)
        {
            args.Logger?.ELog("Failed to find video height");
            return string.Empty;
        }

        var localFile = args.FileService.GetLocalPath(args.WorkingFile);
        if (localFile.IsFailed)
        {
            args.Logger?.ELog("Failed to get local file: " + localFile.Error);
            return string.Empty;
        }
        return Execute(ffmpeg, localFile, args, vidWidth, vidHeight, threshold, (int)videoInfo.VideoStreams[0].Duration.TotalSeconds);
    }

    private int[] GetTimeIntervals(int seconds)
    {
        if (seconds < 2)
            return new int[] { };
        if (seconds < 10)
            return new int[] { 1 };
        if (seconds > 360)
            return new int[] { 60, 120, 240, 360 };

        int increment = seconds / 5;
        return new int[]
        {
            increment,
            increment + increment,
            increment + increment + increment,
            increment + increment + increment + increment,
        };            
    }

    string Execute(string ffplay, string file, NodeParameters args, int vidWidth, int vidHeight, int threshold, int seconds)
    {
        try
        {
            int x = int.MaxValue;
            int y = int.MaxValue;
            int width = 0;
            int height = 0;
            var intervals = GetTimeIntervals(seconds);
            if (intervals.Length == 0)
                return string.Empty;
            foreach (int ss in intervals)  // check at multiple times
            {
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo();
                    process.StartInfo.FileName = ffplay;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.Arguments = $" -ss {ss} -i \"{file}\" -hide_banner -vframes 25 -vf cropdetect -f null -";
                    args.Logger?.DLog("Executing ffmpeg " + process.StartInfo.Arguments);
                    process.Start();
                    string output = process.StandardError.ReadToEnd();
                    Console.WriteLine(output);
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    var dimMatch = Regex.Match(output, @"Stream #[\d]+:[\d]+(.*?)Video:(.*?)([\d]+)x([\d]+)", RegexOptions.Multiline);
                    if (dimMatch.Success == false)
                    {
                        args.Logger?.WLog("Can't find dimensions for video");
                        continue;
                    }

                    var matches = Regex.Matches(output, @"(?<=(crop=))([\d]+:){3}[\d]+");
                    foreach (Match match in matches)
                    {
                        int[] parts = match.Value.Split(':').Select(x => int.Parse(x)).ToArray();
                        x = Math.Min(x, parts[2]);
                        y = Math.Min(y, parts[3]);
                        width = Math.Max(width, parts[0]);
                        height = Math.Max(height, parts[1]);
                    }
                }
            }

            if (width == 0 || height == 0)
            {
                args.Logger?.WLog("Width/Height not detected: " + width + "x" + height);
                return string.Empty;
            }
            if (x == 0 && y == 0)
            {
                // nothing to do
                return string.Empty;
            }

            if (x == int.MaxValue)
                x = 0;
            if (y == int.MaxValue)
                y = 0;

            if (threshold < 0)
                threshold = 0;

            args.Logger?.DLog($"Video dimensions: {vidWidth}x{vidHeight}");

            var willCrop = TestAboveThreshold(vidWidth, vidHeight, width, height, threshold);
            args.Logger?.ILog($"Crop detection, x:{x}, y:{y}, width: {width}, height: {height}, total:{willCrop.diff}, threshold:{threshold}, above threshold: {willCrop}");

            return willCrop.crop ? $"{width}:{height}:{x}:{y}" : string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    (bool crop, int diff) TestAboveThreshold(int vidWidth, int vidHeight, int detectedWidth, int detectedHeight, int threshold)
    {
        int diff = (vidWidth - detectedWidth) + (vidHeight - detectedHeight);
        return (diff > threshold, diff);

    }
}