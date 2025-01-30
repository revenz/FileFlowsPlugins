using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using DM.MovieApi.MovieDb.Companies;
using DM.MovieApi.MovieDb.Genres;
using DM.MovieApi.MovieDb.Keywords;

namespace DM.MovieApi.MovieDb.TV;

/// <summary>
/// Represents detailed information about a TV show, including metadata such as name, episodes, seasons, and genres.
/// </summary>
[DataContract]
public class TVShow
{
    /// <summary>
    /// Gets or sets the unique identifier for the TV show.
    /// </summary>
    [DataMember(Name = "id")]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the URL path to the backdrop image.
    /// </summary>
    [DataMember(Name = "backdrop_path")]
    [JsonPropertyName("backdrop_path")]
    public string BackdropPath { get; set; }

    /// <summary>
    /// Gets or sets the list of creators of the TV show.
    /// </summary>
    [DataMember(Name = "created_by")]
    [JsonPropertyName("created_by")]
    public List<TVShowCreator> CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the list of episode run times in minutes.
    /// </summary>
    [DataMember(Name = "episode_run_time")]
    [JsonPropertyName("episode_run_time")]
    public List<int> EpisodeRunTime { get; set; }

    /// <summary>
    /// Gets or sets the first air date of the TV show.
    /// </summary>
    [DataMember(Name = "first_air_date")]
    [JsonPropertyName("first_air_date")]
    public DateTime FirstAirDate { get; set; }

    /// <summary>
    /// Gets or sets the list of genres associated with the TV show.
    /// </summary>
    [DataMember(Name = "genres")]
    [JsonPropertyName("genres")]
    public List<Genre> Genres { get; set; }

    /// <summary>
    /// Gets or sets the homepage URL of the TV show.
    /// </summary>
    [DataMember(Name = "homepage")]
    [JsonPropertyName("homepage")]
    public string Homepage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the show is still in production.
    /// </summary>
    [DataMember(Name = "in_production")]
    [JsonPropertyName("in_production")]
    public bool InProduction { get; set; }

    /// <summary>
    /// Gets or sets the list of spoken languages in the TV show.
    /// </summary>
    [DataMember(Name = "languages")]
    [JsonPropertyName("languages")]
    public List<string> Languages { get; set; }

    /// <summary>
    /// Gets or sets the last air date of the TV show.
    /// </summary>
    [DataMember(Name = "last_air_date")]
    [JsonPropertyName("last_air_date")]
    public DateTime LastAirDate { get; set; }

    /// <summary>
    /// Gets or sets the name of the TV show.
    /// </summary>
    [DataMember(Name = "name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the list of networks associated with the TV show.
    /// </summary>
    [DataMember(Name = "networks")]
    [JsonPropertyName("networks")]
    public List<Network> Networks { get; set; }

    /// <summary>
    /// Gets or sets the total number of episodes in the TV show.
    /// </summary>
    [DataMember(Name = "number_of_episodes")]
    [JsonPropertyName("number_of_episodes")]
    public int NumberOfEpisodes { get; set; }

    /// <summary>
    /// Gets or sets the total number of seasons in the TV show.
    /// </summary>
    [DataMember(Name = "number_of_seasons")]
    [JsonPropertyName("number_of_seasons")]
    public int NumberOfSeasons { get; set; }

    /// <summary>
    /// Gets or sets the list of origin countries of the TV show.
    /// </summary>
    [DataMember(Name = "origin_country")]
    [JsonPropertyName("origin_country")]
    public List<string> OriginCountry { get; set; }

    /// <summary>
    /// Gets or sets the original language of the TV show.
    /// </summary>
    [DataMember(Name = "original_language")]
    [JsonPropertyName("original_language")]
    public string OriginalLanguage { get; set; }

    /// <summary>
    /// Gets or sets the original name of the TV show.
    /// </summary>
    [DataMember(Name = "original_name")]
    [JsonPropertyName("original_name")]
    public string OriginalName { get; set; }

    /// <summary>
    /// Gets or sets the overview or summary of the TV show.
    /// </summary>
    [DataMember(Name = "overview")]
    [JsonPropertyName("overview")]
    public string Overview { get; set; }

    /// <summary>
    /// Gets or sets the popularity score of the TV show.
    /// </summary>
    [DataMember(Name = "popularity")]
    [JsonPropertyName("popularity")]
    public double Popularity { get; set; }

    /// <summary>
    /// Gets or sets the URL path to the poster image.
    /// </summary>
    [DataMember(Name = "poster_path")]
    [JsonPropertyName("poster_path")]
    public string PosterPath { get; set; }

    /// <summary>
    /// Gets or sets the list of production companies involved in the TV show.
    /// </summary>
    [DataMember(Name = "production_companies")]
    [JsonPropertyName("production_companies")]
    public List<ProductionCompanyInfo> ProductionCompanies { get; set; }

    /// <summary>
    /// Gets or sets the list of seasons in the TV show.
    /// </summary>
    [DataMember(Name = "seasons")]
    [JsonPropertyName("seasons")]
    public List<Season> Seasons { get; set; }

    /// <summary>
    /// Gets or sets the list of keywords associated with the TV show.
    /// </summary>
    [DataMember(Name = "keywords")]
    public IReadOnlyCollection<Keyword> Keywords { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TVShow"/> class.
    /// </summary>
    public TVShow()
    {
        CreatedBy = [];
        EpisodeRunTime = [];
        Genres = [];
        Languages = [];
        Networks = [];
        OriginCountry = [];
        ProductionCompanies = [];
        Seasons = [];
        Keywords = [];
    }

    /// <summary>
    /// Returns a string representation of the TV show.
    /// </summary>
    /// <returns>A formatted string containing the name, first air date, and ID.</returns>
    public override string ToString()
        => $"{Name} ({FirstAirDate:yyyy-MM-dd}) [{Id}]";
}
