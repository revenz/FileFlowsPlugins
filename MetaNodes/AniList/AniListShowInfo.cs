namespace MetaNodes.AniList;

/// <summary>
/// Represents information about an anime show.
/// </summary>
public class AniListShowInfo
{
    /// <summary>
    /// Gets or sets the title of the anime show.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the romaji title of the anime show.
    /// </summary>
    public string TitleRomaji { get; set; }

    /// <summary>
    /// Gets or sets the English title of the anime show.
    /// </summary>
    public string TitleEnglish { get; set; }

    /// <summary>
    /// Gets or sets the native title of the anime show.
    /// </summary>
    public string TitleNative { get; set; }

    /// <summary>
    /// Gets or sets the description of the anime show.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the year the anime show was released.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the average score of the anime show.
    /// </summary>
    public int? Score { get; set; }
}
