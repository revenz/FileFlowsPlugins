namespace FileFlows.VideoNodes.FfmpegBuilderNodes.Models;

public abstract class FfmpegStream
{
    public const string REMOVED = "###REMOVED###";

    public bool Deleted { get; set; }
    public int Index { get; set; }
    public string Title { get; set; }
    public string Language { get; set; }

    public bool IsDefault { get; set; }

    public abstract bool HasChange { get; }
    public bool ForcedChange { get; set; }

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


