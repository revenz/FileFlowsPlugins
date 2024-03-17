namespace FileFlows.Gotify;

/// <summary>
/// A Gotify Plugin
/// </summary>
public class Plugin : FileFlows.Plugin.IPlugin
{
    /// <summary>
    /// Gets the UID for this plugin
    /// </summary>
    public Guid Uid => new Guid("3d8e13f2-819f-437f-b177-be40147c6e2b");
    
    /// <summary>
    /// Gets the name of this plugin
    /// </summary>
    public string Name => "Gotify Nodes";
    
    /// <summary>
    /// Gets the minimum version of FileFlows required for this plugin
    /// </summary>
    public string MinimumVersion => "1.0.4.2019";
    /// <inheritdoc />
    public string Icon => "svg:gotify";

    /// <summary>
    /// Initializes this plugin
    /// </summary>
    public void Init()
    {
    }
}
