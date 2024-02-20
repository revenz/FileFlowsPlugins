namespace FileFlows.Pushbullet;

/// <summary>
/// The plugin settings for this plugin
/// </summary>
public class PluginSettings : IPluginSettings
{
    /// <summary>
    /// Gets or sets the API Token 
    /// </summary>
    [Text(2)]
    [Required]
    public string ApiToken { get; set; }
}
