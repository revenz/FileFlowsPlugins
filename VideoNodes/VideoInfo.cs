namespace FileFlows.VideoNodes
{
    public class VideoInfo
    {
        public string FileName { get; set; }
        /// <summary>
        /// Gets or sets the bitrate in bytes per second
        /// </summary>
        public float Bitrate { get; set; }
        public List<VideoStream> VideoStreams { get; set; } = new List<VideoStream>();
        public List<AudioStream> AudioStreams { get; set; } = new List<AudioStream>();
        public List<SubtitleStream> SubtitleStreams { get; set; } = new List<SubtitleStream>();
    }

    public class VideoFileStream
    {
        /// <summary>
        /// The original index of the stream in the overall video
        /// </summary> 
        public int Index { get; set; }
        /// <summary>
        /// The index of the specific type
        /// </summary>
        public int TypeIndex { get; set; }
        /// <summary>
        /// The stream title (name)
        /// </summary>
        public string Title { get; set; } = "";

        /// <summary>
        /// The bitrate(BPS) of the video stream in bytes per second
        /// </summary>
        public float Bitrate { get; set; }

        /// <summary>
        /// The codec of the stream
        /// </summary>
        public string Codec { get; set; } = "";

        public string IndexString { get; set; }
    }

    public class VideoStream : VideoFileStream
    {
        /// <summary>
        /// The width of the video stream
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// The height of the video stream
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// The number of frames per second
        /// </summary>
        public float FramesPerSecond { get; set; }

        /// <summary>
        /// The duration of the stream
        /// </summary>
        public TimeSpan Duration { get; set; }
    }

    public class AudioStream : VideoFileStream
    {
        /// <summary>
        /// The language of the stream
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// The channels of the stream
        /// </summary>
        public float Channels { get; set; }

        /// <summary>
        /// The duration of the stream
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// The sample rate of the audio stream
        /// </summary>
        public int SampleRate { get; set; }
    }

    public class SubtitleStream : VideoFileStream
    {
        /// <summary>
        /// The language of the stream
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// If this is a forced subtitle
        /// </summary>
        public bool Forced { get; set; }
    }
}