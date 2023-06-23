using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using System.Text;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

public class FfmpegBuilderAudioTrackReorder : FfmpegBuilderNode
{
    public override int Outputs => 2;

    public override string Icon => "fas fa-sort-alpha-down";

    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/track-reorder";


    [Select(nameof(StreamTypeOptions), 1)]
    public string StreamType { get; set; }

    [StringArray(2)]
    public List<string> Languages { get; set; }

    [StringArray(3)]
    public List<string> OrderedTracks { get; set; }

    [StringArray(4)]
    [ConditionEquals(nameof(StreamType), "Subtitle", inverse: true)]
    public List<string> Channels { get; set; }

    private static List<ListOption> _StreamTypeOptions;
    public static List<ListOption> StreamTypeOptions
    {
        get
        {
            if (_StreamTypeOptions == null)
            {
                _StreamTypeOptions = new List<ListOption>
                {
                    new ListOption { Label = "Audio", Value = "Audio" },
                    new ListOption { Label = "Subtitle", Value = "Subtitle" }
                };
            }
            return _StreamTypeOptions;
        }
    }

    public override int Execute(NodeParameters args)
    {
        OrderedTracks = OrderedTracks?.Select(x => x.ToLower())?.ToList() ?? new();

        if (StreamType == "Subtitle")
        {
            var reordered = Reorder(Model.SubtitleStreams);

            bool same = AreSame(Model.SubtitleStreams, reordered);

            for(int i = 0; i < reordered.Count; i++)
            {
                reordered[i].IsDefault = i == 0;
            }
            
            if (same)
            {
                args.Logger?.ILog("No subtitle tracks need reordering");
                return 2;
            }

            Model.SubtitleStreams = reordered;

            return 1;

        }
        else
        {
            var reordered = Reorder(Model.AudioStreams);

            bool same = AreSame(Model.AudioStreams, reordered);

            for(int i = 0; i < reordered.Count; i++)
            {
                reordered[i].IsDefault = i == 0;
            }
            
            if (same)
            {
                args.Logger?.ILog("No audio tracks need reordering");
                return 2;
            }

            Model.AudioStreams = reordered;
            Model.ForceEncode = true;

            return 1;
        }
    }

    public List<T> Reorder<T>(List<T> input) where T : FfmpegStream
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
            int langIndex = 0;
            int codecIndex = 0;
            int channelIndex = 0;

            if (x is FfmpegAudioStream audioStream)
            {
                langIndex = Languages.IndexOf(audioStream.Stream.Language?.ToLower() ?? String.Empty);
                codecIndex = OrderedTracks.IndexOf(audioStream.Stream.Codec?.ToLower() ?? String.Empty);
                channelIndex = actualChannels.IndexOf(audioStream.Stream.Channels);
            }
            else if (x is FfmpegSubtitleStream subStream)
            {
                langIndex = Languages.IndexOf(subStream.Stream.Language?.ToLower() ?? String.Empty);
                codecIndex = OrderedTracks.IndexOf(subStream.Stream.Codec?.ToLower() ?? String.Empty);
            }

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
    public bool AreSame<T>(List<T> original, List<T> reordered) where T: FfmpegStream
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
