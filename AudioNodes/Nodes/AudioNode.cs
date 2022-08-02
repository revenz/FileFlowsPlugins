namespace FileFlows.AudioNodes
{
    using FileFlows.Plugin;

    public abstract class AudioNode : Node
    {
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

        private const string Audio_INFO = "AudioInfo";
        protected void SetAudioInfo(NodeParameters args, AudioInfo AudioInfo, Dictionary<string, object> variables)
        {
            if (args.Parameters.ContainsKey(Audio_INFO))
                args.Parameters[Audio_INFO] = AudioInfo;
            else
                args.Parameters.Add(Audio_INFO, AudioInfo);

            if(AudioInfo.Artist.EndsWith(", The"))
                variables.AddOrUpdate("audio.Artist", "The " + AudioInfo.Artist.Substring(0, AudioInfo.Artist.Length - ", The".Length).Trim());
            else
                variables.AddOrUpdate("audio.Artist", AudioInfo.Artist);

            if(AudioInfo.Artist?.StartsWith("The ") == true)
                variables.AddOrUpdate("audio.ArtistThe", AudioInfo.Artist.Substring(4).Trim() + ", The");
            else
                variables.AddOrUpdate("audio.ArtistThe", AudioInfo.Artist);

            variables.AddOrUpdate("audio.Album", AudioInfo.Album);
            variables.AddOrUpdate("audio.BitRate", AudioInfo.BitRate);
            variables.AddOrUpdate("audio.Channels", AudioInfo.Channels);
            variables.AddOrUpdate("audio.Codec", AudioInfo.Codec);
            variables.AddOrUpdate("audio.Date", AudioInfo.Date);
            variables.AddOrUpdate("audio.Year", AudioInfo.Date.Year);
            variables.AddOrUpdate("audio.Duration", AudioInfo.Duration);
            variables.AddOrUpdate("audio.Encoder", AudioInfo.Encoder);
            variables.AddOrUpdate("audio.Frequency", AudioInfo.Frequency);
            variables.AddOrUpdate("audio.Genres", AudioInfo.Genres);
            variables.AddOrUpdate("audio.Language", AudioInfo.Language);
            variables.AddOrUpdate("audio.Title", AudioInfo.Title);
            variables.AddOrUpdate("audio.Track", AudioInfo.Track);
            variables.AddOrUpdate("audio.Disc", AudioInfo.Disc < 1 ? 1 : AudioInfo.Disc);
            variables.AddOrUpdate("audio.TotalDiscs", AudioInfo.TotalDiscs < 1 ? 1 : AudioInfo.TotalDiscs);

            args.UpdateVariables(variables);
        }

        protected AudioInfo GetAudioInfo(NodeParameters args)
        {
            if (args.Parameters.ContainsKey(Audio_INFO) == false)
            {
                args.Logger.WLog("No codec information loaded, use a 'Audio File' node first");
                return null;
            }
            var result = args.Parameters[Audio_INFO] as AudioInfo;
            if (result == null)
            {
                args.Logger.WLog("AudioInfo not found for file");
                return null;
            }
            return result;
        }

        protected bool ReadAudioFileInfo(NodeParameters args, string ffmpegExe, string filename)
        {

            var AudioInfo = new AudioInfoHelper(ffmpegExe, args.Logger).Read(filename);
            if (AudioInfo.Duration == 0)
            {
                args.Logger?.ILog("Failed to load Audio information.");
                return false;
            }

            SetAudioInfo(args, AudioInfo, Variables);
            return true;
        }
    }
}