namespace FileFlows.Web;

/// <summary>
/// Plugin Information
/// </summary>
public class Plugin : FileFlows.Plugin.IPlugin
{
    /// <inheritdoc />
    public Guid Uid => new Guid("162b4ed0-da61-42de-85e1-576b9d7a2f82");
    /// <inheritdoc />
    public string Name => "Web";
    /// <inheritdoc />
    public string MinimumVersion => "24.8.1.3444";
    /// <inheritdoc />
    public string Icon => "fas fa-globe";
    
    /// <inheritdoc />
    public void Init()
    {
    }
}
