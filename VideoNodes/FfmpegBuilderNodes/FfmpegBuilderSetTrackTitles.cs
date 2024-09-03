namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// FFmpeg Builder flow element to set a tracks title
/// </summary>
public class FfmpegBuilderSetTrackTitles: FfmpegBuilderNode
{
    /// <summary>
    /// Gets the help URL for the flow element
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/set-track-titles";

    /// <summary>
    /// Gets the number of outputs of the flow element
    /// </summary>
    public override int Outputs => 2;

    /// <summary>
    /// Gets the icon of the flow element
    /// </summary>
    public override string Icon => "fas fa-heading";

    /// <summary>
    /// Gets or sets the stream type
    /// </summary>
    [Select(nameof(StreamTypeOptions), 1)]
    public string StreamType { get; set; }

    private static List<ListOption> _StreamTypeOptions;
    /// <summary>
    /// Gets the stream options to show in the UI
    /// </summary>
    public static List<ListOption> StreamTypeOptions
    {
        get
        {
            if (_StreamTypeOptions == null)
            {
                _StreamTypeOptions = new List<ListOption>
                {
                    new () { Label = "Audio", Value = "Audio" },
                    new () { Label = "Subtitle", Value = "Subtitle" },
                    new () { Label = "Audio and Subtitle", Value = "Both" },
                    new () { Label = "Video" , Value = "Video"}
                };
            }
            return _StreamTypeOptions;
        }
    }
    
    /// <summary>
    /// Gets or sets the format for the titles
    /// </summary>
    [Text(2)]
    [DefaultValue("lang / codec / channels")]
    public string Format { get; set; }
    
    /// <summary>
    /// Gets or sets the separator used between fields
    /// </summary>
    [Text(3)]
    [DefaultValue(" / ")]
    public string Separator { get; set; }
    
    /// <summary>
    /// Gets or sets if commentary should be left alone
    /// </summary>
    [Select(nameof(CommentaryFormatOptions), 4)]
    [DefaultValue("Commentary")]
    [ConditionEquals(nameof(StreamType), "Video", inverse: true)]
    public string CommentaryFormat { get; set; }

    private static List<ListOption> _CommentaryFormatOptions;
    /// <summary>
    /// Gets the commentary format options to show in the UI
    /// </summary>
    public static List<ListOption> CommentaryFormatOptions
    {
        get
        {
            if (_CommentaryFormatOptions == null)
            {
                _CommentaryFormatOptions = new List<ListOption>
                {
                    new () { Label = "Same as format", Value = "" },
                    new () { Label = "Leave Alone", Value = "Leave" },
                    new () { Label = "Set to 'Commentary'", Value = "Commentary" },
                };
            }
            return _CommentaryFormatOptions;
        }
    }

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the flow parameters</param>
    /// <returns>the flow output to call next</returns>
    public override int Execute(NodeParameters args)
    {
        int changes = 0;
        if (string.IsNullOrEmpty(StreamType) || StreamType is "Audio" or "Both")
        {
            foreach(var track in Model.AudioStreams)
            {
                bool isCommentary = IsCommentary(track.Title, track.Language)
                    ||  IsCommentary(track.Stream?.Title, track.Stream?.Language);


                string originalTitle = track.Title;
                if (isCommentary && string.IsNullOrWhiteSpace(CommentaryFormat) == false)
                {
                    if (CommentaryFormat == "Leave")
                        continue;
                    track.Title = "Commentary";
                }
                else
                {

                    track.Title = FormatTitle(Format, Separator,
                        track.Language?.EmptyAsNull() ?? track.Stream?.Language,
                        track.Codec?.EmptyAsNull() ?? track.Stream?.Codec,
                        track.IsDefault,
                        track.Stream?.Bitrate ?? 0,
                        track.Channels > 0 ? track.Channels : track.Stream?.Channels ?? 0,
                        track.Stream?.SampleRate ?? 0
                    );
                }

                if (originalTitle == track.Title)
                    continue;
                args.Logger?.ILog($"Title changed from '{(originalTitle ?? string.Empty)}' to '{track.Title}'");
                track.ForcedChange = true;
                ++changes;
            }
        }
        
        if (StreamType is "Subtitle" or "Both")
        {
            foreach(var track in Model.SubtitleStreams)
            {
                bool isCommentary = IsCommentary(track.Title, track.Language)
                                    ||  IsCommentary(track.Stream?.Title, track.Stream?.Language);
                
                bool isHearingImpaired = ContainsHearingImpared(track.Title) || ContainsHearingImpared(track.Language) || 
                                         ContainsHearingImpared(track.Stream?.Title) || ContainsHearingImpared(track.Stream?.Language);
                bool isCC = ContainsClosedCaptions(track.Title) || ContainsClosedCaptions(track.Language) || 
                            ContainsClosedCaptions(track.Stream?.Title) || ContainsClosedCaptions(track.Stream?.Language);
                bool isSDH = ContainsSDH(track.Title) || ContainsSDH(track.Language) || 
                             ContainsSDH(track.Stream?.Title) || ContainsSDH(track.Stream?.Language);

                string originalTitle = track.Title;
                if (isCommentary && string.IsNullOrWhiteSpace(CommentaryFormat) == false)
                {
                    if (CommentaryFormat == "Leave")
                        continue;
                    track.Title = "Commentary";
                }
                else
                {
                    track.Title = FormatTitle(Format, Separator,
                        track.Language?.EmptyAsNull() ?? track.Stream?.Language,
                        track.Codec?.EmptyAsNull() ?? track.Stream?.Codec,
                        track.IsDefault,
                        track.Stream?.Bitrate ?? 0,
                        0,
                        0,
                        track.IsForced,
                        sdh: isSDH,
                        hi: isHearingImpaired,
                        cc: isCC
                    );
                }

                if (originalTitle == track.Title)
                    continue;
                args.Logger?.ILog($"Title changed from '{(originalTitle ?? string.Empty)}' to '{track.Title}'");
                track.ForcedChange = true;
                ++changes;
            }
        }

        if (StreamType is "Video")
        {
            foreach (var track in Model.VideoStreams)
            {
                string originalTitle = track.Title;
                track.Title = FormatTitle(Format, Separator,
                    track.Language?.EmptyAsNull(),
                    track.Codec?.EmptyAsNull() ?? track.Stream?.Codec,
                    bitrate: track.Stream?.Bitrate ?? 0,
                    fps: track.Stream?.FramesPerSecond ?? 0,
                    pixelFormat: track.Stream?.PixelFormat?.EmptyAsNull(),
                    resolution: track.Stream == null ? null : ResolutionHelper.GetResolution(track.Stream.Width, track.Stream.Height),
                    dimensions: track.Stream == null ? null : track.Stream.Width + "x" + track.Stream.Height
                );
                if (originalTitle == track.Title)
                    continue;
                args.Logger?.ILog($"Title changed from '{(originalTitle ?? string.Empty)}' to '{track.Title}'");
                track.ForcedChange = true;
                ++changes;
            }
        }

        if (changes == 0)
        {
            args.Logger?.ILog("No changes made");
            return 2;
        }
        args.Logger?.ILog("Changes were made");
        return 1;
    }

    /// <summary>
    /// Tests if a title is a commentary track
    /// </summary>
    /// <param name="title">the title</param>
    /// <param name="title">the language</param>
    /// <returns>true if commentary</returns>
    private static bool IsCommentary(string title, string language)
        => title?.ToLowerInvariant()?.Contains("comment") == true || language?.ToLowerInvariant()?.Contains("comment") == true;

    /// <summary>
    /// Checks if the input string contains "HI" or "Hearing Impaired" in various formats.
    /// </summary>
    /// <param name="input">The string to be checked.</param>
    /// <returns>True if the string contains "HI" or "Hearing Impaired", false otherwise.</returns>
    public static bool ContainsHearingImpared(string input)
        => string.IsNullOrWhiteSpace(input) == false && Regex.IsMatch(input.ToUpper(), @"\bHI\b|\bHEARING IMPAIRED\b");
    
    /// <summary>
    /// Checks if the input string contains "SDH" in various formats.
    /// </summary>
    /// <param name="input">The string to be checked.</param>
    /// <returns>True if the string contains "SH", false otherwise.</returns>
    public static bool ContainsSDH(string input)
        => string.IsNullOrWhiteSpace(input) == false && Regex.IsMatch(input.ToUpper(), @"\SDH\b");
    
    /// <summary>
    /// Checks if the input string contains "CC" or "Closed Captions" in various formats.
    /// </summary>
    /// <param name="input">The string to be checked.</param>
    /// <returns>True if the string contains "CC", false otherwise.</returns>
    public static bool ContainsClosedCaptions(string input)
        => string.IsNullOrWhiteSpace(input) == false && Regex.IsMatch(input.ToUpper(), @"\bCC\b|\bCLOSED CAPTIONS\b");
    
    /// <summary>
    /// Formats a string for the title
    /// </summary>
    /// <param name="formatter">the string formatting to use</param>
    /// <param name="separator">the separator character that is used</param>
    /// <param name="language">the language of the track</param>
    /// <param name="codec">the codec of the track</param>
    /// <param name="isDefault">if the track is the default or not</param>
    /// <param name="bitrate">the bitrate</param>
    /// <param name="channels">the number of channels</param>
    /// <param name="sampleRate">the sample rate</param>
    /// <param name="isForced">if the track is forced or not</param>
    /// <param name="sdh">if the track is SDH</param>
    /// <param name="cc">if the track is closed captions</param>
    /// <param name="hi">if the track is hearing impared</param>
    /// <param name="resolution">the tracks video resolution</param>
    /// <param name="dimensions">the tracks video dimensions</param>
    /// <param name="pixelFormat">the tracks video pixelFormat</param>
    /// <param name="fps">the tracks video FPS</param>
    /// <returns>the formatted string</returns>
    internal static string FormatTitle(string formatter, string separator, string language, string codec, bool isDefault = false, float bitrate = 0,
        float channels = 0, int sampleRate = 0, bool isForced = false, bool sdh = false, bool cc = false, bool hi = false,
        ResolutionHelper.Resolution? resolution = null, string? dimensions = null, string? pixelFormat = null, float? fps = null)
    {
        if (string.IsNullOrWhiteSpace(formatter))
            return string.Empty;

        string english = LanguageHelper.GetEnglishFor(language) ?? language ?? string.Empty;
        string iso2 = LanguageHelper.GetIso2Code(language) ?? language ?? string.Empty;
        string iso1 = LanguageHelper.GetIso1Code(language) ?? language ?? string.Empty;
        formatter = Replace(formatter, "lang!", english.ToUpperInvariant());
        formatter = Replace(formatter, "lang-iso2!", iso2.ToUpperInvariant());
        formatter = Replace(formatter, "lang-iso1!", iso1.ToUpperInvariant());
        formatter = Replace(formatter, "!lang", english.ToLowerInvariant());
        formatter = Replace(formatter, "!lang-iso2", iso2.ToLowerInvariant());
        formatter = Replace(formatter, "!lang-iso1", iso1.ToLowerInvariant());
        formatter = Replace(formatter, "lang", english);
        formatter = Replace(formatter, "lang-iso2", iso2);
        formatter = Replace(formatter, "lang-iso1", iso1);

        var codecCommericalName = GetCodecCommercialName(codec);
        formatter = Replace(formatter, "!codec-cc", codecCommericalName.ToLowerInvariant());
        formatter = Replace(formatter, "codec-cc!", codecCommericalName.ToUpperInvariant());
        formatter = Replace(formatter, "codec-cc", codecCommericalName);
        
        formatter = Replace(formatter, "!codec", codec.ToLowerInvariant());
        formatter = Replace(formatter, "!codec!", codec);
        formatter = Replace(formatter, "codec", codec.ToUpperInvariant());
        
        formatter = Replace(formatter, "default", isDefault ? "Default" : string.Empty);
        formatter = Replace(formatter, "forced", isForced ? "Forced" : string.Empty);
        formatter = Replace(formatter, "cc", cc ? "CC" : string.Empty);
        formatter = Replace(formatter, "closedcaptions", cc ? "Closed Captions" : string.Empty);
        formatter = Replace(formatter, "hi", hi ? "HI" : string.Empty);
        formatter = Replace(formatter, "hearingimpared", hi ? "Hearing Impared" : string.Empty);
        formatter = Replace(formatter, "sdh", sdh ? "SDH" : string.Empty);
        formatter = Replace(formatter, "channels", Math.Abs(channels - 1) < 0.05f ? "Mono" :
            Math.Abs(channels - 2) < 0.05f ? "Stereo" :
            channels > 0 ? channels.ToString("0.0") : null);
        
        formatter = Replace(formatter, "bitrate", bitrate < 1 ? null : ((bitrate / 1000f).ToString("0.0").Replace(".0", string.Empty) + "Kbps"));
        formatter = Replace(formatter, "samplerate", sampleRate < 1 ? null : ((sampleRate / 1000f).ToString("0.0").Replace(".0", string.Empty) + "kHz"));
        formatter = Replace(formatter, "sample-rate", sampleRate < 1 ? null : ((sampleRate / 1000f).ToString("0.0").Replace(".0", string.Empty) + "kHz"));
        formatter = Replace(formatter, "sample", sampleRate < 1 ? null : ((sampleRate / 1000f).ToString("0.0").Replace(".0", string.Empty) + "kHz"));

        if(formatter.Contains("!fps")) // have to use this, since we're adding "fps" to the string, and without the else, the "fps" we added would be replaced
            formatter = Replace(formatter, "!fps", fps > 0 ? fps + "fps" : string.Empty);
        else
            formatter = Replace(formatter, "fps", fps > 0 ? fps + "FPS" : string.Empty);
        
        formatter = Replace(formatter, "!resolution", resolution switch
        {
            ResolutionHelper.Resolution.r4k => "4k",
            ResolutionHelper.Resolution.r1440p => "1440p",
            ResolutionHelper.Resolution.r1080p => "1080p",
            ResolutionHelper.Resolution.r720p => "720p",
            ResolutionHelper.Resolution.r480p => "480p",
            _ => "" 
        });
        formatter = Replace(formatter, "resolution", resolution switch
        {
            ResolutionHelper.Resolution.r4k => "4K",
            ResolutionHelper.Resolution.r1440p => "1440P",
            ResolutionHelper.Resolution.r1080p => "1080P",
            ResolutionHelper.Resolution.r720p => "720P",
            ResolutionHelper.Resolution.r480p => "480P",
            _ => "" 
        });
        formatter = Replace(formatter, "!pixelformat!", pixelFormat ?? string.Empty);
        formatter = Replace(formatter, "!pixelformat", pixelFormat?.ToLowerInvariant() ?? string.Empty);
        formatter = Replace(formatter, "pixelformat", pixelFormat?.ToUpperInvariant() ?? string.Empty);
        formatter = Replace(formatter, "dimensions", dimensions ?? string.Empty);
        
        // Remove standalone separators
        if(string.IsNullOrWhiteSpace(separator.Trim()) == false)
        {
            int count = 0;
            while (formatter.EndsWith(separator) && ++count < 100)
                formatter = formatter[..^separator.Length];
            count = 0;
            while (formatter.StartsWith(separator) && ++count < 100)
                formatter = formatter[separator.Length..];
            count = 0;
            while (formatter.Contains(separator + separator) && ++count < 100)
            {
                formatter = formatter.Replace((separator + separator), separator);
                formatter = formatter.Replace("  ", " ");
            }
            count = 0;
            while (formatter.EndsWith(separator.TrimEnd()) && ++count < 100)
                formatter = formatter[..^separator.TrimEnd().Length];
            count = 0;
            while (formatter.StartsWith(separator.TrimStart()) && ++count < 100)
                formatter = formatter[separator.TrimStart().Length..];
            count = 0;
            while (formatter.Contains((separator + separator).TrimStart()) && ++count < 100)
            {
                formatter = formatter.Replace((separator + separator).TrimStart(), separator);
                formatter = formatter.Replace("  ", " ");
            }
            count = 0;
            while (formatter.Contains((separator + separator).TrimEnd()) && ++count < 100)
            {
                formatter = formatter.Replace((separator + separator).TrimEnd(), separator);
                formatter = formatter.Replace("  ", " ");
            }
            // Handle whitespace around two separators
            string pattern = $@"{Regex.Escape(separator.Trim())}\s*{Regex.Escape(separator.Trim())}";
            count = 0;
            while (Regex.IsMatch(formatter, pattern) && ++count < 100)
            {
                formatter = Regex.Replace(formatter, pattern, separator);
            }
            
            count = 0;
            while (formatter.Contains("  ") && ++count < 100)
            {
                formatter = formatter.Replace("  ", " ");
            }
        }


        if (formatter.Trim() == separator.Trim())
            return string.Empty;
        
        return formatter.Trim();

        string Replace(string input, string field, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                string pattern = $@"(?<=\b{Regex.Escape(separator)}|\s*|^){Regex.Escape(field)}(?=\s*\b|$)";
                input = Regex.Replace(input, pattern, "", RegexOptions.IgnoreCase); // Remove the field
                input = Regex.Replace(input, $@"\s*{Regex.Escape(separator)}\s*", separator); // Remove extra spaces around separators
            }
            else
            {
                string pattern = $@"(?<=\b{Regex.Escape(separator)}|\s*|^){Regex.Escape(field)}(?=\s*\b|$)";
                input = Regex.Replace(input, pattern, value, RegexOptions.IgnoreCase);
            }
            return input;
        }
    }
    /// <summary>
    /// Gets the commercial name for a codec.
    /// </summary>
    /// <param name="codec">The codec name.</param>
    /// <returns>The commercial name.</returns>
    private static string GetCodecCommercialName(string codec)
    {
        // Convert codec name to uppercase for case-insensitive comparison
        return codec.ToUpper() switch
        {
            "DTS" => "Digital Theater Systems",
            "DOLBY DIGITAL" => "Dolby Digital",
            "DOLBY DIGITAL PLUS" => "Dolby Digital Plus",
            "DOLBY DIGITAL ATMOS" => "Dolby Digital Atmos",
            "DOLBY TRUEHD" => "Dolby TrueHD",
            "DTS-HD MASTER AUDIO" => "DTS-HD Master Audio",
            "PCM" => "Pulse-code Modulation",
            "AAC" => "Advanced Audio Coding",
            "MP3" => "MPEG-1 Audio Layer III",
            "WMA" => "Windows Media Audio",
            "FLAC" => "Free Lossless Audio Codec",
            "ALAC" => "Apple Lossless Audio Codec",
            "VORBIS" => "Vorbis",
            _ => codec.ToUpperInvariant()
        };
    }

}
