using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using System.Text;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// FFmpeg Builder: Audio Track Reorder
/// </summary>
public class FfmpegBuilderAudioTrackReorder : FfmpegBuilderNode
{
    /// <summary>
    /// Gets the number of output nodes
    /// </summary>
    public override int Outputs => 2;
    /// <summary>
    /// Gets the icon
    /// </summary>
    public override string Icon => "fas fa-sort-alpha-down";
    /// <summary>
    /// Gets the help URL
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/track-reorder";
    /// <summary>
    /// Gets or sets the stream type
    /// </summary>
    [Select(nameof(StreamTypeOptions), 1)]
    public string StreamType { get; set; }
    /// <summary>
    /// Gets or sets the languages
    /// </summary>
    [StringArray(2)]
    public List<string> Languages { get; set; }
    /// <summary>
    /// Gets or sets the ordered tracks
    /// </summary>
    [StringArray(3)]
    public List<string> OrderedTracks { get; set; }
    /// <summary>
    /// Gets or sets the channels
    /// </summary>
    [StringArray(4)]
    [ConditionEquals(nameof(StreamType), "Subtitle", inverse: true)]
    public List<string> Channels { get; set; }

    private static List<ListOption> _StreamTypeOptions;
    /// <summary>
    /// Gets or sets the stream type options
    /// </summary>
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

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the next output node</returns>
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

    /// <summary>
    /// Reorders the tracks
    /// </summary>
    /// <param name="input">the inputs to reorder</param>
    /// <typeparam name="T">the type to reorder</typeparam>
    /// <returns>the reordered tracks</returns>
    public List<T> Reorder<T>(List<T> input) where T : FfmpegStream
        => Reorder(Languages, OrderedTracks, Channels, input);
    
    public static  List<T> Reorder<T>(List<string> languages, List<string> orderedTracks, List<string> channels, List<T> input) where T : FfmpegStream
    {
        languages ??= new List<string>();
        orderedTracks ??= new List<string>();
        channels ??= new List<string>();
        List<float> actualChannels = channels.Select(x =>
        {
            if (float.TryParse(x, out float value))
                return value;
            return -1f;
        }).Where(x => x > 0f).ToList();

        if (languages.Any() == false && orderedTracks.Any() == false && actualChannels.Any() == false)
            return input; // nothing to do

        languages.Reverse();
        orderedTracks.Reverse();
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
                langIndex = languages.IndexOf(audioStream.Stream.Language?.ToLower() ?? String.Empty);
                codecIndex = orderedTracks.IndexOf(audioStream.Stream.Codec?.ToLower() ?? String.Empty);
                float asChannels = audioStream.Channels > 0 ? audioStream.Channels : audioStream.Stream.Channels;
                channelIndex = actualChannels.IndexOf(asChannels);
            }
            else if (x is FfmpegSubtitleStream subStream)
            {
                langIndex = languages.IndexOf(subStream.Stream.Language?.ToLower() ?? String.Empty);
                codecIndex = orderedTracks.IndexOf(subStream.Stream.Codec?.ToLower() ?? String.Empty);
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
    
    /// <summary>
    /// Tests if two lists are the same
    /// </summary>
    /// <param name="original">the original list</param>
    /// <param name="reordered">the reordered list</param>
    /// <typeparam name="T">the type of items</typeparam>
    /// <returns>true if the lists are the same, otherwise false</returns>
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
