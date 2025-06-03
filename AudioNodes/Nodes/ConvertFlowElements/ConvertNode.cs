using System.ComponentModel;
using FileFlows.AudioNodes.Helpers;

namespace FileFlows.AudioNodes;

public abstract class ConvertNode:AudioNode
{
    /// <summary>
    /// Gets the default extension to use if none set
    /// </summary>
    protected abstract string DefaultExtension { get; }
    
    /// <summary>
    /// Gets or sets if using high efficiency
    /// </summary>
    protected bool HighEfficiency { get; set; }

    /// <summary>
    /// Gets the bitrate of the source file
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the source bitrate</returns>
    protected long GetSourceBitrate(NodeParameters args)
    {
        var info = GetAudioInfo(args).Value;
        return info.Bitrate;
    }

    /// <summary>
    /// Gets the channels of the source file
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the source channels</returns>
    protected long GetSourceChannels(NodeParameters args)
    {
        var info = GetAudioInfo(args).Value;
        return info.Channels;
    }

    public override int Inputs => 1;
    public override int Outputs => 2;

    protected virtual List<string> GetArguments(NodeParameters args, out string? extension)
    {
        string Codec = DefaultExtension;
        extension = null;
        string codecKey = Codec + "_codec";
        string codec = args.GetToolPathActual(codecKey)?.EmptyAsNull() ?? Codec;
        if (codec.ToLowerInvariant() == "mp3")
        {
            extension = "mp3";
            codec = "mp3";
        }
        else if(codec.ToLowerInvariant() == "alac" || codec.ToLowerInvariant() == "alac_codec") 
            extension = "m4a";
        else if (codec == "libopus")
            extension = "ogg";
        else if (codec == "alac")
            extension = "m4a";
        else if (codec == "libvorbis" || codec == "ogg")
        {
            codec = "libvorbis";
            extension = "ogg";
        }

        bool ogg = extension?.ToLowerInvariant() == "ogg";

        if (codec == codecKey || string.IsNullOrWhiteSpace(codec))
        {
            codec = Codec switch
            {
                "ogg" => "libvorbis",
                "wav" => "pcm_s16le",
                _ => Codec.ToLower()
            };
        }

        int bitrate = Bitrate;
        List<string> ffArgs = new()
        {
            "-c:a",
            codec,
        };
        
        if(bitrate is > 10 and <= 20)
        {
            bool mp3 = Codec.Equals("mp3", StringComparison.InvariantCultureIgnoreCase);
            bool aac = Codec.Equals("aac", StringComparison.InvariantCultureIgnoreCase);
            if(mp3 == false && aac == false && ogg == false)
                throw new Exception("Variable bitrate not supported in codec: " + Codec);
            
            bitrate = (Bitrate - 10);
            if (mp3)
            {
                // ogg is reversed
                bitrate = 10 - bitrate;
            }

            args.Logger?.ILog($"Using variable bitrate setting '{bitrate}' for codec '{Codec}'");

            if (codec == "libfdk_aac")
            {
                ffArgs.AddRange(new[]
                {
                    "-vbr",
                    Math.Min(Math.Max(1, bitrate / 2), 5).ToString()
                });
            }
            else
            {
                ffArgs.AddRange(new[]
                {
                    "-qscale:a",
                    bitrate.ToString()
                });
            }

            if (Codec == "aac" && HighEfficiency)
            {
                extension = "m4a";
                if(Channels is > 0 and <= 2)
                    ffArgs.AddRange(new[] { "-profile:a", "aac_he_v2" });
                else if(Channels > 0)
                    ffArgs.AddRange(new[] { "-profile:a", "aac_he" });
                else if(GetSourceChannels(args) <= 2)
                    ffArgs.AddRange(new[] { "-profile:a", "aac_he_v2" });
                else
                    ffArgs.AddRange(new[] { "-profile:a", "aac_he" });
            }
        }
        else if (bitrate != 0)
        {
            ffArgs.AddRange(new []
            {
                "-ab",
                (bitrate == -1 ? GetSourceBitrate(args).ToString() : bitrate + "k")
            });
            if (Codec == "aac" && HighEfficiency)
            {
                extension = "m4a";
                if(Channels is > 0 and <= 2)
                    ffArgs.AddRange(new[] { "-profile:a", "aac_he_v2" });
                else if(Channels > 0)
                    ffArgs.AddRange(new[] { "-profile:a", "aac_he" });
                else if(GetSourceChannels(args) <= 2)
                    ffArgs.AddRange(new[] { "-profile:a", "aac_he_v2" });
                else
                    ffArgs.AddRange(new[] { "-profile:a", "aac_he" });
            }
        }

        if (SampleRate > 0)
        {
            ffArgs.Add("-ar");
            ffArgs.Add(SampleRate.ToString());
        }

        if (Channels > 0)
        {
            ffArgs.Add("-ac");
            ffArgs.Add(Channels.ToString());
        }

        return ffArgs;
    }

    /// <summary>
    /// Gets the type of flow element
    /// </summary>
    public override FlowElementType Type => FlowElementType.Process;

    /// <summary>
    /// Gets or sets the bitrate for the converted audio
    /// </summary>
    [Select(nameof(BitrateOptions), 1)]
    public int Bitrate { get; set; }
    
    /// <summary>
    /// Gets or sets the sample rate
    /// </summary>
    [DefaultValue(0)]
    [Select(nameof(SampleRateOptions), 2)]
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
    /// Gets or sets the number of channels rate
    /// </summary>
    [DefaultValue(0)]
    [Select(nameof(ChannelsOptions), 3)]
    public int Channels { get; set; }
    
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
    /// Gets or sets a custom extension to override the ont to use
    /// </summary>
    [TextVariable(4)]
    public string CustomExtension { get; set; }
    
    /// <summary>
    /// Gets or sets if the audio should be normalized
    /// </summary>
    [Boolean(5)]
    public bool Normalize { get; set; }

    /// <summary>
    /// Gets or sets if it should be skipped if the codec is the same
    /// </summary>
    [Boolean(6)]
    [ConditionEquals(nameof(Normalize), true, inverse: true)]
    public bool SkipIfCodecMatches { get; set; }

    private static List<ListOption> _BitrateOptions;

    /// <summary>
    /// Gets the bitrate options to show to the user
    /// </summary>
    public static List<ListOption> BitrateOptions
    {
        get
        {
            if (_BitrateOptions == null)
            {
                _BitrateOptions = new List<ListOption>
                {
                    new () { Label = "Automatic", Value = 0 },
                    new () { Label = "Same as source", Value = -1 },
                    
                    new () { Label = "Constant Bitrate", Value = "###GROUP###" },
                    new () { Label = "32 Kbps", Value = 32},
                    new () { Label = "64 Kbps", Value = 64},
                    new () { Label = "96 Kbps", Value = 96},
                    new () { Label = "128 Kbps", Value = 128},
                    new () { Label = "160 Kbps", Value = 160},
                    new () { Label = "192 Kbps", Value = 192},
                    new () { Label = "224 Kbps", Value = 224},
                    new () { Label = "256 Kbps", Value = 256},
                    new () { Label = "288 Kbps", Value = 288},
                    new () { Label = "320 Kbps", Value = 320},
                    
                    new () { Label = "Variable Bitrate", Value = "###GROUP###" },
                    new () { Label = "0 (Lowest Quality)", Value = 10},
                    new () { Label = "1", Value = 11},
                    new () { Label = "2", Value = 12},
                    new () { Label = "3", Value = 13},
                    new () { Label = "4", Value = 14},
                    new () { Label = "5 (Good Quality)", Value = 15},
                    new () { Label = "6", Value = 16},
                    new () { Label = "7", Value = 17},
                    new () { Label = "8", Value = 18},
                    new () { Label = "9", Value = 19},
                    new () { Label = "10 (Highest Quality)", Value = 20},
                    
                };
            }
            return _BitrateOptions;
        }
    }

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        var aiResult = GetAudioInfo(args);
        if (aiResult.Failed(out string error))
        {
            args.FailureReason = error;
            args.Logger.ELog(error);
            return -1;
        }

        AudioInfo AudioInfo = aiResult.Value;
        
        var ffmpegExeResult = GetFFmpeg(args);
        if (ffmpegExeResult.Failed(out string ffmpegError))
        {
            args.FailureReason = ffmpegError;
            args.Logger?.ELog(ffmpegError);
            return -1;
        }
        string ffmpegExe = ffmpegExeResult.Value;

        var ffprobeResult = GetFFprobe(args);
        if (ffprobeResult.Failed(out string ffprobeError))
        {
            args.FailureReason = ffprobeError;
            args.Logger?.ELog(ffprobeError);
            return -1;
        }
        string ffprobe = ffprobeResult.Value;

        if(Normalize == false && AudioInfo.Codec?.ToLower() == DefaultExtension?.ToLower())
        {
            if (SkipIfCodecMatches)
            {
                args.Logger?.ILog($"Audio file already '{DefaultExtension}' at bitrate '{AudioInfo.Bitrate} bps', and set to skip if codec matches");
                return 2;
            }

            args.Logger?.ILog($"Comparing bitrate {AudioInfo.Bitrate} is less than or equal to {(Bitrate * 1000)}");
            if(AudioInfo.Bitrate <= Bitrate * 1000) // this bitrate is in Kbps, whereas AudioInfo.Bitrate is bytes per second
            {
                args.Logger?.ILog($"Audio file already '{DefaultExtension}' at bitrate '{AudioInfo.Bitrate} bps ({(AudioInfo.Bitrate / 1000)} KBps)'");
                return 2;
            }
            if(AudioInfo.Bitrate <= Bitrate * 1024) // this bitrate is in Kbps, whereas AudioInfo.Bitrate is bytes per second
            {
                args.Logger?.ILog($"Audio file already '{DefaultExtension}' at bitrate '{AudioInfo.Bitrate} bps ({(AudioInfo.Bitrate / 1024)} KBps)'");
                return 2;
            }
        }


        var ffArgs = GetArguments(args, out var extension);
        var actualExt = args.ReplaceVariables(CustomExtension, stripMissing: true)?.EmptyAsNull() ??
                        extension?.EmptyAsNull() ?? DefaultExtension;
        var outputFile = FileHelper.Combine(args.TempPath, Guid.NewGuid() + "." + actualExt.TrimStart('.'));
        
        ffArgs.Insert(0, "-hide_banner");
        ffArgs.Insert(1, "-y"); // tells ffmpeg to replace the file if already exists, which it shouldnt but just incase
        ffArgs.Insert(2, "-i");
        ffArgs.Insert(3, LocalWorkingFile);
        ffArgs.Insert(4, "-vn"); // disables video


        if (Normalize)
        {
            var twoPass =  AudioFileNormalization.DoTwoPass(args, ffmpegExe, LocalWorkingFile);
            if (twoPass.Success)
            {
                ffArgs.Add("-af");
                ffArgs.Add(twoPass.Normalization);
            }
        }
        
        var metadata = MetadataHelper.GetMetadataParameters(AudioInfo);
        if (metadata?.Any() == true)
            ffArgs.AddRange(metadata);

        ffArgs.Add(outputFile);

        args.Logger?.ILog("FFArgs: " + string.Join(" ", ffArgs.Select(x => x.IndexOf(" ") > 0 ? "\"" + x + "\"" : x).ToArray()));

        var result = args.Execute(new ExecuteArgs
        {
            Command = ffmpegExe,
            ArgumentList = ffArgs.ToArray()
        });

        if(result.ExitCode != 0)
        {
            args.Logger?.ELog("Invalid exit code detected: " + result.ExitCode);
            return -1;
        }

        args.SetWorkingFile(outputFile);

        // update the Audio file info
        if (ReadAudioFileInfo(args, ffmpegExe, ffprobe, args.WorkingFile))
            return 1;

        return -1;
    }
}
