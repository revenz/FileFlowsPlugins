namespace FileFlows.DiscordNodes;

/// <summary>
/// The plugin information
/// </summary>
public class Plugin : FileFlows.Plugin.IPlugin
{
    /// <summary>
    /// Gets the UID of this plugin
    /// </summary>
    public Guid Uid => new Guid("ebaea108-8783-46b2-a889-be0d79bc8ad6");
    /// <summary>
    /// Gets the name of this plugin
    /// </summary>
    public string Name => "Discord";
    /// <summary>
    /// Gets the minimum version this plugin supports
    /// </summary>
    public string MinimumVersion => "1.0.4.2019";

    /// <inheritdoc />
    public string Icon => "fab fa-discord:#5865F2";

    /// <summary>
    /// Initializes this plugin
    /// </summary>
    public void Init()
    {
    }
}
