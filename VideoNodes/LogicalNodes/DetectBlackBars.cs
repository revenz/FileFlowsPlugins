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
            string tempFile = Path.Combine(tempDir, Guid.NewGuid().ToString() + ".mkv");
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo();
                    process.StartInfo.FileName = ffplay;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.Arguments = $"-i \"{file}\" -hide_banner -t 10 -ss 60 -vf cropdetect {tempFile}";
                    args.Logger?.DLog("Exectuing ffmpeg " + process.StartInfo.Arguments);
                    process.Start();
                    string output = process.StandardError.ReadToEnd();
                    Console.WriteLine(output);
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    var dimMatch = Regex.Match(output, @"Stream #[\d]+:[\d]+(.*?)Video:(.*?)([\d]+)x([\d]+)", RegexOptions.Multiline);
                    if (dimMatch.Success == false)
                    {
                        args.Logger?.WLog("Can't find dimensions for video");
                        return String.Empty; // cant find dimensions
                    }

                    int vidWidth = int.Parse(dimMatch.Groups[3].Value);
                    int vidHeight = int.Parse(dimMatch.Groups[4].Value);

                    args.Logger?.DLog($"Video dimensions: {vidWidth}x{vidHeight}");

                    var matches = Regex.Matches(output, @"(?<=(crop=))([\d]+:){3}[\d]+");
                    int x = int.MaxValue;
                    int y = int.MaxValue;
                    int width = 0;
                    int height = 0;
                    foreach (Match match in matches)
                    {
                        int[] parts = match.Value.Split(':').Select(x => int.Parse(x)).ToArray();
                        x = Math.Min(x, parts[2]);
                        y = Math.Min(y, parts[3]);
                        width = Math.Max(width, parts[0]);
                        height = Math.Max(height, parts[1]);
                    }

                    if (x == int.MaxValue)
                        x = 0;
                    if (y == int.MaxValue)
                        y = 0;

                    if(CroppingThreshold < 0)
                        CroppingThreshold = 0;

                    int diff = x + y + (vidWidth - width) + (vidHeight - height);

                    bool willCrop = diff > CroppingThreshold;
                    args.Logger?.ILog($"Crop detection, x:{x}, y:{y}, width: {width}, height: {height}, total:{diff}, threshold:{CroppingThreshold}, above threshold: {willCrop}");

                    return willCrop ? $"{width}:{height}:{x}:{y}" : string.Empty;
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
            finally
            {
                if (System.IO.File.Exists(tempFile))
                {
                    try
                    {
                        System.IO.File.Delete(tempFile);
                    }
                    catch (Exception) { }
                }
            }
        }
    }
}