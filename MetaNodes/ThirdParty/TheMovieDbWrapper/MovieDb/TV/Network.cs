using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace DM.MovieApi.MovieDb.TV;

/// <summary>
/// Represents a television network.
/// </summary>
[DataContract]
public class Network : IEqualityComparer<Network>
{
    /// <summary>
    /// Gets or sets the unique identifier for the network.
    /// </summary>
    [DataMember(Name = "id")]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the network.
    /// </summary>
    [DataMember(Name = "name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Network"/> class.
    /// </summary>
    public Network() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Network"/> class with specified values.
    /// </summary>
    /// <param name="id">The unique identifier of the network.</param>
    /// <param name="name">The name of the network.</param>
    public Network(int id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <inheritdoc />
    public bool Equals(Network x, Network y)
        => x != null && y != null && x.Id == y.Id && x.Name == y.Name;

    /// <inheritdoc />
    public int GetHashCode(Network obj)
    {
        unchecked // Overflow is fine, just wrap
        {
            int hash = 17;
            hash = hash * 23 + obj.Id.GetHashCode();
            hash = hash * 23 + obj.Name.GetHashCode();
            return hash;
        }
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (obj is not Network network)
        {
            return false;
        }
        return Equals(this, network);
    }

    /// <inheritdoc />
    public override int GetHashCode()
        => GetHashCode(this);

    /// <summary>
    /// Returns a string representation of the network.
    /// </summary>
    /// <returns>A string containing the name and ID of the network.</returns>
    public override string ToString()
        => $"{Name} ({Id})";
}
