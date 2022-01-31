namespace FileFlows.MusicNodes
{
    public class MusicInfo
    {
        public string Language { get; set; }
        public int Track { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Album { get; set; }
        public DateTime Date { get; set; }
        public string[] Genres { get; set; }
        public string Encoder { get; set; }
        public long Duration { get; set; }
        public long BitRate { get; set; }
        public string Codec { get; set; }
        public long Channels { get; set; }
        public long Frequency { get; set; }
    }
}