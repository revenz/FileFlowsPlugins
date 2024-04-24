namespace FileFlows.Pushover;

/// <summary>
/// The plugin settings for this plugin
/// </summary>
public class PluginSettings : IPluginSettings
{
    /// <summary>
    /// Gets or sets the user key to send the push over notification to
    /// </summary>
    [Text(1)]
    [Required]
    public string UserKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API Token 
    /// </summary>
    [Text(2)]
    [Required]
    public string ApiToken { get; set; } = string.Empty;
}
