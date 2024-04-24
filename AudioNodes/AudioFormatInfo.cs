using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FileFlows.AudioNodes;

/// <summary>
/// Represents the JSON structure returned by ffprobe for an audio file.
/// </summary>
public class FFprobeAudioInfo
{
    /// <summary>
    /// Gets or sets the format information extracted from ffprobe.
    /// </summary>
    public AudioFormatInfo Format { get; set; }

    /// <summary>
    /// Parses JSON output from FFprobe
    /// </summary>
    /// <param name="json">the json output from FFprobe</param>
    /// <returns>the AudioFormatInfo parsed</returns>
    public static Result<AudioFormatInfo> Parse(string json)
    {
        try
        {
            var ffAudioFormatInfo = System.Text.Json.JsonSerializer.Deserialize<FFprobeAudioInfo>(json,
                new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                });
            return ffAudioFormatInfo.Format;
        }
        catch (Exception ex)
        {
            return Result<AudioFormatInfo>.Fail(ex.Message);
        }
    }
}

/// <summary>
/// Represents the format information extracted from ffprobe for an audio file.
/// </summary>
public class AudioFormatInfo
{
    /// <summary>
    /// Gets or sets the filename of the audio file.
    /// </summary>
    public string Filename { get; set; }

    /// <summary>
    /// Gets or sets the number of streams in the audio file.
    /// </summary>
    public int NbStreams { get; set; }

    /// <summary>
    /// Gets or sets the number of programs in the audio file.
    /// </summary>
    public int NbPrograms { get; set; }

    /// <summary>
    /// Gets or sets the format name (e.g., flac).
    /// </summary>
    public string FormatName { get; set; }

    /// <summary>
    /// Gets or sets the long format name (e.g., raw FLAC).
    /// </summary>
    public string FormatLongName { get; set; }

    /// <summary>
    /// Gets or sets the duration of the audio file.
    /// </summary>
    [JsonPropertyName("duration")]
    [JsonConverter(typeof(FFprobeTimeSpanConverter))]
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the start time of the audio file.
    /// </summary>
    [JsonPropertyName("start_time")]
    [JsonConverter(typeof(FFprobeTimeSpanConverter))]
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// Gets or sets the size of the audio file.
    /// </summary>
    [JsonPropertyName("size")]
    [JsonConverter(typeof(LongConverter))]
    public long Size { get; set; }

    /// <summary>
    /// Gets or sets the bit rate of the audio file.
    /// </summary>
    [JsonPropertyName("bit_rate")]
    [JsonConverter(typeof(LongConverter))]
    public long Bitrate { get; set; }

    /// <summary>
    /// Gets or sets the probe score.
    /// </summary>
    public int ProbeScore { get; set; }

    /// <summary>
    /// Gets or sets the tags associated with the audio file.
    /// </summary>
    public AudioTags Tags { get; set; }
}

/// <summary>
/// Represents the tags associated with an audio file.
/// </summary>
public class AudioTags
{
    /// <summary>
    /// Gets or sets the title of the audio file.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the artist of the audio file.
    /// </summary>
    public string Artist { get; set; }

    /// <summary>
    /// Gets or sets the album of the audio file.
    /// </summary>
    public string Album { get; set; }

    /// <summary>
    /// Gets or sets the track number of the audio file.
    /// </summary>
    public string Track { get; set; }

    /// <summary>
    /// Gets or sets the release date of the audio file.
    /// </summary>
    public string Date { get; set; }

    /// <summary>
    /// Gets or sets the genre of the audio file.
    /// </summary>
    public string Genre { get; set; }

    /// <summary>
    /// Gets or sets the total number of tracks in the album.
    /// </summary>
    public string TotalTracks { get; set; }

    /// <summary>
    /// Gets or sets the disc number of the audio file.
    /// </summary>
    public string Disc { get; set; }

    /// <summary>
    /// Gets or sets the total number of discs in the album.
    /// </summary>
    public string TotalDiscs { get; set; }
}


/// <summary>
/// Custom converter for TimeSpan to handle string representation.
/// </summary>
public class FFprobeTimeSpanConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var stringValue = reader.GetString() ?? string.Empty;
        if (double.TryParse(stringValue, out double seconds) == false)
            return default;
        
        return TimeSpan.FromSeconds(seconds);
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.TotalSeconds.ToString(CultureInfo.InvariantCulture));
    }
}

/// <summary>
/// Custom converter for long to handle string representation.
/// </summary>
public class LongConverter : JsonConverter<long>
{
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            if (long.TryParse(reader.GetString(), out long result))
            {
                return result;
            }
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetInt64();
        }

        throw new JsonException($"Invalid long format: {reader.GetString()}");
    }

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}