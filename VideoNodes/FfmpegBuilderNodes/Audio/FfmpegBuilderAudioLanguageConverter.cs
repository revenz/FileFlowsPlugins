using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using FileFlows.VideoNodes.Helpers;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Converts/adds audio tracks based on a language
/// </summary>
public class FfmpegBuilderAudioLanguageConverter : FfmpegBuilderNode
{
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/audio-language-converter";
    /// <inheritdoc />
    public override string Icon => "fas fa-comment-dots";
    /// <inheritdoc />
    public override int Outputs => 2;
    
    /// <summary>
    /// Gets or sets the languages to create audio tracks for
    /// </summary>
    [Languages(1)]
    [Required]
    public string[] Languages { get; set; }
    
    /// <summary>
    /// Gets or sets the codec to use
    /// </summary>
    [DefaultValue("aac")]
    [Select(nameof(CodecOptions), 2)]
    public string Codec { get; set; }

    /// <summary>
    /// The available codec options
    /// </summary>
    private static List<ListOption> _CodecOptions;
    /// <summary>
    /// Gets the available codec options
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
                    new () { Label = "DTS", Value = "dts" },
                    new () { Label = "FLAC", Value ="flac" },
                    new () { Label = "OPUS", Value = "opus"}
                };
            }
            return _CodecOptions;
        }
    }

    /// <summary>
    /// Gets or sets the number of channels for the converted audio
    /// </summary>
    [DefaultValue(0)]
    [Select(nameof(ChannelsOptions), 3)]
    public float Channels { get; set; }
    /// <summary>
    /// The channel options
    /// </summary>
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
    [Select(nameof(BitrateOptions), 4)]
    public int Bitrate { get; set; }

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
    /// Gets or sets if the other audio tracks should be removed
    /// </summary>
    [Boolean(5)]
    public bool RemoveOthers { get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        List<FfmpegAudioStream> newAudioStreams = [];
        // ensure theres no duplicates, e.g. if OriginalLanguage is english and english is also specified
        var languages = Languages.Select(x =>
        {
            var comparison = x.Replace("{", "").Replace("}", "").Trim();
            if (string.Equals("original", comparison, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals("orig", comparison, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals("originallanguage", comparison, StringComparison.InvariantCultureIgnoreCase))
            {
                args.Logger?.ILog("Attempting to use original language");
                if (args.Variables.TryGetValue("OriginalLanguage", out var oLang) == false || oLang == null)
                {
                    args.Logger?.ILog("Original Language not found in variables");
                    return null;
                }
                
                string oLangStr = oLang.ToString();
                if (string.IsNullOrWhiteSpace(oLangStr))
                {
                    args.Logger?.ILog("Original Language found but was empty");
                    return null;
                }
        
                args.Logger?.ILog("Found original language: " + oLangStr);
                var code = LanguageHelper.GetIso2Code(oLangStr);
                args.Logger?.ILog("Using original language code: " + code);
                return code;
            }
            return x;
        }).Where(x => x != null).Distinct().ToList();
        
        foreach (var lang in languages)
        {
            var newAudio = GetNewAudioStream(args, lang);
            if (newAudio == null)
            {
                args.Logger?.WLog($"Failed to find language '{lang}'");
                continue;
            }

            newAudioStreams.Add(newAudio);
        }

        if (newAudioStreams.Count == 0)
        {
            args.Logger?.ILog("Failed to locate any matching languages to create audio tracks for");
            return 2;
        }

        if (RemoveOthers)
        {
            foreach (var audioStream in Model.AudioStreams)
            {
                audioStream.Deleted = true;
            }
        }
        else
        {
            foreach (var newAudioStream in newAudioStreams)
            {
                int newAudioChannels = ChannelHelper.GetNumberOfChannels(newAudioStream.Channels);
                args.Logger?.ILog($"New Audio '{newAudioStream} channels: {newAudioChannels}");
                // see if the new audio stream is basically the same as the old one
                var existing = Model.AudioStreams.Where(x => x.Stream == newAudioStream.Stream);
                foreach (var ex in existing)
                {
                    int existingChannels = ChannelHelper.GetNumberOfChannels(ex.Channels);
                    args.Logger?.ILog($"Existing Audio '{ex} channels: {existingChannels}");
                    if (existingChannels != newAudioChannels)
                        continue;
                    if (ex.Codec != newAudioStream.Codec && 
                        Regex.IsMatch(ex.Codec ?? string.Empty, "/^(ac3|aac|opus)$", RegexOptions.IgnoreCase))
                        continue;
                    args.Logger?.ILog("Deleting similar audio to newly created one: " + ex);
                    ex.Deleted = true;
                }
            }
        }

        Model.AudioStreams.AddRange(newAudioStreams);
        
        args.Logger?.ILog($"Created {newAudioStreams.Count} new audio streams");
        return 1;

    }
    
    /// <summary>
    /// Gets the new audio stream for the specified language, or null if failed to locate matching stream
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <param name="language">the language to add</param>
    /// <returns>the new stream or null if not found</returns>
    private FfmpegAudioStream? GetNewAudioStream(NodeParameters args, string language)
    {
        var sourceAudio = GetBestAudioTrack(args, Model.AudioStreams.Select(x => x.Stream), language);
        if (sourceAudio == null)
            return null;
        
        args.Logger?.ILog($"Using audio track for language '{language}': {sourceAudio}");

        var audio = new FfmpegAudioStream();
        audio.Stream = sourceAudio;
        audio.Channels = audio.Stream.Channels;

        bool directCopy = false;
        if(string.Equals(sourceAudio.Codec, this.Codec, StringComparison.CurrentCultureIgnoreCase))
        {
            if((Channels == 0 || Math.Abs(Channels - sourceAudio.Channels) < 0.05f) && Bitrate <= 2)
            {
                directCopy = true;
            }
        }
        
        if (directCopy)
        {
            audio.Codec = sourceAudio.Codec;
            args.Logger?.ILog($"Source audio is already in appropriate format, just copying that track: {sourceAudio.IndexString}, Channels: {sourceAudio.Channels}, Codec: {sourceAudio.Codec}");
        }
        else
        {
            audio.Codec = Codec;
            
            int totalChannels = FfmpegBuilderAudioAddTrack.GetAudioBitrateChannels(args.Logger, Channels < 1 ? audio.Channels : Channels, Codec);
            int channels = Channels < 1 ? 0 : totalChannels;

            int bitrate = Bitrate == 1 ? (int)Math.Round(audio.Stream.Bitrate / Math.Max(1, audio.Stream.Channels)) :
                Bitrate == 2 ? totalChannels * Bitrate :
                0;
            
            if (bitrate > 0)
            {
                args.Logger?.ILog("Total channels: " + totalChannels);
                args.Logger?.ILog("Bitrate Per Channel: " + Bitrate);
                args.Logger?.ILog("Total Bitrate: " + bitrate);
            }

            audio.EncodingParameters.AddRange(FfmpegBuilderAudioAddTrack.GetNewAudioTrackParameters(args, audio, Codec, channels, bitrate, 0));
            if (channels > 0)
            {
                args.Logger?.ILog("Setting channels to: " + channels);
                audio.Channels = channels;
            }
        }

        audio.Title = LanguageHelper.GetEnglishFor(language) + (audio.Channels switch
        {
            < 1.9f => " (Mono)",
            < 2.1f => " (Stereo)",
            < 3f => " (2.1)",
            < 6.1f => " (5.1)",
            < 8.1f => " (7.1)",
            _ => $" ({Math.Round(audio.Channels, 1)})"
        });

        return audio;
    }


    /// <summary>
    /// Gets the best audio track
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <param name="streams">the possible audio streams</param>
    /// <returns>the best stream</returns>
    internal AudioStream GetBestAudioTrack(NodeParameters args, IEnumerable<AudioStream> streams, string language)
    {
        float comparingChannels = Channels switch
        {
            8 => 7.1f,
            6 => 5.1f,
            _ => Channels
        };
        var bestAudio = streams
            // only search tracks of the same language 
            .Where(x => LanguageHelper.Matches(args, language, x.Language))
            // only get a track that has more or equal number of channels
            .Where(x => Math.Abs(x.Channels - comparingChannels) < 0.1f || x.Channels >= comparingChannels)
            // remove any commentary tracks
            .Where(x => System.Text.Json.JsonSerializer.Serialize(x).ToLower().Contains("comment") == false)
            .OrderBy(x =>
            {
                if (Math.Abs(this.Channels - 2) < 0.05f)
                {
                    if (Math.Abs(x.Channels - 2) < 0.05f)
                        return 1_000_000_000;
                    // compare codecs
                    if (x.Codec?.ToLower() == this.Codec?.ToLower())
                        return 1_000_000;
                }

                if (Math.Abs(this.Channels - 1) < 0.05f)
                {
                    if (Math.Abs(x.Channels - 1) < 0.05f)
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

}