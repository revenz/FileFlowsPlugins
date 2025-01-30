using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace DM.MovieApi.MovieDb.Genres;

/// <summary>
/// Represents a genre in the Movie Database API.
/// </summary>
[DataContract]
public class Genre : IEqualityComparer<Genre>
{
    /// <summary>
    /// Gets or sets the unique identifier for the genre.
    /// </summary>
    [DataMember(Name = "id")]
    [JsonProperty("id")]
    public int Id { get; init; }

    /// <summary>
    /// Gets or sets the name of the genre.
    /// </summary>
    [DataMember(Name = "name")]
    [JsonProperty("name")]
    public string Name { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Genre"/> class.
    /// </summary>
    public Genre()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Genre"/> class with specified values.
    /// </summary>
    /// <param name="id">The unique identifier for the genre.</param>
    /// <param name="name">The name of the genre.</param>
    public Genre(int id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (obj is not Genre genre)
        {
            return false;
        }
        return Equals(this, genre);
    }

    /// <inheritdoc/>
    public bool Equals(Genre x, Genre y)
        => x != null && y != null && x.Id == y.Id && x.Name == y.Name;

    /// <inheritdoc/>
    public override int GetHashCode()
        => GetHashCode(this);

    /// <inheritdoc/>
    public int GetHashCode(Genre obj)
    {
        unchecked // Overflow is fine, just wrap
        {
            int hash = 17;
            hash = hash * 23 + obj.Id.GetHashCode();
            hash = hash * 23 + obj.Name.GetHashCode();
            return hash;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
        => $"{Name} ({Id})";
}