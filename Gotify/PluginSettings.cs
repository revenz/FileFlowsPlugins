namespace FileFlows.Gotify;

/// <summary>
/// The plugin settings for this plugin
/// </summary>
public class PluginSettings : IPluginSettings
{
    /// <summary>
    /// Gets or sets the URL to the server to send messages to
    /// </summary>
    [Text(1)]
    [Required]
    public string ServerUrl { get; set; }

    /// <summary>
    /// Gets or sets the Access Token for the server
    /// </summary>
    [Text(2)]
    [Required]
    public string AccessToken { get; set; }
}
