namespace FileFlows.Pushbullet;

/// <summary>
/// A Pushbullet Plugin
/// </summary>
public class Plugin : FileFlows.Plugin.IPlugin
{
    /// <summary>
    /// Gets the UID for this plugin
    /// </summary>
    public Guid Uid => new Guid("3016f2b9-41cb-45cf-b4f9-a8d9a30dc385");
    
    /// <summary>
    /// Gets the name of this plugin
    /// </summary>
    public string Name => "Pushbullet";
    
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
