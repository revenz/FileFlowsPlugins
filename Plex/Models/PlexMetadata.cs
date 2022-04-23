namespace FileFlows.Plex.Models;

internal class PlexMetadata
{
    public string RatingKey { get; set; }
    public string Key { get; set; }
    public PlexMedia[] Media { get; set; }
}
