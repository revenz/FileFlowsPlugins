using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using DM.MovieApi.MovieDb.Genres;

namespace DM.MovieApi.MovieDb.TV;

/// <summary>
/// Represents TV show information, including metadata such as name, popularity, and genre details.
/// </summary>
[DataContract]
public class TVShowInfo
{
    /// <summary>
    /// Gets or sets the unique identifier for the TV show.
    /// </summary>
    [DataMember(Name = "id")]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the TV show.
    /// </summary>
    [DataMember(Name = "name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the original name of the TV show.
    /// </summary>
    [DataMember(Name = "original_name")]
    [JsonPropertyName("original_name")]
    public string OriginalName { get; set; }

    /// <summary>
    /// Gets or sets the URL path to the poster image.
    /// </summary>
    [DataMember(Name = "poster_path")]
    [JsonPropertyName("poster_path")]
    public string PosterPath { get; set; }

    /// <summary>
    /// Gets or sets the URL path to the backdrop image.
    /// </summary>
    [DataMember(Name = "backdrop_path")]
    [JsonPropertyName("backdrop_path")]
    public string BackdropPath { get; set; }

    /// <summary>
    /// Gets or sets the popularity score of the TV show.
    /// </summary>
    [DataMember(Name = "popularity")]
    [JsonPropertyName("popularity")]
    public double Popularity { get; set; }

    /// <summary>
    /// Gets or sets the average vote rating of the TV show.
    /// </summary>
    [DataMember(Name = "vote_average")]
    [JsonPropertyName("vote_average")]
    public double VoteAverage { get; set; }

    /// <summary>
    /// Gets or sets the total number of votes received.
    /// </summary>
    [DataMember(Name = "vote_count")]
    [JsonPropertyName("vote_count")]
    public int VoteCount { get; set; }

    /// <summary>
    /// Gets or sets the overview or summary of the TV show.
    /// </summary>
    [DataMember(Name = "overview")]
    [JsonPropertyName("overview")]
    public string Overview { get; set; }

    /// <summary>
    /// Gets or sets the first air date of the TV show.
    /// </summary>
    [DataMember(Name = "first_air_date")]
    [JsonPropertyName("first_air_date")]
    public DateTime FirstAirDate { get; set; }

    /// <summary>
    /// Gets or sets the origin countries of the TV show.
    /// </summary>
    [DataMember(Name = "origin_country")]
    [JsonPropertyName("origin_country")]
    public List<string> OriginCountry { get; set; }

    /// <summary>
    /// Gets or sets the list of genre IDs associated with the TV show.
    /// </summary>
    [DataMember(Name = "genre_ids")]
    [JsonPropertyName("genre_ids")]
    public List<int> GenreIds { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of genres associated with the TV show.
    /// This property is not serialized to JSON.
    /// </summary>
    public List<Genre> Genres { get; internal set; } = [];

    /// <summary>
    /// Gets or sets the original language of the TV show.
    /// </summary>
    [DataMember(Name = "original_language")]
    [JsonPropertyName("original_language")]
    public string OriginalLanguage { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TVShowInfo"/> class.
    /// </summary>
    public TVShowInfo()
    {
    }

    /// <summary>
    /// Returns a string representation of the TV show.
    /// </summary>
    /// <returns>A formatted string containing the name, ID, and first air date.</returns>
    public override string ToString()
        => $"{Name} ({Id} - {FirstAirDate:yyyy-MM-dd})";
}
