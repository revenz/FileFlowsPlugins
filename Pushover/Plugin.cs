namespace FileFlows.Pushover;

/// <summary>
/// A Pushover Plugin
/// </summary>
public class Plugin : FileFlows.Plugin.IPlugin
{
    /// <summary>
    /// Gets the UID for this plugin
    /// </summary>
    public Guid Uid => new Guid("99cc0b7e-e470-4829-9a3b-30e8cc2ee749");
    
    /// <summary>
    /// Gets the name of this plugin
    /// </summary>
    public string Name => "Pushover";
    
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
