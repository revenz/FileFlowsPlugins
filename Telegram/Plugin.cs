namespace FileFlows.Telegram;

/// <summary>
/// A Telegram Plugin
/// </summary>
public class Plugin : FileFlows.Plugin.IPlugin
{
    /// <summary>
    /// Gets the UID for this plugin
    /// </summary>
    public Guid Uid => new Guid("a610837d-c6d6-438b-8470-33a407ea7c98");
    
    /// <summary>
    /// Gets the name of this plugin
    /// </summary>
    public string Name => "Telegram";
    
    /// <summary>
    /// Gets the minimum version of FileFlows required for this plugin
    /// </summary>
    public string MinimumVersion => "1.0.4.2019";

    /// <summary>
    /// Initializes this plugin
    /// </summary>
    public void Init()
    {
    }
}
