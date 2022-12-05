namespace FileFlows.AudioNodes
{
    public class AudioInfo
    {
        public string Language { get; set; }
        public int Track { get; set; }
        public int Disc { get; set; }
        public int TotalDiscs { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Album { get; set; }
        public DateTime Date { get; set; }
        public string[] Genres { get; set; }
        public string Encoder { get; set; }
        /// <summary>
        /// Gets or sets duration in SECONDS
        /// </summary>
        public long Duration { get; set; }
        /// <summary>
        /// Gets or sets the bitrate (in bytes per second)
        /// </summary>
        public long Bitrate { get; set; }
        public string Codec { get; set; }
        public long Channels { get; set; }
        public long Frequency { get; set; }
    }
}