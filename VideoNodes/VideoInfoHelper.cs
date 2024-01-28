using System.Diagnostics;
using System.Globalization;

namespace FileFlows.VideoNodes;

public class VideoInfoHelper
{
    private string ffMpegExe;
    private ILogger Logger;

    static Regex rgxTitle = new Regex(@"(?<=((^[\s]+title[\s]+:[\s])))(.*?)$", RegexOptions.Multiline);
    static Regex rgxDuration = new Regex(@"(?<=((^[\s]+DURATION(\-[\w]+)?[\s]+:[\s])))([\d]+:?)+\.[\d]{1,7}", RegexOptions.Multiline);
    static Regex rgxDuration2 = new Regex(@"(?<=((^[\s]+Duration:[\s])))([\d]+:?)+\.[\d]{1,7}", RegexOptions.Multiline);
    static Regex rgxAudioSampleRate = new Regex(@"(?<=((,|\s)))[\d]+(?=([\s]?hz))", RegexOptions.IgnoreCase);
    static Regex rgxAudioBitrate = new Regex(@"(?<=(, ))([\d]+)(?=( kb\/s))", RegexOptions.IgnoreCase);
    static Regex rgxAudioBitrateFull = new Regex(@"^[\s]+BPS[^:]+: ([\d]+)", RegexOptions.Multiline);
    static Regex rgxFilename = new Regex(@"(?<=((^[\s]+filename[\s]+:[\s])))(.*?)$", RegexOptions.Multiline);
    static Regex rgxMimeType = new Regex(@"(?<=((^[\s]+mimetype[\s]+:[\s])))(.*?)$", RegexOptions.Multiline);

    static int _ProbeSize = 25;
    internal static int ProbeSize 
    { 
        get => _ProbeSize;
        set
        {
            if (value < 5)
                _ProbeSize = 5;
            else if (value > 1000)
                _ProbeSize = 1000;
            else
                _ProbeSize = value;
        }
    }

    public VideoInfoHelper(string ffMpegExe, ILogger logger)
    {
        this.ffMpegExe = ffMpegExe;
        this.Logger = logger;
    }

    public static string GetFFMpegPath(NodeParameters args) => args.GetToolPath("FFMpeg");

    public VideoInfo Read(string filename)
        => ReadStatic(Logger, ffMpegExe, filename);
    
    internal static VideoInfo ReadStatic(ILogger logger, string ffMpegExe, string filename)
    {
        #if(DEBUG) // UNIT TESTING
        filename = filename.Replace("~/", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/");
        #endif
        
        var vi = new VideoInfo();
        vi.FileName = filename;
        if (System.IO.File.Exists(filename) == false)
        {
            logger.ELog("File not found: " + filename);
            return vi;
        }
        if (string.IsNullOrEmpty(ffMpegExe) || System.IO.File.Exists(ffMpegExe) == false)
        {
            logger.ELog("FFmpeg not found: " + (ffMpegExe ?? "not passed in"));
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
                foreach (var arg in new[]
                {
                    "-hide_banner",
                    "-probesize", ProbeSize + "M",
                    "-i",
                    filename,
                })
                {
                    process.StartInfo.ArgumentList.Add(arg);
                }
                process.Start();
                string output = process.StandardError.ReadToEnd();
                output = output.Replace("At least one output file must be specified", string.Empty).Trim();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (string.IsNullOrEmpty(error) == false && error != "At least one output file must be specified")
                {
                    logger.ELog("Failed reading ffmpeg info: " + error);
                    return vi;
                }

                logger.ILog("Video Information:" + Environment.NewLine + output);
                vi = ParseOutput(logger, output);
            }
        }
        catch (Exception ex)
        {
            logger.ELog(ex.Message, ex.StackTrace.ToString());
        }
        vi.FileName = filename;
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

        int subtitleIndex = 0;
        int videoIndex = 0;
        int audioIndex = 0;
        int attachmentIndex = 0;
        foreach (Match sm in streamMatches)
        {
            if (sm.Value.Contains(" Video: "))
            {
                var vs = ParseVideoStream(logger, sm.Value, output);
                if (vs != null)
                {
                    vs.Index = streamIndex;
                    vs.TypeIndex = videoIndex;
                    var match = Regex.Match(sm.Value, @"(?<=(Stream #))[\d]+:[\d]+");
                    if (match.Success)
                        vs.IndexString = match.Value;
                    vi.VideoStreams.Add(vs);
                }
                ++videoIndex;
            }
            else if (sm.Value.Contains(" Audio: "))
            {
                var audio = ParseAudioStream(logger, sm.Value);
                if (audio != null)
                {
                    audio.TypeIndex = audioIndex;
                    audio.Index = streamIndex;
                    var match = Regex.Match(sm.Value, @"(?<=(Stream #))[\d]+:[\d]+");
                    if (match.Success)
                        audio.IndexString = match.Value;
                    vi.AudioStreams.Add(audio);
                }
                ++audioIndex;
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
            else if (sm.Value.Contains(" Attachment: "))
            {
                AttachmentStream attachmentStream = ParseAttachmentStream(sm.Value);
                if (attachmentStream != null)
                {
                    attachmentStream.Index = streamIndex;
                    attachmentStream.TypeIndex = attachmentIndex;
                    var match = Regex.Match(sm.Value, @"(?<=(Stream #))[\d]+:[\d]+");
                    if (match.Success)
                        attachmentStream.IndexString = match.Value;
                    vi.Attachments.Add(attachmentStream);
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
        vs.IsImage = info.Contains("(attached pic)");

        var matchCodecTag = new Regex(@": Video: [^(,]+\([^)]+\)[^(,]+\(([^)]+)\)").Match(line);
        if (matchCodecTag.Success)
        {
            vs.CodecTag = matchCodecTag.Groups[1].Value;
            if (vs.CodecTag.IndexOf(" /") > 0)
                vs.CodecTag = vs.CodecTag.Substring(0, vs.CodecTag.IndexOf(" /")).Trim();
        }

        vs.Codec = line.Substring(line.IndexOf("Video: ") + "Video: ".Length).Replace(",", "").Trim().Split(' ').First().ToLower();
        vs.PixelFormat = GetDecoderPixelFormat(line);
        vs.Is10Bit = Regex.IsMatch(line, @"p(0)?10l(b)?e", RegexOptions.IgnoreCase);
        vs.Is12Bit = Regex.IsMatch(line, @"p(0)?12l(b)?e", RegexOptions.IgnoreCase);

        var dimensions = Regex.Match(line, @"([\d]{3,})x([\d]{3,})");
        if (int.TryParse(dimensions.Groups[1].Value, out int width))
            vs.Width = width;
        if (int.TryParse(dimensions.Groups[2].Value, out int height))
            vs.Height = height;

        if (Regex.IsMatch(line, @"([\d]+(\.[\d]+)?)\sfps") &&
            float.TryParse(Regex.Match(line, @"([\d]+(\.[\d]+)?)\sfps").Groups[1].Value, out float fps))
        {
            logger?.ILog("Frames Per Second: " + fps);
            vs.FramesPerSecond = fps;
        }

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

        // As per https://video.stackexchange.com/a/33827
        // "HDR is only the new transfer function" (PQ or HLG)
        vs.HDR = info.Contains("arib-std-b67") || info.Contains("smpte2084");

        vs.DolbyVision = info.Contains("DOVI configuration record");
        
        return vs;
    }

    public static AudioStream ParseAudioStream(ILogger logger, string info)
    {
        // Stream #0:1(eng): Audio: dts (DTS), 48000 Hz, stereo, fltp, 1536 kb/s (default)
        string line = info.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).First();
        var parts = line.Split(",").Select(x => x?.Trim() ?? "").ToArray();
        AudioStream audio = new AudioStream();
        audio.Title = "";
        // this isnt type index, this is overall index
        audio.TypeIndex = int.Parse(Regex.Match(line, @"#([\d]+):([\d]+)").Groups[2].Value) - 1;
        audio.Codec = parts[0].Substring(parts[0].IndexOf("Audio: ") + "Audio: ".Length).Trim().Split(' ').First().ToLower() ?? string.Empty;
        if (audio.Codec.EndsWith(","))
            audio.Codec = audio.Codec[..^1].Trim();

        audio.Language = GetLanguage(line);
        // if (info.IndexOf("0 channels", StringComparison.Ordinal) >= 0)
        // {
        //     logger?.WLog("Stream contained '0 Channels'");
        //     audio.Channels = 0;
        // }
        // else
        {
            try
            {
                //Logger.ILog("codec: " + vs.Codec);
                if (parts[2] == "stereo")
                    audio.Channels = 2;
                else if (parts[2] == "mono")
                    audio.Channels = 1;
                else if (Regex.IsMatch(parts[2], @"^[\d]+(\.[\d]+)?"))
                {
                    var matchValue = Regex.Match(parts[2], @"^[\d]+(\.[\d]+)?").Value;
                    
                    CultureInfo culture = CultureInfo.InvariantCulture; // Use invariant culture for consistent parsing
                    if (float.TryParse(matchValue, NumberStyles.Float, culture, out float channels))
                    {
                        audio.Channels = channels;
                        logger?.ILog("Detected audio channels: " + audio.Channels + ", from " + parts[2]);
                    }
                    else
                    {
                        logger?.WLog($"Failed to parse value '{matchValue}' as a float.");
                    }
                }
                else if (line.Contains(" 7.1"))
                    audio.Channels = 7.1f;
                else if (line.Contains(" 7.2"))
                    audio.Channels = 7.2f;
                else if (line.Contains(" 5.1"))
                    audio.Channels = 5.1f;
                else if (line.Contains(" 5.0"))
                    audio.Channels = 5f;
                else if (line.Contains(" 4.1"))
                    audio.Channels = 4.1f;
                else
                {
                    logger?.WLog("Unable to detect channels from: " + line);
                }

                logger?.ILog("Audio channels: " + audio.Channels + ", from " + parts[2]);
            }
            catch (Exception ex)
            {
                logger?.WLog("Failed to parse audio channels: " + ex.Message + "\n" + "From line: " + line);
            }
        }

        var match = rgxAudioSampleRate.Match(info);
        if (match.Success)
            audio.SampleRate = int.Parse(match.Value);

        if (rgxTitle.IsMatch(info))
            audio.Title = rgxTitle.Match(info).Value.Trim();


        if (rgxDuration.IsMatch(info))
            audio.Duration = TimeSpan.Parse(rgxDuration.Match(info).Value);

        try
        {
            if (rgxAudioBitrate.IsMatch(line))
            {
                audio.Bitrate = int.Parse(rgxAudioBitrate.Match(line).Value) * 1000; // this is NOT 1024
            }
            else if (rgxAudioBitrateFull.IsMatch(info))
            {
                audio.Bitrate = float.Parse(rgxAudioBitrateFull.Match(info).Groups[1].Value);
            }
        }
        catch(Exception) { }   


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
        if (sub.Codec.EndsWith(","))
            sub.Codec = sub.Codec[..^1].Trim();
        sub.Language = GetLanguage(line);
        sub.Default = info.Contains("(default)");

        if (rgxTitle.IsMatch(info))
            sub.Title = rgxTitle.Match(info).Value.Trim();

        sub.Forced = info.ToLower().Contains("forced");
        return sub;
    }
    
    private static AttachmentStream ParseAttachmentStream(string info)
    {
        // Stream #0:6: Attachment: ttf
        //   Metadata:
        //     filename        : FF Tisa OT XBold.ttf
        //     mimetype        : application/x-truetype-font
        string line = info.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).First();
        var parts = line.Split(",").Select(x => x?.Trim() ?? "").ToArray();
        AttachmentStream stream = new AttachmentStream();
        stream.TypeIndex = int.Parse(Regex.Match(line, @"#([\d]+):([\d]+)").Groups[2].Value);
        stream.Codec = line.Substring(line.IndexOf("Attachment: ") + "Attachment: ".Length).Trim().Split(' ').First().ToLower();
        if (stream.Codec.EndsWith(","))
            stream.Codec = stream.Codec[..^1].Trim();

        if (rgxFilename.IsMatch(info))
            stream.FileName = rgxFilename.Match(info).Value.Trim();
        if (rgxMimeType.IsMatch(info))
            stream.MimeType = rgxMimeType.Match(info).Value.Trim();
        return stream;
    }

    private static string GetLanguage(string line)
    {
        var langSection = Regex.Match(line, @"(?<=(Stream\s#[\d]+:[\d]+))[^:]+");
        if (langSection.Success == false)
            return string.Empty;
        var lang = Regex.Match(langSection.Value, @"(?<=\()[^)]+").Value?.ToLower() ?? string.Empty;
        if (lang == "und")
            return string.Empty;
        return lang;
    }
    
    /// <summary>
    /// Extracts the supported pixel format from an FFmpeg output line for hardware decoding.
    /// </summary>
    /// <param name="line">The FFmpeg output line containing video stream information.</param>
    /// <returns>The supported pixel format (e.g., "yuv420p" or "p010le"), or an empty string if not found or not supported.</returns>
    /// <remarks>
    /// Supports "yuv420p" and "p010le" formats. Handles cases where "p010le" has no additional specifiers, defaulting to "yuv420p".
    /// Adjust the regular expression or default behavior based on specific hardware and FFmpeg output format.
    /// </remarks>
    static string GetDecoderPixelFormat(string line)
    {
        // only p010le confirmed working so far
        if(Regex.IsMatch(line, @"p(0)?10l(b)?e"))
           return "p010le";
        if(line.IndexOf("yuv420p", StringComparison.Ordinal) > 0)
            return "yuv420p";
        // if (line.IndexOf("nv12", StringComparison.Ordinal) >= 0)
        //     return "nv12";
        // if (line.IndexOf("yuv444p", StringComparison.Ordinal) >= 0)
        //     return "yuv444p";
        return string.Empty;
    }
}