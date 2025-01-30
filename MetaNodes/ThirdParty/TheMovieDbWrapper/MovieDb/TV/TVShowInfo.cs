// ReSharper disable UnusedAutoPropertyAccessor.Local

using System.Runtime.Serialization;
using DM.MovieApi.MovieDb.Genres;

namespace DM.MovieApi.MovieDb.TV;

[DataContract]
public class TVShowInfo
{
    [DataMember( Name = "id" )]
    public int Id { get; init; }

    [DataMember( Name = "name" )]
    public string Name { get; init; }

    [DataMember( Name = "original_name" )]
    public string OriginalName { get; init; }

    [DataMember( Name = "poster_path" )]
    public string PosterPath { get; set; }

    [DataMember( Name = "backdrop_path" )]
    public string BackdropPath { get; set; }

    [DataMember( Name = "popularity" )]
    public double Popularity { get; init; }

    [DataMember( Name = "vote_average" )]
    public double VoteAverage { get; init; }

    [DataMember( Name = "vote_count" )]
    public int VoteCount { get; init; }

    [DataMember( Name = "overview" )]
    public string Overview { get; init; }

    [DataMember( Name = "first_air_date" )]
    public DateTime FirstAirDate { get; init; }

    [DataMember( Name = "origin_country" )]
    public List<string> OriginCountry { get; init; }

    [DataMember(Name = "genre_ids")] public List<int> GenreIds { get; set; } = [];
    public List<Genre> Genres { get; internal set; } = [];

    [DataMember(Name = "original_language")]
    public string OriginalLanguage { get; init; }

    public TVShowInfo()
    {
    }

    public override string ToString()
        => $"{Name} ({Id} - {FirstAirDate:yyyy-MM-dd})";
}
