namespace FileFlows.MusicNodes
{
    using System.ComponentModel;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public class MusicFile : MusicNode
    {
        public override int Outputs => 1;
        public override FlowElementType Type => FlowElementType.Input;

        private Dictionary<string, object> _Variables;
        public override Dictionary<string, object> Variables => _Variables;
        public MusicFile()
        {
            _Variables = new Dictionary<string, object>()
            {
                { "mi.Album", "Album" },
                { "mi.Artist", "Artist" },
                { "mi.BitRate", 845 },
                { "mi.Channels", 2 },
                { "mi.Codec", "flac" },
                { "mi.Date", new DateTime(2020, 05, 23) },
                { "mi.Duration", 256 },
                { "mi.Encoder", "FLAC 1.2.1" },
                { "mi.Frequency", 44100 },
                { "mi.Genres", new  [] { "Pop", "Rock" } },
                { "mi.Language", "English" },
                { "mi.Title", "Song Title" },
                { "mi.Track", 2 },
                { "mi.TrackPad", "02" }
            };
        }

        public override int Execute(NodeParameters args)
        {
            string ffmpegExe = GetFFMpegExe(args);
            if (string.IsNullOrEmpty(ffmpegExe))
                return -1;

            try
            {

                var musicInfo = new MusicInfoHelper(ffmpegExe, args.Logger).Read(args.WorkingFile);
                if (musicInfo.Duration == 0)
                {
                    args.Logger.ILog("Failed to load music information.");
                    return 0;
                }

                SetMusicInfo(args, musicInfo, Variables);

                return 1;
            }
            catch (Exception ex)
            {
                args.Logger.ELog("Failed processing MusicFile: " + ex.Message);
                return -1;
            }
        }
    }


}