using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace DM.MovieApi.MovieDb.TV;

/// <summary>
/// Represents a TV show season with details such as air date, episode count, and season number.
/// </summary>
[DataContract]
public class Season
{
    /// <summary>
    /// Gets or sets the unique identifier for the season.
    /// </summary>
    [DataMember(Name = "id")]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the air date of the season.
    /// </summary>
    [DataMember(Name = "air_date")]
    [JsonPropertyName("air_date")]
    public DateTime AirDate { get; set; }

    /// <summary>
    /// Gets or sets the number of episodes in the season.
    /// </summary>
    [DataMember(Name = "episode_count")]
    [JsonPropertyName("episode_count")]
    public int EpisodeCount { get; set; }

    /// <summary>
    /// Gets or sets the poster path for the season.
    /// </summary>
    [DataMember(Name = "poster_path")]
    [JsonPropertyName("poster_path")]
    public string PosterPath { get; set; }

    /// <summary>
    /// Gets or sets the season number.
    /// </summary>
    [DataMember(Name = "season_number")]
    [JsonPropertyName("season_number")]
    public int SeasonNumber { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Season"/> class.
    /// </summary>
    public Season()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Season"/> class with specified values.
    /// </summary>
    /// <param name="id">The unique identifier for the season.</param>
    /// <param name="airDate">The air date of the season.</param>
    /// <param name="episodeCount">The number of episodes in the season.</param>
    /// <param name="posterPath">The poster path for the season.</param>
    /// <param name="seasonNumber">The season number.</param>
    public Season(int id, DateTime airDate, int episodeCount, string posterPath, int seasonNumber)
    {
        Id = id;
        AirDate = airDate;
        EpisodeCount = episodeCount;
        PosterPath = posterPath;
        SeasonNumber = seasonNumber;
    }

    /// <summary>
    /// Returns a string representation of the season.
    /// </summary>
    /// <returns>A string in the format (SeasonNumber - AirDate).</returns>
    public override string ToString()
        => $"({SeasonNumber} - {AirDate:yyyy-MM-dd})";
}
