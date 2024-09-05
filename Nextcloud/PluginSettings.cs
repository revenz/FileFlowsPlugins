namespace FileFlows.Nextcloud;

/// <summary>
/// The plugin settings for this plugin
/// </summary>
public class PluginSettings : IPluginSettings
{
    /// <summary>
    /// Gets or sets the next cloud URL
    /// </summary>
    [Text(1)]
    [Required]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username
    /// </summary>
    [Text(2)]
    [Required]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password
    /// </summary>
    [Text(3)]
    [Required]
    public string Password { get; set; } = string.Empty;
}
