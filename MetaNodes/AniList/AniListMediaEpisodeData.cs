namespace MetaNodes.AniList;

/// <summary>
/// Represents the media data of an anime show with episode information from AniList API.
/// </summary>
public class AniListMediaEpisodeData
{
    /// <summary>
    /// Gets or sets the season number of the media.
    /// </summary>
    public int Season { get; set; }

    /// <summary>
    /// Gets or sets the total number of episodes in the media.
    /// </summary>
    public int Episodes { get; set; }

    /// <summary>
    /// Gets or sets the season episodes.
    /// </summary>
    public List<AniListEpisodeData> SeasonEpisodes { get; set; }
}