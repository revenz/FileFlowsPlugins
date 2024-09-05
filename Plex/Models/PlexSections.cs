namespace FileFlows.Plex.Models;

internal class PlexSections
{
    public PlexSection? MediaContainer { get; set; }
}

internal class PlexSection
{
    public int Size { get; set; }
    public PlexDirectory[]? Directory { get; set; }
    public PlexMetadata[]? Metadata { get; set; }
}
