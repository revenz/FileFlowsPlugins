namespace MetaNodes.AniList;

/// <summary>
/// Represents the response from AniList API for episode queries.
/// </summary>
public class AniListEpisodeResponse
{
    /// <summary>
    /// Gets or sets the data contained in the response.
    /// </summary>
    public AniListEpisodeData Data { get; set; }
    /// <summary>
    /// Gets or sets the media information.
    /// </summary>
    public AniListMediaEpisodeData Media { get; set; }
}
