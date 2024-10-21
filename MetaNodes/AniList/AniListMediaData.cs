namespace MetaNodes.AniList;

/// <summary>
/// Represents the media data of an anime show from AniList API.
/// </summary>
public class AniListMediaData
{
    /// <summary>
    /// Gets or sets the title information of the media.
    /// </summary>
    public AniListTitle Title { get; set; }

    /// <summary>
    /// Gets or sets the description of the media.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the start date information of the media.
    /// </summary>
    public AniListDateData StartDate { get; set; }

    /// <summary>
    /// Gets or sets the average score of the media.
    /// </summary>
    public int? AverageScore { get; set; }
}
