using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace DM.MovieApi.MovieDb.TV;

/// <summary>
/// Represents a TV show creator with an ID, name, and profile path.
/// </summary>
[DataContract]
public class TVShowCreator : IEqualityComparer<TVShowCreator>
{
    /// <summary>
    /// Gets or sets the unique identifier of the TV show creator.
    /// </summary>
    [DataMember(Name = "id")]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the TV show creator.
    /// </summary>
    [DataMember(Name = "name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the profile image path of the TV show creator.
    /// </summary>
    [DataMember(Name = "profile_path")]
    [JsonPropertyName("profile_path")]
    public string ProfilePath { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TVShowCreator"/> class.
    /// </summary>
    public TVShowCreator()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TVShowCreator"/> class with specified values.
    /// </summary>
    /// <param name="id">The unique identifier of the creator.</param>
    /// <param name="name">The name of the creator.</param>
    /// <param name="profilePath">The profile image path of the creator.</param>
    public TVShowCreator(int id, string name, string profilePath)
    {
        Id = id;
        Name = name;
        ProfilePath = profilePath;
    }

    /// <inheritdoc/>
    public bool Equals(TVShowCreator x, TVShowCreator y)
        => x != null && y != null && x.Id == y.Id && x.Name == y.Name;

    /// <inheritdoc/>
    public int GetHashCode(TVShowCreator obj)
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
    public override bool Equals(object obj)
    {
        if (obj is not TVShowCreator showCreator)
        {
            return false;
        }

        return Equals(this, showCreator);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
        => GetHashCode(this);

    /// <summary>
    /// Returns a string representation of the TV show creator.
    /// </summary>
    /// <returns>A string in the format "Name (Id)".</returns>
    public override string ToString()
        => $"{Name} ({Id})";
}
