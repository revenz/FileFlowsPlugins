namespace FileFlows.Plex;

/// <summary>
/// The Plex plugin settings
/// </summary>
public class PluginSettings:IPluginSettings
{
    /// <summary>
    /// Gets or sets the server URL
    /// </summary>
    [Text(1)]
    [Required]
    public string ServerUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the access token
    /// </summary>
    [Text(2)]
    [Required]
    public string AccessToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets if certificate errors should be ignored
    /// </summary>
    [Boolean(3)]
    public bool IgnoreCertificateErrors { get; set; } = false;

    /// <summary>
    /// Gets or sets the mappings
    /// </summary>
    [KeyValue(4, null)] 
    public List<KeyValuePair<string, string>> Mapping { get; set; } = new();
}