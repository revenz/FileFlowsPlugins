namespace FileFlows.Telegram;

/// <summary>
/// The plugin settings for this plugin
/// </summary>
public class PluginSettings : IPluginSettings
{
    /// <summary>
    /// Gets or sets the bot token
    /// </summary>
    [Text(1)]
    [Required]
    public string BotToken { get; set; }
    
    /// <summary>
    /// Gets or sets the chat ID
    /// </summary>
    [Text(1)]
    [Required]
    public string ChatId { get; set; }
}