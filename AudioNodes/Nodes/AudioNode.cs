namespace FileFlows.AudioNodes
{
    using FileFlows.Plugin;

    public abstract class AudioNode : Node
    {
        public override string Icon => "fas fa-music";

        protected string LocalWorkingFile;

        public override bool PreExecute(NodeParameters args)
        {
            var localFile = args.FileService.GetLocalPath(args.WorkingFile);
            if (localFile.IsFailed)
            {
                args.Logger?.ELog("Failed to get local file: " + localFile.Error);
                return false;
            }

            LocalWorkingFile = localFile.Value;
            return true;
        }

        /// <summary>
        /// Gets the FFmpeg location
        /// </summary>
        /// <param name="args">the node parameters</param>
        /// <returns>the FFmpeg location</returns>
        protected Result<string> GetFFmpeg(NodeParameters args)
        {
            string ffmpeg = args.GetToolPath("FFMpeg");
            if (string.IsNullOrEmpty(ffmpeg))
                return Result<string>.Fail("FFmpeg tool not found.");
            var fileInfo = new System.IO.FileInfo(ffmpeg);
            if (fileInfo.Exists == false)
                Result<string>.Fail("FFmpeg tool configured by ffmpeg file does not exist.");
            return fileInfo.FullName;
        }

        /// <summary>
        /// Gets the FFprobe location
        /// </summary>
        /// <param name="args">the node parameters</param>
        /// <returns>the FFprobe location</returns>
        protected Result<string> GetFFprobe(NodeParameters args)
        {
            string ffmpeg = args.GetToolPath("FFprobe");
            if (string.IsNullOrEmpty(ffmpeg))
                return Result<string>.Fail("FFprobe tool not found.");
            
            var fileInfo = new System.IO.FileInfo(ffmpeg);
            if (fileInfo.Exists == false)
                return Result<string>.Fail("FFprobe tool configured by ffmpeg file does not exist.");
            return fileInfo.FullName;
        }


        private const string Audio_INFO = "AudioInfo";
        internal void SetAudioInfo(NodeParameters args, AudioInfo AudioInfo, Dictionary<string, object> variables, string filename)
        {
            args.Parameters[Audio_INFO] = AudioInfo;

            if(AudioInfo.Artist.EndsWith(", The"))
                variables.AddOrUpdate("audio.Artist", "The " + AudioInfo.Artist.Substring(0, AudioInfo.Artist.Length - ", The".Length).Trim());
            else
                variables.AddOrUpdate("audio.Artist", AudioInfo.Artist);

            if(AudioInfo.Artist?.StartsWith("The ") == true)
                variables.AddOrUpdate("audio.ArtistThe", AudioInfo.Artist.Substring(4).Trim() + ", The");
            else
                variables.AddOrUpdate("audio.ArtistThe", AudioInfo.Artist);

            variables.AddOrUpdate("audio.Album", AudioInfo.Album);
            variables.AddOrUpdate("audio.Bitrate", AudioInfo.Bitrate);
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


            var metadata = new Dictionary<string, object>();
            metadata.Add("Duration", AudioInfo.Duration);
            metadata.Add("Codec", AudioInfo.Codec);
            metadata.Add("Bitrate", AudioInfo.Bitrate);
            metadata.Add("Channels", AudioInfo.Channels);
            AddIfSet(metadata, "Date", AudioInfo.Date);
            AddIfSet(metadata, "Frequency", AudioInfo.Frequency);
            AddIfSet(metadata, "Encoder", AudioInfo.Encoder);
            AddIfSet(metadata, "Genres", AudioInfo.Genres);
            AddIfSet(metadata, "Language", AudioInfo.Language);
            AddIfSet(metadata, "Title", AudioInfo.Title);
            AddIfSet(metadata, "Track", AudioInfo.Track);
            AddIfSet(metadata, "Disc", AudioInfo.Disc);
            AddIfSet(metadata, "TotalDiscs", AudioInfo.TotalDiscs);
            args.SetMetadata(metadata);
            
            
            string extension = FileHelper.GetExtension(filename).ToLowerInvariant()[1..];
            switch (extension)
            {
                case "mp3":
                    args.SetMimeType("audio/mpeg");
                    break;
                case "wav":
                    args.SetMimeType("audio/wav");
                    break;
                case "flac":
                    args.SetMimeType("audio/flac");
                    break;
                case "aac":
                    args.SetMimeType("audio/aac");
                    break;
                case "ogg":
                    args.SetMimeType("audio/ogg");
                    break;
                case "m4a":
                    args.SetMimeType("audio/mp4"); // Used for AAC or ALAC audio in MP4 container
                    break;
                case "opus":
                    args.SetMimeType("audio/opus");
                    break;
                case "wma":
                    args.SetMimeType("audio/x-ms-wma");
                    break;
                case "amr":
                    args.SetMimeType("audio/amr");
                    break;
                case "aiff":
                case "aif":
                    args.SetMimeType("audio/aiff");
                    break;
                default:
                    args.SetMimeType("audio/" + extension); // Fallback for unknown extensions
                    break;
            }

            args.UpdateVariables(variables);
        }

        private void AddIfSet(Dictionary<string, object> dict, string name, object value)
        {
            if (value == null)
                return;
            if (value is string sValue && string.IsNullOrWhiteSpace(sValue))
                return;
            if (value is int iValue && iValue < 1)
                return;
            if (value is TimeSpan tsValue && tsValue.TotalSeconds < 1)
                return;
            if (value is DateTime dtValue && dtValue.Year <= 1900)
                return;
            if (value is IEnumerable<string> strList && strList.Any() == false)
                return;
            dict.Add(name, value);  
        }
        
        /// <summary>
        /// Gets the audio information previously read into the flow
        /// </summary>
        /// <param name="args">the node parameters</param>
        /// <returns>the audio information</returns>
        protected Result<AudioInfo> GetAudioInfo(NodeParameters args)
        {
            if (args.Parameters.ContainsKey(Audio_INFO) == false)
                return Result<AudioInfo>.Fail("No codec information loaded, use a 'Audio File' flow element first");
            if (args.Parameters[Audio_INFO] is AudioInfo result == false)
                return Result<AudioInfo>.Fail("AudioInfo not found for file");
            return result;
        }

        protected bool ReadAudioFileInfo(NodeParameters args, string ffmpegExe, string ffprobe, string filename)
        {
            var result = new AudioInfoHelper(ffmpegExe, ffprobe, args.Logger).Read(filename);
            if (result.Failed(out string error))
            {
                args.FailureReason = error;
                args.Logger?.ELog(error);
                return false;
            }

            SetAudioInfo(args, result.Value, Variables, filename);
            return true;
        }
    }
}