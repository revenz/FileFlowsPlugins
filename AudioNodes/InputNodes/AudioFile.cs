namespace FileFlows.AudioNodes
{
    using System.ComponentModel;
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;

    public class AudioFile : AudioNode
    {
        public override int Outputs => 1;
        public override FlowElementType Type => FlowElementType.Input;

        public override string HelpUrl => "https://docs.fileflows.com/plugins/audio-nodes/audio-file";

        private Dictionary<string, object> _Variables;
        public override Dictionary<string, object> Variables => _Variables;
        public AudioFile()
        {
            _Variables = new Dictionary<string, object>()
            {
                { "mi.Album", "Album" },
                { "mi.Artist", "Artist" },
                { "mi.ArtistThe", "Artist, The" },
                { "mi.Bitrate", 845 },
                { "mi.Channels", 2 },
                { "mi.Codec", "flac" },
                { "mi.Date", new DateTime(2020, 05, 23) },
                { "mi.Year", 2020 },
                { "mi.Duration", 256 },
                { "mi.Encoder", "FLAC 1.2.1" },
                { "mi.Frequency", 44100 },
                { "mi.Genres", new  [] { "Pop", "Rock" } },
                { "mi.Language", "English" },
                { "mi.Title", "Song Title" },
                { "mi.Track", 2 },
                { "mi.Disc", 2 },
                { "mi.TotalDiscs", 2 }
        };
        }

        public override int Execute(NodeParameters args)
        {
            string ffmpegExe = GetFFMpegExe(args);
            if (string.IsNullOrEmpty(ffmpegExe))
                return -1;

            try
            {
                if (ReadAudioFileInfo(args, ffmpegExe, args.WorkingFile))
                    return 1;

                var AudioInfo = GetAudioInfo(args);

                if (string.IsNullOrEmpty(AudioInfo.Codec) == false)
                    args.RecordStatistic("CODEC", AudioInfo.Codec);

                return 0;
            }
            catch (Exception ex)
            {
                args.Logger.ELog("Failed processing AudioFile: " + ex.Message);
                return -1;
            }
        }
    }


}