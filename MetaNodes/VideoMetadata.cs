namespace MetaNodes;

internal class VideoMetadata
{
    /// <summary>
    /// Gets or sets the title of the item
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the subtitle of the item
    /// </summary>
    public string Subtitle { get; set; }
    
    /// <summary>
    /// Gets or sets the description of the item
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// Gets or sets the year hte item was released
    /// </summary>
    public int Year { get; set; }
    
    /// <summary>
    /// Gets or sets the date the item was released
    /// </summary>
    public DateTime ReleaseDate { get; set; }
    
    /// <summary>
    /// Gets or sets the original language
    /// </summary>
    public string OriginalLanguage { get; set; }
    
    /// <summary>
    /// Gets or sets a filename where a saved copy of the art JPEG is located
    /// </summary>
    public string ArtJpeg { get; set; }
    
    /// <summary>
    /// Gets or sets the season number of the show, if a show
    /// </summary>
    public int? Season { get; set; }
    /// <summary>
    /// Gets or sets the episode number of the show, if a show
    /// </summary>
    public int? Episode { get; set; }

    private List<string> _Actors = new ();
    public List<string> Actors { get => _Actors; set { _Actors = value ?? new(); } }

    private List<string> _Directors = new();
    public List<string> Directors { get => _Directors; set { _Directors = value ?? new(); } }

    private List<string> _Writers = new();
    public List<string> Writers { get => _Writers; set { _Writers = value ?? new(); } }

    private List<string> _Producers = new();
    public List<string> Producers { get => _Producers; set { _Producers = value ?? new(); } }

    private List<string> _Genres = new();
    public List<string> Genres { get => _Genres; set { _Genres = value ?? new(); } }
}
