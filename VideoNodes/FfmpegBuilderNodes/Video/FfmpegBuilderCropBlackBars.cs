using System.Diagnostics;
using System.IO;
using System.Text;

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
            return [];
        if (seconds < 10)
            return [1];
        if (seconds > 1200)
            return [60, 240, 360, 500, 800, 1200];
        if (seconds > 360)
            return [60, 120, 240, 360];

        int increment = seconds / 5;
        return
        [
            increment,
            increment + increment,
            increment + increment + increment,
            increment + increment + increment + increment
        ];            
    }

    string Execute(string ffmpeg, string file, NodeParameters args, int vidWidth, int vidHeight, int threshold, int seconds)
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
            foreach (int ss in intervals) // check at multiple times
            {
                int intervalX = 0, intervalY = 0, intervalWidth = 0, intervalHeight = 0;
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo();
                    process.StartInfo.FileName = ffmpeg;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.ArgumentList.Add("-ss");
                    process.StartInfo.ArgumentList.Add(ss.ToString());
                    process.StartInfo.ArgumentList.Add("-i");
                    process.StartInfo.ArgumentList.Add(file);
                    process.StartInfo.ArgumentList.Add("-hide_banner");
                    process.StartInfo.ArgumentList.Add("-vframes");
                    process.StartInfo.ArgumentList.Add("25");
                    process.StartInfo.ArgumentList.Add("-vf");
                    process.StartInfo.ArgumentList.Add("cropdetect"); 
                    process.StartInfo.ArgumentList.Add("-f"); 
                    process.StartInfo.ArgumentList.Add("null"); 
                    process.StartInfo.ArgumentList.Add("-");
                        //$" -ss {ss} -i \"{file}\" -hide_banner -vframes 25 -vf cropdetect -f null -";
                    args.Logger?.DLog("Executing ffmpeg " + string.Join(' ', process.StartInfo.ArgumentList
                        .Select(x => x.IndexOf(" ", StringComparison.Ordinal) > 0 ? "\"" + x + "\"" : x)
                        .ToArray()));
                        
                    process.Start();
                    string output = process.StandardError.ReadToEnd();
                    // Console.WriteLine(output);
                    string error = process.StandardError.ReadToEnd();
                    if (process.WaitForExit(30_000) == false)
                    {
                        process.Kill();
                        args.Logger.ELog("Aborted FFmpeg due to timeout");
                        continue;
                    }

                    var dimMatch = Regex.Match(output, @"Stream #[\d]+:[\d]+(.*?)Video:(.*?)([\d]+)x([\d]+)",
                        RegexOptions.Multiline);
                    if (dimMatch.Success == false)
                    {
                        args.Logger?.WLog("Can't find dimensions for video");
                        continue;
                    }

                    var matches = Regex.Matches(output, @"(?<=(crop=))([\d]+:){3}[\d]+");
                    foreach (Match match in matches)
                    {
                        int[] parts = match.Value.Split(':').Select(x => int.Parse(x)).ToArray();
                        intervalX = parts[2];
                        intervalY = parts[3];
                        intervalWidth = parts[0];
                        intervalHeight = parts[1];
                        x = Math.Min(x, parts[2]);
                        y = Math.Min(y, parts[3]);
                        width = Math.Max(width, parts[0]);
                        height = Math.Max(height, parts[1]);
                    }
                }

                string imgFile = Path.Combine(args.TempPath, Guid.NewGuid() + ".jpg");
                if (ExtractFrameWithFFmpeg(args, ffmpeg, file, ss, imgFile) && File.Exists(imgFile))
                {
                    // draw rectangle on frame
                    args.ImageHelper?.DrawRectangleOnImage(imgFile, intervalX, intervalY, intervalWidth, intervalHeight);
                    args.LogImage(imgFile);
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
    
    
    /// <summary>
    /// Extracts a frame from a video at a specified time, scales it to 640x480 resolution, and saves it as a JPEG image using FFmpeg.
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <param name="ffmpeg">The path to the FFmpeg executable.</param>
    /// <param name="inputFile">The input video file.</param>
    /// <param name="ss">The time offset in seconds.</param>
    /// <param name="destination">The output image file path.</param>
    /// <returns>True if the frame was extracted and saved successfully, otherwise false.</returns>
    static bool ExtractFrameWithFFmpeg(NodeParameters args, string ffmpeg, string inputFile, int ss, string destination)
    {
        try
        {
            // Start the process
            using (Process process = new Process())
            {
                process.StartInfo = new ProcessStartInfo();
                process.StartInfo.FileName = ffmpeg;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                // Specify arguments using ArgumentList to avoid escaping issues
                process.StartInfo.ArgumentList.Add("-ss");
                process.StartInfo.ArgumentList.Add(ss.ToString());
                process.StartInfo.ArgumentList.Add("-i");
                process.StartInfo.ArgumentList.Add(inputFile);
                // process.StartInfo.ArgumentList.Add("-vf");
                // process.StartInfo.ArgumentList.Add("select=eq(n\\,0),scale=640:480");
                process.StartInfo.ArgumentList.Add("-vframes");
                process.StartInfo.ArgumentList.Add("1");
                process.StartInfo.ArgumentList.Add("-update");
                process.StartInfo.ArgumentList.Add("1");
                process.StartInfo.ArgumentList.Add(destination);
                process.StartInfo.ArgumentList.Add("-y");
                
                // Capture and display the output and error messages separately
                StringBuilder outputBuilder = new ();
                StringBuilder errorBuilder = new ();

                process.OutputDataReceived += (sender, e) => outputBuilder.AppendLine(e.Data);
                process.ErrorDataReceived += (sender, e) => errorBuilder.AppendLine(e.Data);

                process.Start();

                // Begin asynchronous read operations on stdout and stderr
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                
                if (process.WaitForExit(20_000) == false)
                {
                    process.Kill();
                    args.Logger?.ELog("Timed out extracting image");
                }

                if (File.Exists(destination))
                    return true;

                // Get the output and error messages
                string output = outputBuilder.ToString();
                string error = errorBuilder.ToString();
                // Log the output and error messages if needed
                if (!string.IsNullOrWhiteSpace(output))
                {
                    Console.WriteLine("Output from ffmpeg:");
                    Console.WriteLine(output);
                }

                if (string.IsNullOrWhiteSpace(error) == false)
                {
                    args.Logger?.WLog($"Error from ffmpeg: {error}");
                }
                args.Logger?.WLog($"Error extracting frame: {error}");
                return false;
            }
        }
        catch (Exception ex)
        {
            args.Logger?.ELog($"An error occurred: {ex.Message}");
            return false;
        }
    }
}