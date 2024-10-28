namespace MetaNodes.AniList;

/// <summary>
/// Represents the airing schedule for a specific anime, containing a list of episodes and their air dates.
/// </summary>
public class AniListAirScheduleResponse
{
    /// <summary>
    /// Gets or sets the airing schedule for the anime.
    /// </summary>
    public AniListAiringSchedule AiringSchedule { get; set; }
    
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public int Id { get; set; }
}

/// <summary>
/// Represents the airing schedule for a specific anime, containing a list of episodes and their air dates.
/// </summary>
public class AniListAiringSchedule
{
    /// <summary>
    /// Gets or sets the list of edges, where each edge contains information about an episode's airing schedule.
    /// </summary>
    public List<AniListAiringScheduleEdge> Edges { get; set; }
}

/// <summary>
/// Represents an edge in the airing schedule, containing a node with the episode information.
/// </summary>
public class AniListAiringScheduleEdge
{
    /// <summary>
    /// Gets or sets the node that contains information about a specific episode.
    /// </summary>
    public AniListAiringScheduleNode Node { get; set; }
}

/// <summary>
/// Represents an individual episode's airing information.
/// </summary>
public class AniListAiringScheduleNode
{
    /// <summary>
    /// Gets or sets the episode number.
    /// </summary>
    public int Episode { get; set; }

    /// <summary>
    /// Gets or sets the Unix timestamp representing the airing time of the episode.
    /// </summary>
    public long AiringAt { get; set; }

    /// <summary>
    /// Gets the airing date and time of the episode as a <see cref="DateTimeOffset"/> object, based on the Unix timestamp <see cref="AiringAt"/>.
    /// </summary>
    /// <remarks>
    /// Converts the <see cref="AiringAt"/> Unix timestamp to a <see cref="DateTimeOffset"/> in UTC format.
    /// </remarks>
    public DateTimeOffset AirDate => DateTimeOffset.FromUnixTimeSeconds(AiringAt);
}