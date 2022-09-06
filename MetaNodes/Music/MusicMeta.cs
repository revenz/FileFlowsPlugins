using FileFlows.Plugin;
using Id3;
using System.Text.RegularExpressions;

namespace MetaNodes.Music
{
    public class MusicMeta:Node
    {
        public override int Inputs => 1;
        public override int Outputs => 1;
        public override string HelpUrl => "https://docs.fileflows.com/plugins/meta-nodes/music-meta";

        private Dictionary<string, object> _Variables;
        public override Dictionary<string, object> Variables => _Variables;
        public override string Icon => "fas fa-music";

        public MusicMeta()
        {
            _Variables = new Dictionary<string, object>()
            {
                { "music.Artist", "A Fine Frenzy" },
                { "music.Album", "Pines" },
                //{ "music.Year", 2012 },
                { "music.Track.Number", 4 },
                { "music.Track.Name", "Riversong" },
            };
        }


        public override int Execute(NodeParameters args)
        {
            string artist = FindArtist(args);
            string album = FindAlbum(args, artist);
            int trackNumber = FindTrack(args, artist, album);
            int year = FindYear(args, artist, album);
            string track = FindTrackName(args, artist, album);
            //Query q = new Query("FileFlows.MusicBrainzTagger", "0.0.1");
            //findalbu
            //q.FindArtists(artist);

            Variables["music.Artist"] = artist;
            Variables["music.Album"] = album;
            Variables["music.Year"] = year;
            Variables["music.Track.Number"] = trackNumber;
            Variables["music.Track.Name"] = track;

            args.UpdateVariables(Variables);

            return 1;
        }

        private string FindTrackName(NodeParameters args, string artist, string album)
        {
            try
            {
                using (var mp3 = new Mp3(args.WorkingFile))
                {
                    Id3Tag tag = mp3.GetTag(Id3TagFamily.Version2X);
                    if (tag != null && tag.Track != null && tag.Track.TrackCount > 0)
                        return tag.Title;
                }
            }
            catch (Exception) { }
            return String.Empty;
        }
        private int FindTrack(NodeParameters args, string artist, string album)
        {
            try
            {
                using (var mp3 = new Mp3(args.WorkingFile))
                {
                    Id3Tag tag = mp3.GetTag(Id3TagFamily.Version2X);
                    if (tag != null && tag.Track != null && tag.Track.TrackCount > 0)
                        return tag.Track.TrackCount;
                }
            }
            catch (Exception) { }
            // find in the filename
            string filename = new FileInfo(args.FileName).Name;
            var match = Regex.Match(filename, @"[\d]{1,3}");
            if (match.Success)
                return int.Parse(match.Value);
            return 0;
        }

        private int FindYear(NodeParameters args, string artist, string album)
        {
            try
            {
                using (var mp3 = new Mp3(args.WorkingFile))
                {
                    Id3Tag tag = mp3.GetTag(Id3TagFamily.Version2X);
                    if (tag?.Year?.Value > 0)
                        return tag.Year.Value.Value;
                }
            }
            catch (Exception) { }
            // find in the filename
            string filename = new FileInfo(args.FileName).Name;
            var match = Regex.Match(filename, @"(?<=([^\d]))[\d]{4}(?=([^\d]))");
            if (match.Success)
                return int.Parse(match.Value);
            return 0;
        }
        private string FindAlbum(NodeParameters args, string artist)
        {
            try
            {
                using (var mp3 = new Mp3(args.WorkingFile))
                {
                    Id3Tag tag = mp3.GetTag(Id3TagFamily.Version2X);
                    if (string.IsNullOrEmpty(tag.Album) == false)
                        return tag.Album;
                }
            }
            catch (Exception) { }

            // try find it from the filename....
            var parts = args.RelativeFile.Replace("\\", "/").Split('/');
            if (parts.Length == 3)
                return parts[1]; // artist/album/song.mp3
            else if (parts.Length > 3)
                return parts[parts.Length - 2];

            // not in path. try looking for it in the filename
            string file = new FileInfo(args.FileName).Name;

            return Regex.Match(parts[parts.Length - 1], @"(?<=([\-_]))[^\-_]+").Value?.Trim() ?? string.Empty;
        }

        private string FindArtist(NodeParameters args)
        {
            try
            {
                using (var mp3 = new Mp3(args.WorkingFile))
                {
                    Id3Tag tag = mp3.GetTag(Id3TagFamily.Version2X);
                    if (string.IsNullOrEmpty(tag.Artists) == false)
                        return tag.Artists;
                }
            }
            catch (Exception ex) { }

            // try find it from the filename....
            var parts = args.RelativeFile.Replace("\\", "/").Split('/');
            if (parts.Length == 3)
                return parts[0]; // artist/album/song.mp3
            else if (parts.Length > 3)
                return parts[parts.Length - 3];
            else if (parts.Length == 2)
                return parts[0];

            // not in path. try looking for it in the filename
            return Regex.Match(parts[parts.Length - 1], @"^[^\-_]+").Value?.Trim() ?? string.Empty;
        }
    }
}
