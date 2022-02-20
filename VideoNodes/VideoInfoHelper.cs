namespace FileFlows.VideoNodes
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text.RegularExpressions;
    using FileFlows.Plugin;

    public class VideoInfoHelper
    {
        private string ffMpegExe;
        private ILogger Logger;

        static Regex rgxTitle = new Regex(@"(?<=((^[\s]+title[\s]+:[\s])))(.*?)$", RegexOptions.Multiline);
        static Regex rgxDuration = new Regex(@"(?<=((^[\s]+DURATION(\-[\w]+)?[\s]+:[\s])))([\d]+:?)+\.[\d]{1,7}", RegexOptions.Multiline);
        static Regex rgxDuration2 = new Regex(@"(?<=((^[\s]+Duration:[\s])))([\d]+:?)+\.[\d]{1,7}", RegexOptions.Multiline);
        static Regex rgxAudioSampleRate = new Regex(@"(?<=((,|\s)))[\d]+(?=([\s]?hz))", RegexOptions.IgnoreCase);

        public VideoInfoHelper(string ffMpegExe, ILogger logger)
        {
            this.ffMpegExe = ffMpegExe;
            this.Logger = logger;
        }

        public static string GetFFMpegPath(NodeParameters args) => args.GetToolPath("FFMpeg");
        public VideoInfo Read(string filename)
        {
            var vi = new VideoInfo();
            if (File.Exists(filename) == false)
            {
                Logger.ELog("File not found: " + filename);
                return vi;
            }
            if (string.IsNullOrEmpty(ffMpegExe) || File.Exists(ffMpegExe) == false)
            {
                Logger.ELog("FFMpeg not found: " + (ffMpegExe ?? "not passed in"));
                return vi;
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
                    process.StartInfo.ArgumentList.Add("-hide_banner");
                    process.StartInfo.ArgumentList.Add("-i");
                    process.StartInfo.ArgumentList.Add(filename);
                    process.Start();
                    string output = process.StandardError.ReadToEnd();
                    output = output.Replace("At least one output file must be specified", string.Empty).Trim();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (string.IsNullOrEmpty(error) == false && error != "At least one output file must be specified")
                    {
                        Logger.ELog("Failed reading ffmpeg info: " + error);
                        return vi;
                    }

                    Logger.ILog("Video Information:" + Environment.NewLine + output);
                    vi = ParseOutput(Logger, output);
                }
            }
            catch (Exception ex)
            {
                Logger.ELog(ex.Message, ex.StackTrace.ToString());
            }

            return vi;
        }

        public static VideoInfo ParseOutput(ILogger logger, string output)
        {
            var vi = new VideoInfo();
            var rgxStreams = new Regex(@"Stream\s#[\d]+:[\d]+(.*?)(?=(Stream\s#[\d]|$))", RegexOptions.Singleline);
            var streamMatches = rgxStreams.Matches(output);
            int streamIndex = 0;


            // get a rough estimate, bitrate: 346 kb/s
            var rgxBitrate = new Regex(@"(?<=(bitrate: ))[\d\.]+(?!=( kb/s))");
            var brMatch = rgxBitrate.Match(output);
            if (brMatch.Success)
            {
                vi.Bitrate = float.Parse(brMatch.Value) * 1_000; // to convert to b/s
            }

            vi.Chapters = ParseChapters(output);

            int subtitleIndex = 1;
            foreach (Match sm in streamMatches)
            {
                if (sm.Value.Contains(" Video: "))
                {
                    var vs = ParseVideoStream(logger, sm.Value, output);
                    if (vs != null)
                    {
                        vs.Index = streamIndex;
                        var match = Regex.Match(sm.Value, @"(?<=(Stream #))[\d]+:[\d]+");
                        if (match.Success)
                            vs.IndexString = match.Value;
                        vi.VideoStreams.Add(vs);
                    }
                }
                else if (sm.Value.Contains(" Audio: "))
                {
                    var audio = ParseAudioStream(sm.Value);
                    if (audio != null)
                    {
                        audio.Index = streamIndex;
                        var match = Regex.Match(sm.Value, @"(?<=(Stream #))[\d]+:[\d]+");
                        if (match.Success)
                            audio.IndexString = match.Value;
                        vi.AudioStreams.Add(audio);
                    }
                }
                else if (sm.Value.Contains(" Subtitle: "))
                {
                    var sub = ParseSubtitleStream(sm.Value);
                    if (sub != null)
                    {
                        sub.Index = streamIndex;
                        sub.TypeIndex = subtitleIndex;
                        var match = Regex.Match(sm.Value, @"(?<=(Stream #))[\d]+:[\d]+");
                        if (match.Success)
                            sub.IndexString = match.Value;
                        vi.SubtitleStreams.Add(sub);
                    }
                    ++subtitleIndex;
                }
                ++streamIndex;
            }
            return vi;
        }

        private static List<Chapter> ParseChapters(string output)
        {
            try
            {
                var rgxChatpers = new Regex("(?<=(Chapters:))(.*?)(?=(Stream))", RegexOptions.Singleline);
                string strChapters;
                if (rgxChatpers.TryMatch(output, out Match matchChapters))
                    strChapters = matchChapters.Value.Trim();
                else
                    return new List<Chapter>();

                var rgxChapter = new Regex("Chapter #(.*?)(?=(Chapter #|$))", RegexOptions.Singleline);
                var chapters = new List<Chapter>();

                var rgxTitle = new Regex(@"title[\s]*:[\s]*(.*?)$");
                var rgxStart = new Regex(@"(?<=(start[\s]))[\d]+\.[\d]+");
                var rgxEnd = new Regex(@"(?<=(end[\s]))[\d]+\.[\d]+");
                foreach (Match match in rgxChapter.Matches(strChapters))
                {
                    try
                    {
                        Chapter chapter = new Chapter();
                        if (rgxTitle.TryMatch(match.Value.Trim(), out Match title))
                            chapter.Title = title.Groups[1].Value;

                        if (rgxStart.TryMatch(match.Value, out Match start))
                        {
                            double startSeconds = double.Parse(start.Value);
                            chapter.Start = TimeSpan.FromSeconds(startSeconds);
                        }
                        if (rgxEnd.TryMatch(match.Value, out Match end))
                        {
                            double endSeconds = double.Parse(end.Value);
                            chapter.End = TimeSpan.FromSeconds(endSeconds);
                        }

                        if (chapter.Start > TimeSpan.Zero || chapter.End > TimeSpan.Zero)
                        {
                            chapters.Add(chapter);
                        }
                    }
                    catch (Exception ) { }
                }

                return chapters;
            }catch (Exception) { return new List<Chapter>(); }
        }

        public static VideoStream ParseVideoStream(ILogger logger, string info, string fullOutput)
        {
            // Stream #0:0(eng): Video: h264 (High), yuv420p(tv, bt709/unknown/unknown, progressive), 1920x1080 [SAR 1:1 DAR 16:9], 23.98 fps, 23.98 tbr, 1k tbn (default)
            string line = info.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).First();
            VideoStream vs = new VideoStream();
            vs.Codec = line.Substring(line.IndexOf("Video: ") + "Video: ".Length).Replace(",", "").Trim().Split(' ').First().ToLower();
            var dimensions = Regex.Match(line, @"([\d]{3,})x([\d]{3,})");
            if (int.TryParse(dimensions.Groups[1].Value, out int width))
                vs.Width = width;
            if (int.TryParse(dimensions.Groups[2].Value, out int height))
                vs.Height = height;
            if (int.TryParse(Regex.Match(line, @"#([\d]+):([\d]+)").Groups[2].Value, out int typeIndex))
                vs.TypeIndex = typeIndex;

            if (Regex.IsMatch(line, @"([\d]+(\.[\d]+)?)\sfps") && float.TryParse(Regex.Match(line, @"([\d]+(\.[\d]+)?)\sfps").Groups[1].Value, out float fps))
                vs.FramesPerSecond = fps;
            
            var rgxBps = new Regex(@"(?<=((BPS(\-[\w]+)?[\s]*:[\s])))([\d]+)");
            if (rgxBps.IsMatch(info) && float.TryParse(rgxBps.Match(info).Value, out float bps))
                vs.Bitrate = bps;

            if (rgxDuration.IsMatch(info) && TimeSpan.TryParse(rgxDuration.Match(info).Value, out TimeSpan duration) && duration.TotalSeconds > 0)
            {
                vs.Duration = duration;
                logger?.ILog("Video stream duration: " + vs.Duration);
            }
            else if (rgxDuration2.IsMatch(fullOutput) && TimeSpan.TryParse(rgxDuration2.Match(fullOutput).Value, out TimeSpan duration2) && duration2.TotalSeconds > 0)
            {
                vs.Duration = duration2;
                logger?.ILog("Video stream duration: " + vs.Duration);
            }
            else
            {
                logger?.ILog("Failed to read duration for VideoStream: " + info);
            }

            return vs;
        }

        public static AudioStream ParseAudioStream(string info)
        {
            // Stream #0:1(eng): Audio: dts (DTS), 48000 Hz, stereo, fltp, 1536 kb/s (default)
            string line = info.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).First();
            var parts = line.Split(",").Select(x => x?.Trim() ?? "").ToArray();
            AudioStream audio = new AudioStream();
            audio.Title = "";
            audio.TypeIndex = int.Parse(Regex.Match(line, @"#([\d]+):([\d]+)").Groups[2].Value);
            audio.Codec = parts[0].Substring(parts[0].IndexOf("Audio: ") + "Audio: ".Length).Trim().Split(' ').First().ToLower() ?? "";
            audio.Language = Regex.Match(line, @"(?<=(Stream\s#[\d]+:[\d]+)\()[^\)]+").Value?.ToLower() ?? "";
            //Logger.ILog("codec: " + vs.Codec);
            if (parts[2] == "stereo")
                audio.Channels = 2;
            else if (Regex.IsMatch(parts[2], @"^[\d]+(\.[\d]+)?"))
            {
                audio.Channels = float.Parse(Regex.Match(parts[2], @"^[\d]+(\.[\d]+)?").Value);
            }

            var match = rgxAudioSampleRate.Match(info);
            if (match.Success)
                audio.SampleRate = int.Parse(match.Value);

            if (rgxTitle.IsMatch(info))
                audio.Title = rgxTitle.Match(info).Value.Trim();


            if (rgxDuration.IsMatch(info))
                audio.Duration = TimeSpan.Parse(rgxDuration.Match(info).Value);


            return audio;
        }
        public static SubtitleStream ParseSubtitleStream(string info)
        {
            // Stream #0:1(eng): Audio: dts (DTS), 48000 Hz, stereo, fltp, 1536 kb/s (default)
            string line = info.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).First();
            var parts = line.Split(",").Select(x => x?.Trim() ?? "").ToArray();
            SubtitleStream sub = new SubtitleStream();
            sub.TypeIndex = int.Parse(Regex.Match(line, @"#([\d]+):([\d]+)").Groups[2].Value);
            sub.Codec = line.Substring(line.IndexOf("Subtitle: ") + "Subtitle: ".Length).Trim().Split(' ').First().ToLower();
            sub.Language = Regex.Match(line, @"(?<=(Stream\s#[\d]+:[\d]+)\()[^\)]+").Value?.ToLower() ?? "";

            if (rgxTitle.IsMatch(info))
                sub.Title = rgxTitle.Match(info).Value.Trim();

            sub.Forced = info.ToLower().Contains("forced");
            return sub;
        }
    }
}