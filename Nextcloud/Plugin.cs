namespace FileFlows.Nextcloud;

/// <summary>
/// Nextcloud plugin
/// </summary>
public class Plugin : FileFlows.Plugin.IPlugin
{
    /// <inheritdoc />
    public Guid Uid => new Guid("c9aff033-ae5b-45ad-81b0-8691c850242a");
    /// <inheritdoc />
    public string Name => "Nextcloud";
    /// <inheritdoc />
    public string MinimumVersion => "24.8.1.3444";
    /// <inheritdoc />
    public string Icon => "svg:nextcloud";
    
    /// <inheritdoc />
    public void Init()
    {
    }
}
