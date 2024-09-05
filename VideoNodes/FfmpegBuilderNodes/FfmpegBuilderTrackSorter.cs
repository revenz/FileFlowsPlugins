using System.Globalization;
using FileFlows.VideoNodes.FfmpegBuilderNodes.Models;
using FileFlows.VideoNodes.Helpers;

namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// FFmpeg Builder: Track Sorter
/// </summary>
public class FfmpegBuilderTrackSorter : FfmpegBuilderNode
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
    public override string HelpUrl => "https://fileflows.com/docs/plugins/video-nodes/ffmpeg-builder/track-sorter";

    /// <summary>
    /// Gets or sets the stream type
    /// </summary>
    [Select(nameof(StreamTypeOptions), 1)]
    public string StreamType { get; set; }

    [KeyValue(2, nameof(SorterOptions))]
    [Required]
    public List<KeyValuePair<string, string>> Sorters { get; set; }

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
                    new() { Label = "Audio", Value = "Audio" },
                    new() { Label = "Subtitle", Value = "Subtitle" }
                };
            }

            return _StreamTypeOptions;
        }
    }

    private static List<ListOption> _SorterOptions;

    /// <summary>
    /// Gets or sets the sorter options
    /// </summary>
    public static List<ListOption> SorterOptions
    {
        get
        {
            if (_SorterOptions == null)
            {
                _SorterOptions = new List<ListOption>
                {
                    new() { Label = "Bitrate", Value = "Bitrate" },
                    new() { Label = "Bitrate Reversed", Value = "BitrateDesc" },
                    new() { Label = "Channels", Value = "Channels" },
                    new() { Label = "Channels Reversed", Value = "ChannelsDesc" },
                    new() { Label = "Codec", Value = "Codec" },
                    new() { Label = "Codec Reversed", Value = "CodecDesc" },
                    new() { Label = "Language", Value = "Language" },
                    new() { Label = "Language Reversed", Value = "LanguageDesc" },
                    new() { Label = "Title", Value = "Title" },
                    new() { Label = "Title Reversed", Value = "TitleDesc" }
                };
            }

            return _SorterOptions;
        }
    }
    
    /// <summary>
    /// Gets or sets if the first track should be marked default
    /// </summary>
    [Boolean(3)]
    [DefaultValue(true)]
    public bool SetFirstDefault { get; set; }

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the next output node</returns>
    public override int Execute(NodeParameters args)
    {
        bool changed = false;
        if (StreamType == "Audio")
        {
            args.Logger?.ILog("Sorting Audio Tracks");
            changed = ProcessStreams(args, Model.AudioStreams);
        }
        else if (StreamType == "Subtitle")
        {
            args.Logger?.ILog("Sorting Subtitle Tracks");
            changed = ProcessStreams(args, Model.SubtitleStreams);
        }

        if (changed)
            args.Logger?.ILog("Changes were made");
        else
            args.Logger?.ILog("No changes were made");

        return changed ? 1 : 2;
    }

    /// <summary>
    /// Processes the streams 
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <param name="streams">the streams to process for deletion</param>
    /// <typeparam name="T">the stream type</typeparam>
    /// <returns>if any changes were made</returns>
    internal bool ProcessStreams<T>(NodeParameters args, List<T> streams, int sortIndex = 0) where T : FfmpegStream
    {
        if (streams?.Any() != true || Sorters?.Any() != true || sortIndex >= Sorters.Count)
            return false;

        if (sortIndex == 0)
        {
            foreach (var sorter in Sorters)
            {
                args.Logger?.ILog("Sorter: " + sorter.Key + " = " + sorter.Value);
            }
        }

        var orderedStreams = SortStreams(args, streams);
        
        if(SetFirstDefault)
            args.Logger?.ILog("Will set first track as default");
        else
            args.Logger?.ILog("Won't set first track as default");

        bool changes = false;
        // Replace the unsorted items with the sorted ones
        for (int i = 0; i < streams.Count; i++)
        {
            bool changed = false;
            if (SetFirstDefault)
            {
                bool newDefault = orderedStreams[0] == streams[i];
                changed = streams[i] != orderedStreams[i] || orderedStreams[i].IsDefault != newDefault;
                streams[i].IsDefault = newDefault;
            }
            else
            {
                changed = streams[i] != orderedStreams[i];
            }

            if (changed)
            {
                streams[i].ForcedChange = true;
                args.Logger?.ILog("Stream has change[1]: " + streams[i]);
                
                if (streams[i] != orderedStreams[i])
                {
                    orderedStreams[i].ForcedChange = true;
                    args.Logger?.ILog("Stream has change[2]: " + orderedStreams[i]);
                }
            }
            changes |= changed;
            streams[i] = orderedStreams[i];
        }
        
        return changes;
    }

    internal List<T> SortStreams<T>(NodeParameters args, List<T> streams) where T : FfmpegStream
    {
        if (streams?.Any() != true || Sorters?.Any() != true || streams.Count == 1)
            return streams;

        return streams.OrderBy(stream =>
            {
                // we add a 1 to deleted ones first, so they are always sorted after non-deleted ones
                var sortKey = (stream.Deleted ? "1|" : "0|") + GetSortKey(args, stream);
                args.Logger?.ILog(stream.ToString() + " -> sort key = " + sortKey);
                return sortKey;
            })
            .ToList();
    }

    private string GetSortKey<T>(NodeParameters args, T stream) where T : FfmpegStream
    {
        string sortKey = "";

        for (int i = 0; i < Sorters.Count; i++)
        {
            var key = Sorters[i].Key;
            int sorterLength = 1;
            if (string.IsNullOrWhiteSpace(Sorters[i].Value))
            {
                // we are sorting by value, not comparison
                if (key.StartsWith(nameof(AudioStream.Bitrate)))
                    sorterLength = 15;
                else if (key.StartsWith(nameof(AudioStream.Channels)))
                    sorterLength = 2;
            }

            var sortValue = Math.Round(SortValue(args, stream, Sorters[i], sorterLength)).ToString(CultureInfo.InvariantCulture);
            // Trim the sort value to sorter Length characters
            string trimmedValue = sortValue[..Math.Min(sortValue.Length, sorterLength)];

            // Pad the trimmed value with left zeros if needed
            string paddedValue = trimmedValue.PadLeft(sorterLength, '0');

            // Concatenate the padded value to the sort key
            sortKey += paddedValue + "|";
        }

        return sortKey.TrimEnd('|');
    }

    /// <summary>
    /// Tests if two lists are the same
    /// </summary>
    /// <param name="original">the original list</param>
    /// <param name="reordered">the reordered list</param>
    /// <typeparam name="T">the type of items</typeparam>
    /// <returns>true if the lists are the same, otherwise false</returns>
    public bool AreSame<T>(List<T> original, List<T> reordered) where T : FfmpegStream
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
    
    
    
    
    
    /// <summary>
    /// Calculates the sort value for a stream property based on the specified sorter.
    /// </summary>
    /// <typeparam name="T">Type of the stream.</typeparam>
    /// <param name="args">the node parameters</param>
    /// <param name="stream">The stream instance.</param>
    /// <param name="sorter">The key-value pair representing the sorter.</param>
    /// <param name="sorterLength">The number of characters to use for this sorter value</param>
    /// <returns>The calculated sort value for the specified property and sorter.</returns>
    public static double SortValue<T>(NodeParameters args, T stream, KeyValuePair<string, string> sorter, int sorterLength) where T : FfmpegStream
    {
        string property = sorter.Key;
        bool invert = property.EndsWith("Desc");
        if (invert)
            property = property[..^4]; // remove "Desc"
        
        string comparison = sorter.Value ?? string.Empty;

        if (comparison.StartsWith("{") && comparison.EndsWith("}"))
        {
            comparison = comparison[1..^1];
            if (args?.Variables?.TryGetValue(comparison, out object? variable) == true)
                comparison = variable?.ToString() ?? string.Empty;
            else
                comparison = string.Empty;
        }
        else if (args?.Variables?.TryGetValue(comparison, out object? oVariable) == true)
        {
            comparison = oVariable?.ToString() ?? string.Empty;
        }

        if (property == nameof(stream.Language))
        {
            object? oOriginalLanguage = null;
            args?.Variables?.TryGetValue("OriginalLanguage", out oOriginalLanguage);
            var originalLanguage = LanguageHelper.GetIso2Code(oOriginalLanguage?.ToString() ?? string.Empty);
            comparison = string.Join("|",
                comparison.Split('|', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x =>
                    {
                        if (x?.ToLowerInvariant()?.StartsWith("orig") == true)
                            return originalLanguage;
                        return LanguageHelper.GetIso2Code(x);
                    }).Where(x => string.IsNullOrWhiteSpace(x) == false).ToArray());
        }

        var value = property switch
        {
            nameof(FfmpegStream.Codec) => stream.Codec,
            nameof(AudioStream.Bitrate) => (stream is FfmpegAudioStream audioStream) ? audioStream?.Stream?.Bitrate : null,
            nameof(FfmpegStream.Language) => LanguageHelper.GetIso2Code(stream.Language),
            _ => stream.GetType().GetProperty(property)?.GetValue(stream, null)
        };

        double result;

        if (value != null && value is string == false && string.IsNullOrWhiteSpace(comparison) &&
            double.TryParse(value.ToString(), out double dblValue))
        {
            if (property == nameof(AudioStream.Channels))
            {
                // remove the decimal and round
                result = Math.Round(dblValue, 1) * 10;
            }
            else
            {
                result = dblValue;
            }
        }
        else if (IsMathOperation(comparison))
            result = ApplyMathOperation(value.ToString(), comparison) ? 0 : 1;
        else if (GeneralHelper.IsRegex(comparison))
            result = Regex.IsMatch(value.ToString(), comparison, RegexOptions.IgnoreCase) ? 0 : 1;
        else if (value != null && double.TryParse(value.ToString(), out double dbl))
            result = dbl;
        else if (property == nameof(FfmpegStream.Title) && string.IsNullOrWhiteSpace(comparison) == false && string.IsNullOrWhiteSpace(value?.ToString()) == false)
            result = value?.ToString()?.ToLowerInvariant()?.Contains(comparison.ToLowerInvariant()) == true ? 0 : 1;
        else
            result = string.Equals(value?.ToString() ?? string.Empty, comparison ?? string.Empty, StringComparison.OrdinalIgnoreCase) ? 0 : 1;

        return invert ? InvertBits(result, sorterLength) : result;
    }

    /// <summary>
    /// Adjusts the comparison string by handling common mistakes in units and converting them into full numbers.
    /// </summary>
    /// <param name="comparisonValue">The original comparison string to be adjusted.</param>
    /// <returns>The adjusted comparison string with corrected units or the original comparison if no adjustments are made.</returns>
    private static string AdjustComparisonValue(string comparisonValue)
    {
        if (string.IsNullOrWhiteSpace(comparisonValue))
            return string.Empty;
        
        string adjustedComparison = comparisonValue.ToLower().Trim();

        // Handle common mistakes in units
        if (adjustedComparison.EndsWith("mbps"))
        {
            // Make an educated guess for Mbps to kbps conversion
            return adjustedComparison[..^4] switch
            {
                { } value when double.TryParse(value, out var numericValue) => (numericValue * 1_000_000)
                    .ToString(CultureInfo.InvariantCulture),
                _ => comparisonValue
            };
        }
        if (adjustedComparison.EndsWith("kbps"))
        {
            // Make an educated guess for kbps to bps conversion
            return adjustedComparison[..^4] switch
            {
                { } value when double.TryParse(value, out var numericValue) => (numericValue * 1_000)
                    .ToString(CultureInfo.InvariantCulture),
                _ => comparisonValue
            };
        }
        if (adjustedComparison.EndsWith("kb"))
        {
            return adjustedComparison[..^2] switch
            {
                { } value when double.TryParse(value, out var numericValue) => (numericValue * 1_000 )
                    .ToString(CultureInfo.InvariantCulture),
                _ => comparisonValue
            };
        }
        if (adjustedComparison.EndsWith("mb"))
        {
            return adjustedComparison[..^2] switch
            {
                { } value when double.TryParse(value, out var numericValue) => (numericValue * 1_000_000 )
                    .ToString(CultureInfo.InvariantCulture),
                _ => comparisonValue
            };
        }
        if (adjustedComparison.EndsWith("gb"))
        {
            return adjustedComparison[..^2] switch
            {
                { } value when double.TryParse(value, out var numericValue) => (numericValue * 1_000_000_000 )
                    .ToString(CultureInfo.InvariantCulture),
                _ => comparisonValue
            };
        }
        if (adjustedComparison.EndsWith("tb"))
        {
            return adjustedComparison[..^2] switch
            {
                { } value when double.TryParse(value, out var numericValue) => (numericValue * 1_000_000_000_000)
                    .ToString(CultureInfo.InvariantCulture),
                _ => comparisonValue
            };
        }

        if (adjustedComparison.EndsWith("kib"))
        {
            return adjustedComparison[..^3] switch
            {
                { } value when double.TryParse(value, out var numericValue) => (numericValue * 1_024 )
                    .ToString(CultureInfo.InvariantCulture),
                _ => comparisonValue
            };
        }
        if (adjustedComparison.EndsWith("mib"))
        {
            return adjustedComparison[..^3] switch
            {
                { } value when double.TryParse(value, out var numericValue) => (numericValue * 1_048_576 )
                    .ToString(CultureInfo.InvariantCulture),
                _ => comparisonValue
            };
        }
        if (adjustedComparison.EndsWith("gib"))
        {
            return adjustedComparison[..^3] switch
            {
                { } value when double.TryParse(value, out var numericValue) => (numericValue * 1_099_511_627_776 )
                    .ToString(CultureInfo.InvariantCulture),
                _ => comparisonValue
            };
        }
        if (adjustedComparison.EndsWith("tib"))
        {
            return adjustedComparison[..^3] switch
            {
                { } value when double.TryParse(value, out var numericValue) => (numericValue * 1_000_000_000_000)
                    .ToString(CultureInfo.InvariantCulture),
                _ => comparisonValue
            };
        }
        return comparisonValue;
    }

    /// <summary>
    /// Inverts the bits of a double value.
    /// </summary>
    /// <param name="value">The double value to invert.</param>
    /// <param name="sorterLength">The number of characters to use for this sorter value</param>
    /// <returns>The inverted double value.</returns>
    private static double InvertBits(double value, int sorterLength)
    {
        // Convert the double to a string with 15 characters above the decimal point
        string stringValue = Math.Round(value, 0).ToString("F0");

        // Invert the digits and pad left with zeros
        char[] charArray = stringValue.PadLeft(sorterLength, '0').ToCharArray();
        for (int i = 0; i < charArray.Length; i++)
        {
            charArray[i] = (char)('9' - (charArray[i] - '0'));
        }

        // Parse the inverted string back to a double
        double invertedDouble;
        if (double.TryParse(new string(charArray), out invertedDouble))
        {
            return invertedDouble;
        }
        else
        {
            // Handle parsing error
            throw new InvalidOperationException("Failed to parse inverted double string.");
        }
    }
    
    /// <summary>
    /// Checks if the comparison string represents a mathematical operation.
    /// </summary>
    /// <param name="comparison">The comparison string to check.</param>
    /// <returns>True if the comparison is a mathematical operation, otherwise false.</returns>
    private static bool IsMathOperation(string comparison)
    {
        // Check if the comparison string starts with <=, <, >, >=, ==, or =
        return new[] { "<=", "<", ">", ">=", "==", "=" }.Any(comparison.StartsWith);
    }

    /// <summary>
    /// Applies a mathematical operation to the value based on the specified operation string.
    /// </summary>
    /// <param name="value">The value to apply the operation to.</param>
    /// <param name="operation">The operation string representing the mathematical operation.</param>
    /// <returns>True if the mathematical operation is successful, otherwise false.</returns>
    private static bool ApplyMathOperation(string value, string operation)
    {
        // This is a basic example; you may need to handle different operators
        switch (operation.Substring(0, 2))
        {
            case "<=":
                return Convert.ToDouble(value) <= Convert.ToDouble(AdjustComparisonValue(operation[2..].Trim()));
            case ">=":
                return Convert.ToDouble(value) >= Convert.ToDouble(AdjustComparisonValue(operation[2..].Trim()));
            case "==":
                return Math.Abs(Convert.ToDouble(value) - Convert.ToDouble(AdjustComparisonValue(operation[2..].Trim()))) < 0.05f;
            case "!=":
            case "<>":
                return Math.Abs(Convert.ToDouble(value) - Convert.ToDouble(AdjustComparisonValue(operation[2..].Trim()))) > 0.05f;
        }

        switch (operation.Substring(0, 1))
        {
            case "<":
                return Convert.ToDouble(value) < Convert.ToDouble(AdjustComparisonValue(operation[1..].Trim()));
            case ">":
                return Convert.ToDouble(value) > Convert.ToDouble(AdjustComparisonValue(operation[1..].Trim()));
            case "=":
                return Math.Abs(Convert.ToDouble(value) - Convert.ToDouble(AdjustComparisonValue(operation[1..].Trim()))) < 0.05f;
        }

        return false;
    }
}