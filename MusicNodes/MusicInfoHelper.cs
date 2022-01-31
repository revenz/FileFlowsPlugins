namespace FileFlows.MusicNodes
{
    using System.Diagnostics;
    using System.IO;
    using System.Text.RegularExpressions;
    using FileFlows.Plugin;

    public class MusicInfoHelper
    {
        private string ffMpegExe;
        private ILogger Logger;
        
        public MusicInfoHelper(string ffMpegExe, ILogger logger)
        {
            this.ffMpegExe = ffMpegExe;
            this.Logger = logger;
        }

        public static string GetFFMpegPath(NodeParameters args) => args.GetToolPath("FFMpeg");
        public MusicInfo Read(string filename)
        {
            var mi = new MusicInfo();
            if (File.Exists(filename) == false)
            {
                Logger.ELog("File not found: " + filename);
                return mi;
            }
            if (string.IsNullOrEmpty(ffMpegExe) || File.Exists(ffMpegExe) == false)
            {
                Logger.ELog("FFMpeg not found: " + (ffMpegExe ?? "not passed in"));
                return mi;
            }

            try
            {
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo();
                    process.StartInfo.FileName = ffMpegExe;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.Arguments = $"-hide_banner -i \"{filename}\"";
                    process.Start();
                    string output = process.StandardError.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (string.IsNullOrEmpty(error) == false && error != "At least one output file must be specified")
                    {
                        Logger.ELog("Failed reading ffmpeg info: " + error);
                        return mi;
                    }

                    Logger.ILog("Music Information:" + Environment.NewLine + output);

                    if(output.IndexOf("Input #0") < 0)
                    {
                        Logger.ELog("Failed to read audio information for file");
                        return mi;
                    }

                    if (output.ToLower().Contains("mp3"))
                        mi.Codec = "mp3";
                    else if (output.ToLower().Contains("ogg"))
                        mi.Codec = "ogg";
                    else if (output.ToLower().Contains("flac"))
                        mi.Codec = "flac";
                    else if (output.ToLower().Contains("wav"))
                        mi.Codec = "wav";
                    else if (filename.ToLower().EndsWith(".mp3"))
                        mi.Codec = "mp3";
                    else if (filename.ToLower().EndsWith(".ogg"))
                        mi.Codec = "ogg";
                    else if (filename.ToLower().EndsWith(".flac"))
                        mi.Codec = "flac";
                    else if (filename.ToLower().EndsWith(".wav"))
                        mi.Codec = "wav";

                    foreach (string line in output.Split('\n'))
                    {
                        int colonIndex = line.IndexOf(":");
                        if(colonIndex < 1)
                            continue;
                        if(line.Trim().ToLower().StartsWith("language"))
                            mi.Language = line.Substring(colonIndex + 1).Trim();
                        else if (line.Trim().ToLower().StartsWith("track"))
                        {
                            if (int.TryParse(line.Substring(colonIndex + 1).Trim(), out int value))
                                mi.Track = value;
                        }
                        else if (line.Trim().ToLower().StartsWith("artist"))
                            mi.Artist = line.Substring(colonIndex + 1).Trim();
                        else if (line.Trim().ToLower().StartsWith("title"))
                            mi.Title = line.Substring(colonIndex + 1).Trim();
                        else if (line.Trim().ToLower().StartsWith("album"))
                            mi.Album = line.Substring(colonIndex + 1).Trim();
                        else if (line.Trim().ToLower().StartsWith("disc"))
                        {
                            if (int.TryParse(line.Substring(colonIndex + 1).Trim(), out int value))
                                mi.Disc = value;
                        }
                        else if (line.Trim().ToLower().StartsWith("disctotal") || line.Trim().ToLower().StartsWith("totaldiscs"))
                        {
                            if (int.TryParse(line.Substring(colonIndex + 1).Trim(), out int value))
                                mi.TotalDiscs = value;
                        }
                        else if (line.Trim().ToLower().StartsWith("date") && mi.Date < new DateTime(1900, 1, 1))
                        {
                            if (int.TryParse(line.Substring(colonIndex + 1).Trim(), out int value))
                                mi.Date = new DateTime(value, 1, 1);
                        }
                        else if (line.Trim().ToLower().StartsWith("retail date"))
                        {
                            if (DateTime.TryParse(line.Substring(colonIndex + 1).Trim(), out DateTime value))
                                mi.Date = value;
                        }
                        else if (line.Trim().ToLower().StartsWith("genre"))
                            mi.Genres = line.Substring(colonIndex + 1).Trim().Split(' ');
                        else if (line.Trim().ToLower().StartsWith("encoder"))
                            mi.Encoder = line.Substring(colonIndex + 1).Trim();
                        else if (line.Trim().ToLower().StartsWith("duration"))
                        {
                            string temp = line.Substring(colonIndex + 1).Trim();
                            if(temp.IndexOf(",") > 0)
                            {
                                temp = temp.Substring(0, temp.IndexOf(","));
                                if (TimeSpan.TryParse(temp, out TimeSpan value))
                                    mi.Duration = (long)value.TotalSeconds;
                            }
                        }


                        if (line.IndexOf("bitrate:") > 0)
                        {
                            string br = line.Substring(line.IndexOf("bitrate:") + "bitrate:".Length).Trim();
                            if (br.IndexOf(" ") > 0)
                            {
                                br = br.Substring(0, br.IndexOf(" "));
                                if (long.TryParse(br, out long value))
                                    mi.BitRate = value;
                            }
                        }

                        var match = Regex.Match(line, @"([\d]+) Hz");
                        if (match.Success)
                        {
                            mi.Frequency = int.Parse(match.Groups[1].Value);
                        }

                        if (line.IndexOf(" stereo,") > 0)
                            mi.Channels = 2;

                    }

                }
            }
            catch (Exception ex)
            {
                Logger.ELog(ex.Message, ex.StackTrace.ToString());
            }

            return mi;
        }

    }
}