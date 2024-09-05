namespace FileFlows.AudioNodes
{
    public class AudioInfo
    {
        /// <summary>
        /// Gets or sets the title of the audio file.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the artist of the audio file.
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// Gets or sets the album of the audio file.
        /// </summary>
        public string Album { get; set; }
        /// <summary>
        /// Gets or sets the track number of the audio file.
        /// </summary>
        public int Track { get; set; }
        /// <summary>
        /// Gets or sets the language number of the audio file.
        /// </summary>
        public string Language { get; set; }
        /// <summary>
        /// Gets or sets the disc number of the audio file.
        /// </summary>
        public int Disc { get; set; }
        /// <summary>
        /// Gets or sets the total number of discs in the album.
        /// </summary>
        public int TotalDiscs { get; set; }
        /// <summary>
        /// Gets or sets the total number of tracks in the album.
        /// </summary>
        public int TotalTracks { get; set; }
        /// <summary>
        /// Gets or sets the release date of the audio file.
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Gets or sets the genre of the audio file.
        /// </summary>
        public string[] Genres { get; set; }
        /// <summary>
        /// Gets or sets duration in SECONDS
        /// </summary>
        public long Duration { get; set; }
        /// <summary>
        /// Gets or sets the bitrate (in bytes per second)
        /// </summary>
        public long Bitrate { get; set; }
        public string Encoder { get; set; }
        public string Codec { get; set; }
        public long Channels { get; set; }
        public long Frequency { get; set; }
    }
}