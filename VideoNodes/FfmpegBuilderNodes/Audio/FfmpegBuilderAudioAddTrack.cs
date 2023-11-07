using System.Diagnostics;
using System.Runtime.InteropServices;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// FFmpeg Builder: Add Audio Track
/// </summary>
public class FfmpegBuilderAudioAddTrack : FfmpegBuilderNode
{
    /// <summary>
    /// Gets the icon for this flow element
    /// </summary>
    public override string Icon => "fas fa-volume-off";
    /// <summary>
    /// Gets the help URL for this flow element
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/audio-add-track";
    /// <summary>
    /// Gets or sets the index to insert this track
    /// </summary>
    [NumberInt(1)]
    [Range(0, 100)]
    [DefaultValue(1)]
    public int Index { get; set; }
    /// <summary>
    /// Gets or sets the codec to to use
    /// </summary>
    [DefaultValue("aac")]
    [Select(nameof(CodecOptions), 1)]
    public string Codec { get; set; }

    private static List<ListOption> _CodecOptions;
    /// <summary>
    /// Gets the codec options
    /// </summary>
    public static List<ListOption> CodecOptions
    {
        get
        {
            if (_CodecOptions == null)
            {
                _CodecOptions = new List<ListOption>
                {
                    new () { Label = "AAC", Value = "aac"},
                    new () { Label = "AC3", Value = "ac3"},
                    new () { Label = "EAC3", Value = "eac3" },
                    new () { Label = "MP3", Value = "mp3"},
                    new () { Label = "OPUS", Value = "opus"},
                };
            }
            return _CodecOptions;
        }
    }
    /// <summary>
    /// Gets or sets the audio channels for the new track
    /// </summary>
    [DefaultValue(2f)]
    [Select(nameof(ChannelsOptions), 2)]
    public float Channels { get; set; }

    private static List<ListOption> _ChannelsOptions;
    /// <summary>
    /// Gets the channel options
    /// </summary>
    public static List<ListOption> ChannelsOptions
    {
        get
        {
            if (_ChannelsOptions == null)
            {
                _ChannelsOptions = new List<ListOption>
                {
                    new () { Label = "Same as source", Value = 0},
                    new () { Label = "Mono", Value = 1f},
                    new () { Label = "Stereo", Value = 2f},
                    new () { Label = "5.1", Value = 6},
                    new () { Label = "7.1", Value = 8}
                };
            }
            return _ChannelsOptions;
        }
    }
    /// <summary>
    /// Gets or sets the bitrate
    /// </summary>
    [Select(nameof(BitrateOptions), 3)]
    public int Bitrate { get; set; }
    
    /// <summary>
    /// Gets or sets if the bitrate specified should be per channel
    /// </summary>
    [Boolean(4)]
    public bool BitratePerChannel { get; set; }

    private static List<ListOption> _BitrateOptions;
    /// <summary>
    /// Gets the background bitrate options
    /// </summary>
    public static List<ListOption> BitrateOptions
    {
        get
        {
            if (_BitrateOptions == null)
            {
                _BitrateOptions = new List<ListOption>
                {
                    new() { Label = "Automatic", Value = 0},
                    new() { Label = "Same as source", Value = 1}
                };
                for (int i = 64; i <= 2048; i += 32)
                {
                    _BitrateOptions.Add(new ListOption { Label = i + " Kbps", Value = i });
                }
            }
            return _BitrateOptions;
        }
    }
    
    
    /// <summary>
    /// Gets or sets the sample rate
    /// </summary>
    [DefaultValue(0)]
    [Select(nameof(SampleRateOptions), 5)]
    public int SampleRate { get; set; }

    private static List<ListOption> _SampleRateOptions;
    /// <summary>
    /// Gets the sample rate options
    /// </summary>
    public static List<ListOption> SampleRateOptions
    {
        get
        {
            if (_SampleRateOptions == null)
            {
                _SampleRateOptions = new List<ListOption>
                {
                    new () { Label = "Automatic", Value = 0},
                    new () { Label = "Same as source", Value = 1},
                    new () { Label = "44100", Value = 44100 },
                    new () { Label = "48000", Value = 48000 },
                    new () { Label = "88200", Value = 88200 },
                    new () { Label = "96000", Value = 96000 },
                    new () { Label = "176400", Value = 176400 },
                    new () { Label = "192000", Value = 192000 }
                };
            }
            return _SampleRateOptions;
        }
    }
    
    /// <summary>
    /// Gets or sets the language of the new track
    /// </summary>
    [DefaultValue("eng")]
    [TextVariable(6)]
    public string Language { get; set; }
    /// <summary>
    /// Gets or sets if the title of the new track should be removed
    /// </summary>
    [Boolean(7)]
    public bool RemoveTitle { get; set; }
    /// <summary>
    /// Gets or sets the title of the new track
    /// </summary>
    [TextVariable(8)]
    [ConditionEquals(nameof(RemoveTitle), false)]
    public string NewTitle { get; set; }
    
    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output node to execute next</returns>
    public override int Execute(NodeParameters args)
    {
        if (string.IsNullOrEmpty(Codec) || Codec == "ORIGINAL")
        {
            // this is a special case we use in the templates, to not add an audio track and use original
            return 1;
        }
        var audio = new FfmpegAudioStream();

        var bestAudio = GetBestAudioTrack(args, Model.AudioStreams.Select(x => x.Stream));
        if (bestAudio == null)
        {
            args.Logger.WLog("No source audio track found");
            return -1;
        }

        audio.Stream = bestAudio;
        audio.Channels = audio.Stream.Channels;

        bool directCopy = false;
        if(bestAudio.Codec.ToLower() == this.Codec.ToLower())
        {
            if((Channels == 0 || Channels == bestAudio.Channels) && Bitrate <= 2)
            {
                directCopy = true;
            }
        }

        if (directCopy)
        {
            audio.Codec = bestAudio.Codec;
            args.Logger?.ILog($"Source audio is already in appropriate format, just copying that track: {bestAudio.IndexString}, Channels: {bestAudio.Channels}, Codec: {bestAudio.Codec}");
        }
        else
        {
            audio.Codec = Codec;
            int sampleRate = SampleRate == 1 ? audio.Stream.SampleRate : SampleRate;

            int bitrate = Bitrate;
            if (BitratePerChannel)
            {
                int totalChannels = GetAudioBitrateChannels(audio);
                args.Logger?.ILog("Total channels: " + totalChannels);
                args.Logger?.ILog("Bitrate Per Channel: " + bitrate);
                
                bitrate = totalChannels * bitrate;
                args.Logger?.ILog("Total Bitrate: " + bitrate);
            }

            audio.EncodingParameters.AddRange(GetNewAudioTrackParameters(args, audio, Codec, Channels, bitrate, sampleRate));
            if (this.Channels > 0)
                audio.Channels = this.Channels;
        }

        if (RemoveTitle)
            audio.Title = FfmpegStream.REMOVED;
        else if(string.IsNullOrWhiteSpace(NewTitle) == false)
            audio.Title = args.ReplaceVariables(NewTitle, stripMissing: true);

        if (Index > Model.AudioStreams.Count - 1)
            Model.AudioStreams.Add(audio);
        else 
            Model.AudioStreams.Insert(Math.Max(0, Index), audio);

        return 1;
    }

    /// <summary>
    /// Gets how many channels there are including sub channels, eg. 5.1 is 6 channels
    /// </summary>
    /// <param name="audio">the audio track</param>
    /// <returns>the number of channels for the bitrate calculation</returns>
    private int GetAudioBitrateChannels(FfmpegAudioStream audio)
    {
        float channels = (Channels > 0 ? Channels : audio.Channels);
        
        // Check if there are any decimal parts in the channels
        float decimalPart = channels - (int)channels;

        // Calculate the additional channels based on the decimal part
        int additionalChannels = (int)(decimalPart * 10);

        // Total channels including sub-channels
        int totalChannels = (int)channels + additionalChannels;

        return totalChannels;
    }

    /// <summary>
    /// Gets the best audio track
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <param name="streams">the possible audio streams</param>
    /// <returns>the best stream</returns>
    internal AudioStream GetBestAudioTrack(NodeParameters args, IEnumerable<AudioStream> streams)
    {
        Regex? rgxLanguage = null;
        string language = args.ReplaceVariables(this.Language ?? string.Empty, stripMissing: true) ?? string.Empty;
        try
        {
            rgxLanguage = new Regex(language, RegexOptions.IgnoreCase);
        }
        catch (Exception) { }
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
        var bestAudio = streams.Where(x => System.Text.Json.JsonSerializer.Serialize(x).ToLower().Contains("commentary") == false)
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
        .OrderBy(x =>
        {
            if (language != string.Empty)
            {
                args.Logger?.ILog("Language: " + x.Language, x);
                if (string.IsNullOrEmpty(x.Language))
                    return 50; // no language specified
                if (rgxLanguage != null && rgxLanguage.IsMatch(x.Language))
                    return 0;
                if (x.Language.ToLower() != language)
                    return 100; // low priority not the desired language
            }
            return 0;
        })
        .ThenByDescending(x => {
            if(this.Channels == 2)
            {
                if (x.Channels == 2)
                    return 1_000_000_000;
                // compare codecs
                if (x.Codec?.ToLower() == this.Codec?.ToLower())
                    return 1_000_000;
            }
            if(this.Channels == 1)
            {
                if (x.Channels == 1)
                    return 1_000_000_000;
                if (x.Channels <= 2.1f)
                    return 5_000_000;
                if (x.Codec?.ToLower() == this.Codec?.ToLower())
                    return 1_000_000;
            }

            // now we want best channels, but to prefer matching codec
            if (x.Codec?.ToLower() == this.Codec?.ToLower())
            {
                return 1_000 + x.Channels;
            }
            return x.Channels;
        })
        .ThenBy(x => x.Index)
        .FirstOrDefault();
        return bestAudio;
    }

    /// <summary>
    /// Gets hte new audio track parameters
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <param name="stream">the input stream</param>
    /// <param name="codec">the codec of the new track</param>
    /// <param name="channels">the channels of the new track</param>
    /// <param name="bitrate">the bitrate of the new track</param>
    /// <param name="sampleRate">the sample rate</param>
    /// <returns>the new track parameters</returns>
    internal static string[] GetNewAudioTrackParameters(NodeParameters args, FfmpegAudioStream stream, string codec, float channels, int bitrate, int sampleRate)
    {
        bool opus = codec == "opus"; 
        if (opus)
            codec = "libopus";
        
        // Prepare the options list
        var options = new List<string>
        {
            "-map", "0:a:{sourceTypeIndex}",
            "-c:a:{index}",
            codec
        };

        // Handle channels
        if (channels > 0)
        {
            options.Add("-ac:a:{index}");
            options.Add(channels.ToString());
        }
        else if (opus)
        {
            // FF-1016: Opus needs this for side by side channel layout
            args.Logger?.ILog("OPUS Audio: " + stream.Channels);
            if (Math.Abs(stream.Channels - 61) < 1)
            {
                args.Logger?.ILog("Channels 61 detected setting to 8");
                options.AddRange(new[] { "-ac:a:{index}", "8" });
            }
            else if (stream.Channels is > 5 and <= 6 || Math.Abs(stream.Channels - 51) < 1)
            {
                args.Logger?.ILog("Channels between 5 and 6 or 50/51 detected setting to 6");
                options.AddRange(new[] { "-ac:a:{index}", "6" });
            }
            else if (stream.Channels is >= 4 and <= 5 || Math.Abs(stream.Channels - 40) < 2)
            {
                args.Logger?.ILog("Channels between 4 and 5 or 40/41 detected setting to 4");
                options.AddRange(new[] { "-ac:a:{index}", "4" });
            }
            else if (stream.Channels == 0)
            {
                args.Logger?.ILog("No channels detected setting to 2");
                options.AddRange(new[] { "-ac:a:{index}", "2" });
            }
            else if (Math.Abs(stream.Channels - 5) < 0.5)
            {
                args.Logger?.ILog("Channels 5 detected setting to 5");
                options.AddRange(new[] { "-ac:a:{index}", "5" });
            }
        }

        // Handle bitrate
        if (bitrate == 1)
        {
            // use same bitrate as the source
            // but only if that source HAS a bitrate detected
            if (stream.Stream.Bitrate > 0)
            {
                options.Add("-b:a:{index}");
                options.Add((stream.Stream.Bitrate / 1000) + "k");
                options.Add("-metadata:s:a:{index}");
                options.Add($"BPS={stream.Stream.Bitrate}");
            }
        }
        else if (bitrate > 0)
        {
            options.Add("-b:a:{index}");
            options.Add(bitrate + "k");
            options.Add("-metadata:s:a:{index}");
            options.Add($"BPS={bitrate * 1000}");
        }

        // Handle sample rate
        if (sampleRate > 0)
        {
            options.Add("-ar:a:{index}");
            options.Add(sampleRate.ToString());
        }
        
        args.Logger.ILog("New Audio Arguments: " + string.Join(" ", options));

        return options.ToArray();
    }
}
