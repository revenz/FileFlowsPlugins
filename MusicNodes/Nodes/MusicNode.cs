namespace FileFlows.MusicNodes
{
    using FileFlows.Plugin;

    public abstract class MusicNode : Node
    {
        public override bool Obsolete => true;
        public override string Icon => "fas fa-music";

        protected string GetFFMpegExe(NodeParameters args)
        {
            string ffmpeg = args.GetToolPath("FFMpeg");
            if (string.IsNullOrEmpty(ffmpeg))
            {
                args.Logger.ELog("FFMpeg tool not found.");
                return "";
            }
            var fileInfo = new FileInfo(ffmpeg);
            if (fileInfo.Exists == false)
            {
                args.Logger.ELog("FFMpeg tool configured by ffmpeg file does not exist.");
                return "";
            }
            return fileInfo.FullName;
        }
        protected string GetFFMpegPath(NodeParameters args)
        {
            string ffmpeg = args.GetToolPath("FFMpeg");
            if (string.IsNullOrEmpty(ffmpeg))
            {
                args.Logger.ELog("FFMpeg tool not found.");
                return "";
            }
            var fileInfo = new FileInfo(ffmpeg);
            if (fileInfo.Exists == false)
            {
                args.Logger.ELog("FFMpeg tool configured by ffmpeg file does not exist.");
                return "";
            }
            return fileInfo.DirectoryName;
        }

        private const string MUSIC_INFO = "MusicInfo";
        protected void SetMusicInfo(NodeParameters args, MusicInfo musicInfo, Dictionary<string, object> variables)
        {
            if (args.Parameters.ContainsKey(MUSIC_INFO))
                args.Parameters[MUSIC_INFO] = musicInfo;
            else
                args.Parameters.Add(MUSIC_INFO, musicInfo);

            if(musicInfo.Artist.EndsWith(", The"))
                variables.AddOrUpdate("mi.Artist", "The " + musicInfo.Artist.Substring(0, musicInfo.Artist.Length - ", The".Length).Trim());
            else
                variables.AddOrUpdate("mi.Artist", musicInfo.Artist);

            if(musicInfo.Artist?.StartsWith("The ") == true)
                variables.AddOrUpdate("mi.ArtistThe", musicInfo.Artist.Substring(4).Trim() + ", The");
            else
                variables.AddOrUpdate("mi.ArtistThe", musicInfo.Artist);

            variables.AddOrUpdate("mi.Album", musicInfo.Album);
            variables.AddOrUpdate("mi.Bitrate", musicInfo.Bitrate);
            variables.AddOrUpdate("mi.Channels", musicInfo.Channels);
            variables.AddOrUpdate("mi.Codec", musicInfo.Codec);
            variables.AddOrUpdate("mi.Date", musicInfo.Date);
            variables.AddOrUpdate("mi.Year", musicInfo.Date.Year);
            variables.AddOrUpdate("mi.Duration", musicInfo.Duration);
            variables.AddOrUpdate("mi.Encoder", musicInfo.Encoder);
            variables.AddOrUpdate("mi.Frequency", musicInfo.Frequency);
            variables.AddOrUpdate("mi.Genres", musicInfo.Genres);
            variables.AddOrUpdate("mi.Language", musicInfo.Language);
            variables.AddOrUpdate("mi.Title", musicInfo.Title);
            variables.AddOrUpdate("mi.Track", musicInfo.Track);
            variables.AddOrUpdate("mi.Disc", musicInfo.Disc < 1 ? 1 : musicInfo.Disc);
            variables.AddOrUpdate("mi.TotalDiscs", musicInfo.TotalDiscs < 1 ? 1 : musicInfo.TotalDiscs);

            args.UpdateVariables(variables);
        }

        protected MusicInfo GetMusicInfo(NodeParameters args)
        {
            if (args.Parameters.ContainsKey(MUSIC_INFO) == false)
            {
                args.Logger.WLog("No codec information loaded, use a 'Music File' node first");
                return null;
            }
            var result = args.Parameters[MUSIC_INFO] as MusicInfo;
            if (result == null)
            {
                args.Logger.WLog("MusicInfo not found for file");
                return null;
            }
            return result;
        }

        protected bool ReadMusicFileInfo(NodeParameters args, string ffmpegExe, string filename)
        {

            var musicInfo = new MusicInfoHelper(ffmpegExe, args.Logger).Read(filename);
            if (musicInfo.Duration == 0)
            {
                args.Logger?.ILog("Failed to load music information.");
                return false;
            }

            SetMusicInfo(args, musicInfo, Variables);
            return true;
        }
    }
}