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

                        string lowLine = line.ToLower().Trim();

                        if (lowLine.StartsWith("language"))
                            mi.Language = line.Substring(colonIndex + 1).Trim();
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
                                mi.Artist = line.Substring(colonIndex + 1).Trim();
                        }
                        else if (lowLine.StartsWith("title") && lowLine.Contains(".jpg") == false)
                        {
                            if (string.IsNullOrWhiteSpace(mi.Title))
                                mi.Title = line.Substring(colonIndex + 1).Trim();
                        }
                        else if (lowLine.StartsWith("album"))
                        {
                            if (string.IsNullOrWhiteSpace(mi.Album))
                                mi.Album = line.Substring(colonIndex + 1).Trim();
                        }
                        else if (lowLine.StartsWith("disc"))
                        {
                            if (int.TryParse(line.Substring(colonIndex + 1).Trim(), out int value))
                                mi.Disc = value;
                        }
                        else if (lowLine.StartsWith("disctotal") || lowLine.StartsWith("totaldiscs"))
                        {
                            if (int.TryParse(line.Substring(colonIndex + 1).Trim(), out int value))
                                mi.TotalDiscs = value;
                        }
                        else if (lowLine.StartsWith("date") || lowLine.StartsWith("retail date") || lowLine.StartsWith("retaildate") || lowLine.StartsWith("originaldate") || lowLine.StartsWith("original date"))
                        {
                            if (int.TryParse(line.Substring(colonIndex + 1).Trim(), out int value))
                            {
                                if(mi.Date < new DateTime(1900, 1, 1))
                                    mi.Date = new DateTime(value, 1, 1);
                            }
                            else if(DateTime.TryParse(line.Substring(colonIndex + 1).Trim(), out DateTime dtValue) && dtValue.Year > 1900)
                                mi.Date = dtValue; 
                        }
                        else if (lowLine.StartsWith("genre"))
                        {
                            if(mi.Genres?.Any() != true)
                                mi.Genres = line.Substring(colonIndex + 1).Trim().Split(' ');
                        }
                        else if (lowLine.StartsWith("encoder"))
                            mi.Encoder = line.Substring(colonIndex + 1).Trim();
                        else if (lowLine.StartsWith("duration"))
                        {
                            if (mi.Duration < 1)
                            {
                                string temp = line.Substring(colonIndex + 1).Trim();
                                if (temp.IndexOf(",") > 0)
                                {
                                    temp = temp.Substring(0, temp.IndexOf(","));
                                    if (TimeSpan.TryParse(temp, out TimeSpan value))
                                        mi.Duration = (long)value.TotalSeconds;
                                }
                            }
                        }


                        if (line.ToLower().IndexOf("bitrate:") > 0)
                        {
                            string br = line.Substring(line.ToLower().IndexOf("bitrate:") + "bitrate:".Length).Trim();
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

            if (string.IsNullOrEmpty(mi.Artist) && string.IsNullOrEmpty(mi.Album) && mi.Track < 1)
            {
                // try parse the file
                ParseFileNameInfo(filename, mi);
            }

            return mi;
        }

        public MusicInfo ReadMetaData(string file)
        {
            using var tfile = TagLib.File.Create(file);
            MusicInfo info = new MusicInfo();
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
                if(tfile.Tag.Year > 1900)
                {
                    info.Date = new DateTime(Convert.ToInt32(tfile.Tag.Year), 1, 1);
                }
                info.Genres = tfile.Tag.Genres.SelectMany(x => x.Split(new[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())).ToArray();
            }
            catch (Exception) { }
            return info;
        }

        public void ParseFileNameInfo(string filename, MusicInfo mi)
        {
            try
            {
                var fileInfo = new FileInfo(filename);

                using var tfile = TagLib.File.Create(filename);

                var cdMatch = Regex.Match(filename.Replace("\\", "/"), @"(?<=(/(cd|disc)))[\s]*([\d]+)(?!=(/))", RegexOptions.IgnoreCase);
                if (cdMatch.Success && int.TryParse(cdMatch.Value.Trim(), out int disc))
                {
                    mi.Disc = disc;
                    tfile.Tag.Disc = Convert.ToUInt32(disc);
                }

                var trackMatch = Regex.Match(fileInfo.Name, @"[\-_\s\.]+([\d]{1,2})[\-_\s\.]+");
                if (trackMatch.Success) 
                {
                    string trackString = trackMatch.Value;
                    if (int.TryParse(Regex.Match(trackString, @"[\d]+").Value, out int track))
                    {
                        mi.Track = track;
                        tfile.Tag.Track = Convert.ToUInt32(track);
                    }
                }

                string album = fileInfo.Directory.Name;
                var yearMatch = Regex.Match(album, @"(?<=(\())[\d]{4}(?!=\))");
                if (yearMatch.Success)
                {
                    album = album.Replace("(" + yearMatch.Value + ")", "").Trim();
                    if (int.TryParse(yearMatch.Value, out int year))
                    {
                        mi.Date = new DateTime(year, 1, 1);
                        tfile.Tag.Year = Convert.ToUInt32(year);
                    }
                }

                mi.Album = album;
                mi.Artist = fileInfo.Directory.Parent.Name;

                // the title
                int titleIndex = fileInfo.Name.LastIndexOf(" - ");
                if (titleIndex > 0)
                {
                    mi.Title = fileInfo.Name.Substring(titleIndex + 3);
                    if (string.IsNullOrEmpty(fileInfo.Extension) == false)
                    {
                        mi.Title = mi.Title.Replace(fileInfo.Extension, "");
                        tfile.Tag.Title = mi.Title;
                    }
                }
                if(string.IsNullOrEmpty(mi.Artist) == false)
                    tfile.Tag.AlbumArtists = new[] { mi.Artist };
                if(string.IsNullOrEmpty(mi.Album) == false)
                    tfile.Tag.Album = mi.Album;

                tfile.Save();

            }
            catch (Exception ex)
            {
                Logger?.WLog("Failed parsing music info from filename: " + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

    }
}