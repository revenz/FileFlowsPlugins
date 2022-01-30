namespace FileFlows.VideoNodes
{
    using FileFlows.Plugin;
    using FileFlows.Plugin.Attributes;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;

    public class AudioTrackReorder: EncodingNode
    {
        public override int Outputs => 2;

        public override string Icon => "fas fa-volume-off";

        [StringArray(1)]
        public List<string> Languages { get; set; }

        [StringArray(2)]
        public List<string> OrderedTracks { get; set; }

        [StringArray(3)]
        public List<string> Channels { get; set; }

        public List<AudioStream> Reorder(List<AudioStream> input)
        {
            Languages ??= new List<string>();
            OrderedTracks ??= new List<string>();
            Channels ??= new List<string>();
            List<float> actualChannels = Channels.Select(x =>
            {
                if (float.TryParse(x, out float value))
                    return value;
                return -1f;
            }).Where(x => x > 0f).ToList();

            if (Languages.Any() == false && OrderedTracks.Any() == false && actualChannels.Any() == false)
                return input; // nothing to do

            Languages.Reverse();
            OrderedTracks.Reverse();
            actualChannels.Reverse();

            const int base_number = 1_000_000_000;
            int count = base_number;
            var debug = new StringBuilder();
            var data = input.OrderBy(x =>
            {
                int langIndex = Languages.IndexOf(x.Language?.ToLower() ?? String.Empty);
                int codecIndex = OrderedTracks.IndexOf(x.Codec?.ToLower() ?? String.Empty);
                int channelIndex = actualChannels.IndexOf(x.Channels);

                int result = base_number;
                if (langIndex >= 0)
                {
                    result -= ((langIndex + 1) * 10_000_000);
                }
                if(codecIndex >= 0)
                {
                    result -= ((codecIndex + 1) * 100_000);
                }
                if(channelIndex >= 0)
                {
                    result -= ((channelIndex + 1) * 1_000);
                }
                if (result == base_number)
                    result = ++count;
                return result;
            }).ToList();

            
            return data;

        }

        public bool AreSame(List<AudioStream> original, List<AudioStream> reordered)
        {
            for (int i = 0; i < reordered.Count; i++)
            {
                if (reordered[i] != original[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override int Execute(NodeParameters args)
        {
            try
            {
                VideoInfo videoInfo = GetVideoInfo(args);
                if (videoInfo == null)
                    return -1;

                string ffmpegExe = GetFFMpegExe(args);
                if (string.IsNullOrEmpty(ffmpegExe))
                    return -1;

                List<string> ffArgs = new List<string>
                {
                    "-c", "copy",
                    "-map", "0:v",
                };


                OrderedTracks = OrderedTracks?.Select(x => x.ToLower())?.ToList() ?? new ();

                var reordered = Reorder(videoInfo.AudioStreams);

                bool same = AreSame(videoInfo.AudioStreams, reordered);

                if(same)
                {
                    args.Logger?.ILog("No audio tracks need reordering");
                    return 2;
                }

                foreach (var audio in reordered)
                {
                    ffArgs.AddRange(new[] { "-map", audio.IndexString });
                }

                if (videoInfo.SubtitleStreams?.Any() == true)
                    ffArgs.AddRange(new[] { "-map", "0:s" });

                // this makes the first audio track now the default track
                ffArgs.AddRange(new[] { "-disposition:a:0", "default" });

                string extension = new FileInfo(args.WorkingFile).Extension;
                if(extension.StartsWith("."))
                    extension = extension.Substring(1); 

                if (Encode(args, ffmpegExe, ffArgs, extension) == false)
                    return -1;

                return 1;
            }
            catch (Exception ex)
            {
                args.Logger?.ELog("Failed processing VideoFile: " + ex.Message);
                return -1;
            }
}
    }
}
