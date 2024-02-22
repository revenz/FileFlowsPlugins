namespace FileFlows.VideoNodes;

/// <summary>
/// Metadata about a video file
/// </summary>
public class VideoInfo
{
    /// <summary>
    /// Gets or sets the full filename of the file
    /// </summary>
    public string FileName { get; set; }
    
    /// <summary>
    /// Gets or sets the bitrate in bytes per second
    /// </summary>
    public float Bitrate { get; set; }
    
    /// <summary>
    /// Gets or sets the video streams contained in the file
    /// </summary>
    public List<VideoStream> VideoStreams { get; set; } = new List<VideoStream>();
    
    /// <summary>
    /// Gets or sets the audio streams contained in the file
    /// </summary>
    public List<AudioStream> AudioStreams { get; set; } = new List<AudioStream>();
    
    /// <summary>
    /// Gets or sets the subtitle streams contained in the file
    /// </summary>
    public List<SubtitleStream> SubtitleStreams { get; set; } = new List<SubtitleStream>();

    /// <summary>
    /// Gets or sets the chapters in the file
    /// </summary>
    public List<Chapter> Chapters { get; set; } = new List<Chapter>();

    /// <summary>
    /// Gets or sets the attachments in the file
    /// </summary>
    public List<AttachmentStream> Attachments { get; set; } = new();
}

/// <summary>
/// Metadata about a stream in a vidoe file
/// </summary>
public class VideoFileStream
{
    /// <summary>
    /// The original index of the stream in the overall video
    /// </summary> 
    public int Index { get; set; }
    /// <summary>
    /// The index of the specific type
    /// </summary>
    public int TypeIndex { get; set; }
    /// <summary>
    /// The stream title (name)
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// The bitrate(BPS) of the video stream in bytes per second
    /// </summary>
    public float Bitrate { get; set; }

    /// <summary>
    /// The codec of the stream
    /// </summary>
    public string Codec { get; set; } = "";

    /// <summary>
    /// The codec tag of the stream
    /// </summary>
    public string CodecTag { get; set; } = "";

    /// <summary>
    /// If this stream is an image
    /// </summary>
    public bool IsImage { get; set; }

    /// <summary>
    /// Gets or sets the index string of this track
    /// </summary>
    public string IndexString { get; set; }

    /// <summary>
    /// Gets or sets the input file index
    /// </summary>
    public int InputFileIndex { get; set; } = 0;
    
    /// <summary>
    /// Gets or sets the pixel format that should be used to decode this stream
    /// </summary>
    public string PixelFormat { get; set; }
    
    /// <summary>
    /// Gets or sets if this stream is 10 bit
    /// </summary>
    public bool Is10Bit { get; set; }
    
    /// <summary>
    /// Gets or sets if this stream is 12 bit
    /// </summary>
    public bool Is12Bit { get; set; }
}

/// <summary>
/// Metadata about a video stream
/// </summary>
public class VideoStream : VideoFileStream
{
    /// <summary>
    /// Gets or sets if the stream is HDR
    /// </summary>
    public bool HDR { get; set; }
    
    /// <summary>
    /// Gets or sets the bits
    /// 0 if unknown
    /// </summary>
    public int Bits { get; set; }
    
    /// <summary>
    /// Gets or sets if this is dolby vision
    /// </summary>
    public bool DolbyVision { get; set; }
    
    /// <summary>
    /// The width of the video stream
    /// </summary>
    public int Width { get; set; }
    /// <summary>
    /// The height of the video stream
    /// </summary>
    public int Height { get; set; }
    /// <summary>
    /// The number of frames per second
    /// </summary>
    public float FramesPerSecond { get; set; }

    /// <summary>
    /// The duration of the stream
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Converts the steam to a string
    /// </summary>
    /// <returns></returns>
    public override string ToString()
        => string.Join(" / ", new string[]
        {
            Index.ToString(),
            Codec,
            Title
        }.Where(x => string.IsNullOrWhiteSpace(x) == false));
}

/// <summary>
/// Metadata about an audio stream in a video file
/// </summary>
public class AudioStream : VideoFileStream
{
    /// <summary>
    /// The language of the stream
    /// </summary>
    public string Language { get; set; }

    /// <summary>
    /// The channels of the stream
    /// </summary>
    public float Channels { get; set; }

    /// <summary>
    /// The duration of the stream
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// The sample rate of the audio stream
    /// </summary>
    public int SampleRate { get; set; }

    /// <summary>
    /// If this is a the default audio track
    /// </summary>
    public bool Default { get; set; }

    /// <summary>
    /// Converts the steam to a string
    /// </summary>
    /// <returns></returns>
    public override string ToString()
        => string.Join(" / ", new string[]
        {
            Index.ToString(),
            Language,
            Codec,
            Title,
            Channels > 0 ? Channels.ToString("0.0") : null,
            Default ? "Default" : null
        }.Where(x => string.IsNullOrWhiteSpace(x) == false));
}

/// <summary>
/// Metadata about a subtitle stream in a video file
/// </summary>
public class SubtitleStream : VideoFileStream
{
    /// <summary>
    /// The language of the stream
    /// </summary>
    public string Language { get; set; }

    /// <summary>
    /// If this is a forced subtitle
    /// </summary>
    public bool Forced { get; set; }

    /// <summary>
    /// If this is a the default subtitle track
    /// </summary>
    public bool Default { get; set; }

    /// <summary>
    /// Converts the steam to a string
    /// </summary>
    /// <returns></returns>
    public override string ToString()
        => string.Join(" / ", new string[]
        {
            Index.ToString(),
            Language,
            Codec,
            Title,
            Default ? "Default" : null
        }.Where(x => string.IsNullOrWhiteSpace(x) == false));
}

/// <summary>
/// A chapter in a video file
/// </summary>
public class Chapter
{
    /// <summary>
    /// Gets or sets the title of the stream
    /// </summary>
    public string Title { get; set; }    
    
    /// <summary>
    /// Gets or sets the start of the chapter
    /// </summary>
    public TimeSpan Start { get; set; }
    
    /// <summary>
    /// Gets or sets the end of the chapter
    /// </summary>
    public TimeSpan End { get; set; }
}

/// <summary>
/// Attachment stream
/// </summary>
public class AttachmentStream: VideoFileStream
{
    /// <summary>
    /// Gets or sets the filename of the attachment
    /// </summary>
    public string FileName { get; set; }
    /// <summary>
    /// Gets or sets the mime type of the attachment
    /// </summary>
    public string MimeType { get; set; }
}