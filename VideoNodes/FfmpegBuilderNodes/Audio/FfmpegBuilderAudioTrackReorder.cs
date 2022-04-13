using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using System.Text;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes
{
    public class FfmpegBuilderAudioTrackReorder : FfmpegBuilderNode
    {
        public override int Outputs => 2;

        public override string Icon => "fas fa-volume-off";

        [StringArray(1)]
        public List<string> Languages { get; set; }

        [StringArray(2)]
        public List<string> OrderedTracks { get; set; }

        [StringArray(3)]
        public List<string> Channels { get; set; }

        public override int Execute(NodeParameters args)
        {
            base.Init(args);

            OrderedTracks = OrderedTracks?.Select(x => x.ToLower())?.ToList() ?? new();

            var reordered = Reorder(Model.AudioStreams);

            bool same = AreSame(Model.AudioStreams, reordered);

            if (same)
            {
                args.Logger?.ILog("No audio tracks need reordering");
                return 2;
            }

            Model.AudioStreams = reordered;
            // set the first audio track as the default track
            Model.MetadataParameters.AddRange(new[] { "-disposition:a:0", "default" });

            return 1;
        }

        public List<FfmpegAudioStream> Reorder(List<FfmpegAudioStream> input)
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
                int langIndex = Languages.IndexOf(x.Stream.Language?.ToLower() ?? String.Empty);
                int codecIndex = OrderedTracks.IndexOf(x.Stream.Codec?.ToLower() ?? String.Empty);
                int channelIndex = actualChannels.IndexOf(x.Stream.Channels);

                int result = base_number;
                if (langIndex >= 0)
                {
                    result -= ((langIndex + 1) * 10_000_000);
                }
                if (codecIndex >= 0)
                {
                    result -= ((codecIndex + 1) * 100_000);
                }
                if (channelIndex >= 0)
                {
                    result -= ((channelIndex + 1) * 1_000);
                }
                if (result == base_number)
                    result = ++count;
                return result;
            }).ToList();

            return data;
        }
        public bool AreSame(List<FfmpegAudioStream> original, List<FfmpegAudioStream> reordered)
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
    }
}
