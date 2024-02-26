namespace FileFlows.VideoNodes.FfmpegBuilderNodes.Models
{
    public class FfmpegModel
    {
        private List<FfmpegVideoStream> _VideoStreams = new List<FfmpegVideoStream>();
        public List<FfmpegVideoStream> VideoStreams
        {
            get => _VideoStreams;
            set => _VideoStreams = value ?? new List<FfmpegVideoStream>();
        }
        private List<FfmpegAudioStream> _AudioStreams = new List<FfmpegAudioStream>();
        public List<FfmpegAudioStream> AudioStreams
        {
            get => _AudioStreams;
            set => _AudioStreams = value ?? new List<FfmpegAudioStream>();
        }
        private List<FfmpegSubtitleStream> _SubtitleStreams = new List<FfmpegSubtitleStream>();
        public List<FfmpegSubtitleStream> SubtitleStreams
        {
            get => _SubtitleStreams;
            set => _SubtitleStreams = value ?? new List<FfmpegSubtitleStream>();
        }

        private List<string> _MetadataParameters = new List<string>();
        public List<string> MetadataParameters
        {
            get => _MetadataParameters;
            set => _MetadataParameters = value ?? new List<string>();
        }

        public string Extension { get; set; }

        private List<InputFile> _InputFiles = new List<InputFile>();
        public List<InputFile> InputFiles
        {
            get => _InputFiles;
            set => _InputFiles = value ?? new List<InputFile>();
        }

        private List<string> _CustomParameters = new List<string>();

        /// <summary>
        /// Gets or sets custom parameters to use in the FFMPEG Builder
        /// </summary>
        public List<string> CustomParameters
        {
            get => _CustomParameters;
            set => _CustomParameters = value ?? new List<string>();
        }

        /// <summary>
        /// Gets or sets if the builder should forcible execute even if nothing appears to have changed
        /// </summary>
        public bool ForceEncode { get; set; }
        
        /// <summary>
        /// Gets or sets the code to run prior to FFmpeg Executing 
        /// </summary>
        public string PreExecuteCode { get; set; }
        
        /// <summary>
        /// Gets or sets if attachments should be removed from the output file
        /// </summary>
        public bool RemoveAttachments { get; set; }

        /// <summary>
        /// Gets or sets the video information for this video file
        /// </summary>
        public VideoInfo VideoInfo => _VideoInfo;
        readonly VideoInfo _VideoInfo;

        public FfmpegModel(VideoInfo info)
        {
            this._VideoInfo = info;
        }
        
        
        /// <summary>
        /// Gets or sets a watermark to apply
        /// </summary>
        internal WatermarkModel Watermark { get; set; }

        /// <summary>
        /// Gets or sets the cut duration of the video, ie the -t variable
        /// </summary>
        public TimeSpan? CutDuration { get; set; }

        /// <summary>
        /// Gets or sets the start time of hte video, ie the -ss variable
        /// </summary>
        public TimeSpan? StartTime { get; set; }

        internal static FfmpegModel CreateModel(VideoInfo info)
        {
            var model = new FfmpegModel(info);
            model.InputFiles.Add(new InputFile(info.FileName));
            foreach (var item in info.VideoStreams.Select((stream, index) => (stream, index)))
            {
                model.VideoStreams.Add(new FfmpegVideoStream
                {
                    Index = item.index,
                    Title = item.stream.Title,
                    Stream = item.stream,
                    Codec = item.stream.Codec
                });
            }
            foreach (var item in info.AudioStreams.Select((stream, index) => (stream, index)))
            {
                model.AudioStreams.Add(new FfmpegAudioStream
                {
                    Index = item.index,
                    Title = item.stream.Title,
                    Language = item.stream.Language,
                    Stream = item.stream,
                    Channels = item.stream.Channels,
                    IsDefault = item.stream.Default,
                    Codec = item.stream.Codec
                });
            }
            foreach (var item in info.SubtitleStreams.Select((stream, index) => (stream, index)))
            {
                model.SubtitleStreams.Add(new FfmpegSubtitleStream
                {
                    Index = item.index,
                    Title = item.stream.Title,
                    Language = item.stream.Language,
                    Stream = item.stream,
                    IsDefault = item.stream.Default,
                    IsForced = item.stream.Forced,
                    Codec = item.stream.Codec
                });
            }

            if(info.FileName.ToLower().EndsWith(".mp4"))
                model.Extension = info.FileName[(info.FileName.LastIndexOf(".", StringComparison.Ordinal) + 1)..];
            else if (info.FileName.ToLower().EndsWith(".mkv"))
                model.Extension = info.FileName[(info.FileName.LastIndexOf(".", StringComparison.Ordinal) + 1)..];
            else if (info.FileName.ToLower().EndsWith(".mov"))
                model.Extension = info.FileName[(info.FileName.LastIndexOf(".", StringComparison.Ordinal) + 1)..];
            else if (info.FileName.ToLower().EndsWith(".mxf"))
                model.Extension = info.FileName[(info.FileName.LastIndexOf(".", StringComparison.Ordinal) + 1)..];
            else if (info.FileName.ToLower().EndsWith(".webm"))
                model.Extension = info.FileName[(info.FileName.LastIndexOf(".", StringComparison.Ordinal) + 1)..];

            return model;
        }
    }

    /// <summary>
    /// Input file 
    /// </summary>
    public class InputFile
    {
        /// <summary>
        /// Gets or sets the filename of the file
        /// </summary>
        public string FileName { get; set; }    
        /// <summary>
        /// Gets or sets if the file should be deleted after processing
        /// </summary>
        public bool DeleteAfterwards { get; set; }

        public InputFile(string fileName)
        {
            FileName = fileName;
        }
    }
}
