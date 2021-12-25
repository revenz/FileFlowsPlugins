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
            string ffplay = GetFFMpegExe(args);
            if (string.IsNullOrEmpty(ffplay))
                return -1;

            string crop = Execute(ffplay, args.WorkingFile, args.TempPath, args);
            if (crop == string.Empty)
                return 2;

            args.Logger?.ILog("Black bars detected, crop: " + crop);
            args.Parameters.Add(CROP_KEY, crop);

            return 1;
        }

        public string Execute(string ffplay, string file, string tempDir, NodeParameters args)
        {
            try
            {
                int x = int.MaxValue;
                int y = int.MaxValue;
                int width = 0;
                int height = 0;
                int vidWidth = 0;
                int vidHeight = 0;
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
                        process.StartInfo.Arguments = $" -ss {ss} -i \"{file}\" -hide_banner -vframes 25 -vf cropdetect=24:16:0 -f null -";
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

                        if(vidWidth == 0)
                            vidWidth = int.Parse(dimMatch.Groups[3].Value);
                        if(vidHeight == 0)
                            vidHeight = int.Parse(dimMatch.Groups[4].Value);

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

                int diff = x + y + (vidWidth - width) + (vidHeight - height);

                bool willCrop = diff > CroppingThreshold;
                args.Logger?.ILog($"Crop detection, x:{x}, y:{y}, width: {width}, height: {height}, total:{diff}, threshold:{CroppingThreshold}, above threshold: {willCrop}");

                return willCrop ? $"{width}:{height}:{x}:{y}" : string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}