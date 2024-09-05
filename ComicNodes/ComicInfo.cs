namespace FileFlows.Comic;

/// <summary>
/// Represents information about a comic book.
/// </summary>
public class ComicInfo
{
    /// <summary>
    /// Gets or sets the title of the book.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the title of the series the book is part of.
    /// </summary>
    public string? Series { get; set; }
    
    /// <summary>
    /// Gets or sets a person or organization responsible for publishing, releasing, or issuing a resource.
    /// </summary>
    public string? Publisher { get; set; }

    /// <summary>
    /// Gets or sets the number of the book in the series.
    /// </summary>
    public int? Number { get; set; }

    /// <summary>
    /// Gets or sets the total number of books in the series.
    /// </summary>
    public int? Count { get; set; }

    /// <summary>
    /// Gets or sets the volume containing the book.
    /// </summary>
    public string? Volume { get; set; }

    /// <summary>
    /// Gets or sets the alternate series the book is part of.
    /// </summary>
    public string? AlternateSeries { get; set; }

    /// <summary>
    /// Gets or sets the alternate number of the book in the alternate series.
    /// </summary>
    public int? AlternateNumber { get; set; }

    /// <summary>
    /// Gets or sets the total number of books in the alternate series.
    /// </summary>
    public int? AlternateCount { get; set; }

    /// <summary>
    /// Gets or sets a description or summary of the book.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Gets or sets any additional notes about the comic.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the release date of the book.
    /// </summary>
    public DateTime? ReleaseDate { get; set; }
    
    /// <summary>
    /// Gets or sets optional tags for the comic book.
    /// </summary>
    public string[]? Tags { get; set; }
}

