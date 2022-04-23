namespace FileFlows.Plex.Models;

internal class PlexMedia
{
    public string RatingKey { get; set; }
    public int Id { get; set; }
    public PlexPart[] Part { get; set; }
}

internal class PlexPart
{
    public int Id { get; set; }
    public string Key { get; set; }
    public string File { get; set; }
}
