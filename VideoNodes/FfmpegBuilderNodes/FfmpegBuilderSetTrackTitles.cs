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
                    new () { Label = "Both", Value = "Both" },
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
                        track.IsForced
                    );
                }

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
    /// <returns>the formatted string</returns>
    public static string FormatTitle(string formatter, string separator, string language, string codec, bool isDefault, float bitrate = 0,
        float channels = 0, int sampleRate = 0, bool isForced = false)
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
        formatter = Replace(formatter, "!codec", codec.ToLowerInvariant());
        formatter = Replace(formatter, "!codec!", codec);
        formatter = Replace(formatter, "codec", codec.ToUpperInvariant());
        formatter = Replace(formatter, "default", isDefault ? "Default" : string.Empty);
        formatter = Replace(formatter, "forced", isForced ? "Forced" : string.Empty);
        formatter = Replace(formatter, "channels", Math.Abs(channels - 1) < 0.05f ? "Mono" :
            Math.Abs(channels - 2) < 0.05f ? "Stereo" :
            channels > 0 ? channels.ToString("0.0") : null);
        
        formatter = Replace(formatter, "bitrate", bitrate < 1 ? null : ((bitrate / 1000f).ToString("0.0").Replace(".0", string.Empty) + "Kbps"));
        formatter = Replace(formatter, "samplerate", sampleRate < 1 ? null : ((sampleRate / 1000f).ToString("0.0").Replace(".0", string.Empty) + "kHz"));
        formatter = Replace(formatter, "sample-rate", sampleRate < 1 ? null : ((sampleRate / 1000f).ToString("0.0").Replace(".0", string.Empty) + "kHz"));
        formatter = Replace(formatter, "sample", sampleRate < 1 ? null : ((sampleRate / 1000f).ToString("0.0").Replace(".0", string.Empty) + "kHz"));

        
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
}