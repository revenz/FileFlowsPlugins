namespace FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

public abstract class FfmpegStream
{
    public const string REMOVED = "###REMOVED###";

    /// <summary>
    /// Gets or sets a value indicating whether this item is deleted.
    /// </summary>
    public bool Deleted { get; set; }

    /// <summary>
    /// Gets or sets the index of the item.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets the title of the item.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the language associated with the item.
    /// </summary>
    public string Language { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this item is the default stream.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Gets a value indicating whether there have been changes to this stream.
    /// </summary>
    public abstract bool HasChange { get; }

    /// <summary>
    /// Gets or sets a value indicating whether changes are being forced for this stream.
    /// </summary>
    public bool ForcedChange { get; set; }
    
    /// <summary>
    /// Gets or sets the codec this stream will use when run by FFmpeg
    /// </summary>
    public string Codec { get; set; }

    /// <summary>
    /// Gets the parameters for the stream
    /// </summary>
    /// <param name="args">the arguments</param>
    /// <returns>the parameters for this stream</returns>
    public abstract string[] GetParameters(GetParametersArgs args);

    private List<string> _Metadata = new List<string>();
    public List<string> Metadata
    {
        get => _Metadata;
        set
        {
            _Metadata = value ?? new List<string>();
        }
    }

    /// <summary>
    /// Args passed to get parameters
    /// </summary>
    public class GetParametersArgs
    {
        /// <summary>
        /// Gets or sets the overall index for all streams
        /// </summary>
        public int OutputOverallIndex { get; set; }

        /// <summary>
        /// the index for just this type of streams
        /// </summary>
        public int OutputTypeIndex { get; set; }

        /// <summary>
        /// Gets or sets the source extension
        /// </summary>
        public string SourceExtension { get; set; }

        /// <summary>
        /// Gets or sets the destination extension
        /// </summary>
        public string DestinationExtension { get; set; }

        /// <summary>
        /// Gets or sets if the default flag should be set
        /// </summary>
        public bool UpdateDefaultFlag { get; set; }
        
        /// <summary>
        /// Gets or sets the logger
        /// </summary>
        public ILogger Logger { get; set; }
    }
}


