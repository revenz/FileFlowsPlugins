using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace DM.MovieApi.MovieDb.Companies;

/// <summary>
/// Represents information about a production company.
/// </summary>
[DataContract]
public class ProductionCompanyInfo : IEqualityComparer<ProductionCompanyInfo>
{
    /// <summary>
    /// Gets or sets the ID of the production company.
    /// </summary>
    [DataMember(Name = "id")]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the production company.
    /// </summary>
    [DataMember(Name = "name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductionCompanyInfo"/> class.
    /// </summary>
    public ProductionCompanyInfo()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductionCompanyInfo"/> class with specified values.
    /// </summary>
    /// <param name="id">The ID of the production company.</param>
    /// <param name="name">The name of the production company.</param>
    public ProductionCompanyInfo(int id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (obj is not ProductionCompanyInfo info)
        {
            return false;
        }

        return Equals(this, info);
    }

    /// <inheritdoc/>
    public bool Equals(ProductionCompanyInfo x, ProductionCompanyInfo y)
        => x != null && y != null && x.Id == y.Id && x.Name == y.Name;

    /// <inheritdoc/>
    public override int GetHashCode()
        => GetHashCode(this);

    /// <inheritdoc/>
    public int GetHashCode(ProductionCompanyInfo obj)
    {
        unchecked // Overflow is fine, just wrap
        {
            int hash = 17;
            hash = hash * 23 + obj.Id.GetHashCode();
            hash = hash * 23 + (obj.Name?.GetHashCode() ?? 0);
            return hash;
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(Name) ? "n/a" : $"{Name} ({Id})";
    }
}
