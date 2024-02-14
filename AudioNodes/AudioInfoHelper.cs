using System.Diagnostics;
//using System.IO;
using System.Text.RegularExpressions;
using TagLib.Matroska;

namespace FileFlows.AudioNodes;

public class AudioInfoHelper
{
    private string ffMpegExe;
    private string ffprobe;
    private ILogger Logger;
    
    public AudioInfoHelper(string ffMpegExe, string ffprobe, ILogger logger)
    {
        this.ffMpegExe = ffMpegExe;
        this.ffprobe = ffprobe;
        this.Logger = logger;
    }

    public Result<AudioInfo> Read(string filename)
    {
        
        if (System.IO.File.Exists(filename) == false)
            return Result<AudioInfo>.Fail("File not found: " + filename);
        if (string.IsNullOrEmpty(ffMpegExe) || System.IO.File.Exists(ffMpegExe) == false)
            return Result<AudioInfo>.Fail("FFmpeg not found: " + (ffMpegExe ?? "not passed in"));

        var mi = new AudioInfo();
        var result = ReadFromFFprobe(filename);
        if (result.IsFailed == false)
            mi = result.Value;
        else
            mi = ReadMetaData(filename);

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
                    return Result<AudioInfo>.Fail("Failed reading ffmpeg info: " + error);

                Logger.ILog("Audio Information:" + Environment.NewLine + output);

                if(output.IndexOf("Input #0", StringComparison.Ordinal) < 0)
                    return Result<AudioInfo>.Fail("Failed to read audio information for file");

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
                    int colonIndex = line.IndexOf(":", StringComparison.Ordinal);
                    if(colonIndex < 1)
                        continue;

                    string lowLine = line.ToLower().Trim();

                    if (lowLine.StartsWith("language"))
                        mi.Language = line[(colonIndex + 1)..].Trim();
                    else if (lowLine.StartsWith("track") && lowLine.Contains("total") == false)
                    {
                        if (mi.Track < 1)
                        {
                            var trackMatch = Regex.Match(line.Substring(colonIndex + 1).Trim(), @"^[\d]+");
                            if (trackMatch.Success && int.TryParse(trackMatch.Value, out int value))
                                mi.Track = value;
                        }
                    }
                    else if (lowLine.StartsWith("artist") || lowLine.StartsWith("album_artist"))
                    {
                        if (string.IsNullOrWhiteSpace(mi.Artist))
                            mi.Artist = line[(colonIndex + 1)..].Trim();
                    }
                    else if (lowLine.StartsWith("title") && lowLine.Contains(".jpg") == false)
                    {
                        if (string.IsNullOrWhiteSpace(mi.Title))
                            mi.Title = line[(colonIndex + 1)..].Trim();
                    }
                    else if (lowLine.StartsWith("album"))
                    {
                        if (string.IsNullOrWhiteSpace(mi.Album))
                            mi.Album = line[(colonIndex + 1)..].Trim();
                    }
                    else if (lowLine.StartsWith("disc"))
                    {
                        if (int.TryParse(line[(colonIndex + 1)..].Trim(), out int value))
                            mi.Disc = value;
                    }
                    else if (lowLine.StartsWith("disctotal") || lowLine.StartsWith("totaldiscs"))
                    {
                        if (int.TryParse(line[(colonIndex + 1)..].Trim(), out int value))
                            mi.TotalDiscs = value;
                    }
                    else if (lowLine.StartsWith("date") || lowLine.StartsWith("retail date") || lowLine.StartsWith("retaildate") || lowLine.StartsWith("originaldate") || lowLine.StartsWith("original date"))
                    {
                        if (int.TryParse(line[(colonIndex + 1)..].Trim(), out int value))
                        {
                            if(mi.Date < new DateTime(1900, 1, 1))
                                mi.Date = new DateTime(value, 1, 1);
                        }
                        else if(DateTime.TryParse(line[(colonIndex + 1)..].Trim(), out DateTime dtValue) && dtValue.Year > 1900)
                            mi.Date = dtValue; 
                    }
                    else if (lowLine.StartsWith("genre"))
                    {
                        if(mi.Genres?.Any() != true)
                            mi.Genres = line[(colonIndex + 1)..].Trim().Split(' ');
                    }
                    else if (lowLine.StartsWith("encoder"))
                        mi.Encoder = line[(colonIndex + 1)..].Trim();
                    else if (lowLine.StartsWith("duration"))
                    {
                        if (mi.Duration < 1)
                        {
                            string temp = line[(colonIndex + 1)..].Trim();
                            if (temp.IndexOf(",", StringComparison.Ordinal) > 0)
                            {
                                temp = temp.Substring(0, temp.IndexOf(","));
                                if (TimeSpan.TryParse(temp, out TimeSpan value))
                                    mi.Duration = (long)value.TotalSeconds;
                            }
                        }
                    }


                    if (line.ToLower().IndexOf("bitrate:", StringComparison.Ordinal) > 0)
                    {
                        string br = line.Substring(line.ToLower().IndexOf("bitrate:", StringComparison.Ordinal) + "bitrate:".Length).Trim();
                        if (br.IndexOf(" ", StringComparison.Ordinal) > 0)
                        {
                            int multiplier = br.ToLower().IndexOf("kb/s", StringComparison.Ordinal) > 0 ? 1024 : 
                                             br.ToLower().IndexOf("mb/s", StringComparison.Ordinal) > 0 ? 1024 * 1024 : 
                                             1; 
                            br = br.Substring(0, br.IndexOf(" "));
                            if (long.TryParse(br, out long value))
                                mi.Bitrate = value * multiplier;
                        }
                    }

                    var match = Regex.Match(line, @"([\d]+) Hz");
                    if (match.Success)
                    {
                        mi.Frequency = int.Parse(match.Groups[1].Value);
                    }

                    if (line.IndexOf(" stereo,", StringComparison.Ordinal) > 0)
                        mi.Channels = 2;
                }

            }
        }
        catch (Exception ex)
        {
            Logger.ELog(ex.Message, ex.StackTrace.ToString());
            return Result<AudioInfo>.Fail("Failed reading audio information: " + ex.Message);
        }

        if (string.IsNullOrEmpty(mi.Artist) || string.IsNullOrEmpty(mi.Album) || mi.Track < 1 || string.IsNullOrEmpty(mi.Title))
        {
            // try parse the file
            ParseFileNameInfo(filename, mi);
        }

        return mi;
    }

    public Result<AudioInfo> ReadFromFFprobe(string file)
    {
        try
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo();
                process.StartInfo.FileName = ffprobe;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.ArgumentList.Add("-v");
                process.StartInfo.ArgumentList.Add("error");
                process.StartInfo.ArgumentList.Add("-select_streams");
                process.StartInfo.ArgumentList.Add("a:0");
                process.StartInfo.ArgumentList.Add("-show_format");
                process.StartInfo.ArgumentList.Add("-of");
                process.StartInfo.ArgumentList.Add("json");
                process.StartInfo.ArgumentList.Add(file);
                process.Start();

                string output = process.StandardError.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (string.IsNullOrEmpty(error) == false && error != "At least one output file must be specified")
                {
                    Logger.ELog("Failed reading ffmpeg info: " + error);
                    return Result<AudioInfo>.Fail("Failed reading ffmpeg info: " + error);
                }

                var result = FFprobeAudioInfo.Parse(output);
                if (result.IsFailed)
                    return Result<AudioInfo>.Fail(result.Error);

                var audioInfo = new AudioInfo();
                audioInfo.Album = result.Value.Tags.Album;
                audioInfo.Artist = result.Value.Tags.Artist;
                audioInfo.Bitrate = result.Value.Bitrate;
                audioInfo.Codec = result.Value.FormatName;
                if (DateTime.TryParse(result.Value.Tags.Date ?? string.Empty, out DateTime date))
                    audioInfo.Date = date;
                else if (int.TryParse(result.Value.Tags.Date ?? string.Empty, out int year))
                    audioInfo.Date = new DateTime(year, 1, 1);

                if (int.TryParse(result.Value.Tags.Disc ?? string.Empty, out int disc))
                    audioInfo.Disc = disc;
                audioInfo.Duration = (long)result.Value.Duration.TotalSeconds;
                audioInfo.Genres = result.Value.Tags?.Genre
                    .Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries)?.Select(x => x.Trim())
                    ?.ToArray();
                audioInfo.Title = result.Value.Tags.Title;
                if (int.TryParse(result.Value.Tags.Track, out int track))
                    audioInfo.Track = track;
                if (int.TryParse(result.Value.Tags.TotalDiscs, out int totalDiscs))
                    audioInfo.TotalDiscs = totalDiscs;
                if (int.TryParse(result.Value.Tags.TotalTracks, out int totalTracks))
                    audioInfo.TotalTracks = totalTracks;

                return audioInfo;
            }
        }
        catch (Exception ex)
        {
            return Result<AudioInfo>.Fail(ex.Message);
        }
    }

    public AudioInfo ReadMetaData(string file)
    {
        using var tfile = TagLib.File.Create(file);
        AudioInfo info = new AudioInfo();
        try
        {
            info.Title = tfile.Tag.Title;
            info.Duration = (long)tfile.Properties.Duration.TotalSeconds;
            info.TotalDiscs = Convert.ToInt32(tfile.Tag.DiscCount);
            if (info.TotalDiscs < 1)
                info.TotalDiscs = 1;
            info.Disc = Convert.ToInt32(tfile.Tag.Disc);
            if (info.Disc < 1)
                info.Disc = 1;
            info.Artist = String.Join(", ", tfile.Tag.AlbumArtists);
            info.Album = tfile.Tag.Album;
            info.Track = Convert.ToInt32(tfile.Tag.Track);
            info.TotalTracks = Convert.ToInt32(tfile.Tag.TrackCount);
            if(tfile.Tag.Year > 1900)
            {
                info.Date = new DateTime(Convert.ToInt32(tfile.Tag.Year), 1, 1);
            }
            info.Genres = tfile.Tag.Genres.SelectMany(x => x.Split(new[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())).ToArray();
        }
        catch (Exception) { }
        tfile.Dispose();
        return info;
    }

    public void ParseFileNameInfo(string filename, AudioInfo mi)
    {
        using var tfile = TagLib.File.Create(filename);
        try
        {
            var fileInfo = new System.IO.FileInfo(filename);

            bool dirty = false;

            if (mi.Disc < 1)
            {
                var cdMatch = Regex.Match(filename.Replace("\\", "/"), @"(?<=(/(cd|disc)))[\s]*([\d]+)(?!=(/))", RegexOptions.IgnoreCase);
                if (cdMatch.Success && int.TryParse(cdMatch.Value.Trim(), out int disc))
                {
                    dirty = true;
                    mi.Disc = disc;
                    tfile.Tag.Disc = Convert.ToUInt32(disc);
                }
            }

            if (mi.Track < 1)
            {
                var trackMatch = Regex.Match(fileInfo.Name, @"[\-_\s\.]+([\d]{1,2})[\-_\s\.]+");
                if (trackMatch.Success)
                {
                    string trackString = trackMatch.Value;
                    if (int.TryParse(Regex.Match(trackString, @"[\d]+").Value, out int track))
                    {
                        mi.Track = track;
                        tfile.Tag.Track = Convert.ToUInt32(track);
                        dirty = true;
                    }
                }
            }

            string album = fileInfo.Directory.Name;
            var yearMatch = Regex.Match(album, @"(?<=(\())[\d]{4}(?!=\))");
            if (yearMatch.Success)
            {
                album = album.Replace("(" + yearMatch.Value + ")", "").Trim();

                if (mi.Date < new DateTime(1900, 1, 1))
                {
                    if (int.TryParse(yearMatch.Value, out int year))
                    {
                        mi.Date = new DateTime(year, 1, 1);
                        tfile.Tag.Year = Convert.ToUInt32(year);
                        dirty = true;
                    }
                }
            }


            if (string.IsNullOrEmpty(mi.Album))
            {
                mi.Album = album;
                if (string.IsNullOrEmpty(album) == false)
                {
                    tfile.Tag.Album = mi.Album;
                    dirty = true;
                }
            }

            if (string.IsNullOrEmpty(mi.Artist))
            {
                mi.Artist = fileInfo.Directory.Parent.Name;
                if (string.IsNullOrEmpty(mi.Artist) == false)
                {
                    tfile.Tag.AlbumArtists = new[] { mi.Artist };
                    dirty = true;
                }
            }

            // the title
            if (string.IsNullOrEmpty(mi.Title))
            {
                int titleIndex = fileInfo.Name.LastIndexOf(" - ");
                if (titleIndex > 0)
                {
                    mi.Title = fileInfo.Name.Substring(titleIndex + 3);
                    if (string.IsNullOrEmpty(fileInfo.Extension) == false)
                    {
                        mi.Title = mi.Title.Replace(fileInfo.Extension, "");
                        tfile.Tag.Title = mi.Title;
                        dirty = true;
                    }
                }
            }

            if(dirty)
                tfile.Save();

        }
        catch (Exception ex)
        {
            Logger?.WLog("Failed parsing Audio info from filename: " + ex.Message + Environment.NewLine + ex.StackTrace);
        }
        finally
        {
            tfile.Dispose();
        }
    }

}