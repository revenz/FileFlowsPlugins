namespace FileFlows.ImageNodes;

/// <summary>
/// Describes the plugin
/// </summary>
public class Plugin : FileFlows.Plugin.IPlugin
{
    /// <summary>
    /// Gets the UID of the plugin
    /// </summary>
    public Guid Uid => new Guid("008801f1-76fb-4316-bc45-b1beb284b76b");
    /// <summary>
    /// Gets the name of the plugin
    /// </summary>
    public string Name => "Site Scraping";
    /// <summary>
    /// Gets the minimum version of the FileFlows server this plugin must use
    /// </summary>
    public string MinimumVersion => "24.1.2.2019";

    /// <inheritdoc />
    public string Icon => "fas fa-globe-asia";

    /// <summary>
    /// Initializes the plugin
    /// </summary>
    public void Init()
    {
    }
}
