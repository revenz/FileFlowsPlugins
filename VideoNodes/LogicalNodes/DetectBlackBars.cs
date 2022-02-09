namespace FileFlows.VideoNodes
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public class DetectBlackBars : VideoNode
    {
        public override int Outputs => 2;
        public override int Inputs => 1;
        public override FlowElementType Type => FlowElementType.Logic;
        public override string Icon => "fas fa-film";

        internal const string CROP_KEY = "VideoCrop";

        [NumberInt(1)]
        public int CroppingThreshold { get; set; }

        public override int Execute(NodeParameters args)
        {
            string ffmpeg = GetFFMpegExe(args);
            if (string.IsNullOrEmpty(ffmpeg))
                return -1;

            var videoInfo = GetVideoInfo(args);
            if (videoInfo == null || videoInfo.VideoStreams?.Any() != true)
                return -1;

            int vidWidth = videoInfo.VideoStreams[0].Width;
            int vidHeight = videoInfo.VideoStreams[0].Height;
            if(vidWidth < 1)
            {
                args.Logger?.ELog("Failed to find video width");
                return -1;
            }
            if (vidHeight < 1)
            {
                args.Logger?.ELog("Failed to find video height");
                return -1;
            }

            string crop = Execute(ffmpeg, args.WorkingFile, args, vidWidth, vidHeight);
            if (crop == string.Empty)
                return 2;

            args.Logger?.ILog("Black bars detected, crop: " + crop);
            args.Parameters.Add(CROP_KEY, crop);

            return 1;
        }

        public string Execute(string ffplay, string file, NodeParameters args, int vidWidth, int vidHeight)
        {
            try
            {
                int x = int.MaxValue;
                int y = int.MaxValue;
                int width = 0;
                int height = 0;
                foreach (int ss in new int[] { 60, 120, 240, 360 })  // check at multiple times
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
                    return String.Empty;
                }
                if(x == 0 && y == 0)
                {
                    // nothing to do
                    return String.Empty;
                }

                if (x == int.MaxValue)
                    x = 0;
                if (y == int.MaxValue)
                    y = 0;

                if (CroppingThreshold < 0)
                    CroppingThreshold = 0;

                args.Logger?.DLog($"Video dimensions: {vidWidth}x{vidHeight}");

                var willCrop = TestAboveThreshold(vidWidth, vidHeight, width, height, CroppingThreshold); 
                args.Logger?.ILog($"Crop detection, x:{x}, y:{y}, width: {width}, height: {height}, total:{willCrop.diff}, threshold:{CroppingThreshold}, above threshold: {willCrop}");

                return willCrop.crop ? $"{width}:{height}:{x}:{y}" : string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static (bool crop, int diff) TestAboveThreshold(int vidWidth, int vidHeight, int detectedWidth, int detectedHeight, int threshold)
        {
            int diff = (vidWidth - detectedWidth) + (vidHeight - detectedHeight);
            return (diff > threshold, diff);

        }
    }
}