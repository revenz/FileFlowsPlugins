namespace MetaNodes.AniList;


/// <summary>
/// Represents information about a season episode from AniList API.
/// </summary>
public class AniListEpisodeData
{
    /// <summary>
    /// Gets or sets the title of the episode.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the episode number.
    /// </summary>
    public int Episode { get; set; }

    /// <summary>
    /// Gets or sets the description of the episode.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the air date of the episode.
    /// </summary>
    public string AirDate { get; set; }

    /// <summary>
    /// Gets or sets the episode number for filtering.
    /// </summary>
    public int Number { get; set; }
}