namespace FileFlows.DiscordNodes;

/// <summary>
/// THe Plugin settings
/// </summary>
public class PluginSettings:IPluginSettings
{
    /// <summary>
    /// Gets or sets the webhook id for this plugin
    /// </summary>
    [Text(1)]
    [Required]
    public string WebhookId { get; set; }

    /// <summary>
    /// Gets or sets the webhook token for this plugin
    /// </summary>
    [Text(2)]
    [Required]
    public string WebhookToken { get; set; }
}
